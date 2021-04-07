#r  "nuget: FSharp.Data"
#r  "nuget: ReverseMarkdown, 3.16.0"

open FSharp.Data
open System
open System.IO
open ReverseMarkdown

type Block =
    | RawText of string
    | ItalicText of string
    | BoldText of string
    | Link of string * string // txt, url
    | Image of string * string // alt, url

type Language =
    | CSharp
    | FSharp
    | VB
    | Other

type Content =
    | Paragraph of Block list
    | Header of string
    | Code of Language * string
    | Problem of string

type BlogPost = XmlProvider<"/Users/mallibone/Downloads/mallibone-backup/fs\\site\\wwwroot\\posts\\6a84e03b-d7f0-4691-a67c-7caa7875f429.xml">

let originPath = "/Users/mallibone/Downloads/mallibone-backup/"

let getOriginBlogPosts =
    Directory.EnumerateFiles(originPath)
    |> Seq.filter (fun filename -> filename.Replace(originPath, "").StartsWith("fs\\site\\wwwroot\\posts\\") && filename.EndsWith(".xml"))

let getImages =
    let validImageFileEndings = [".png"; ".gif"; ".jpg"]
    Directory.EnumerateFiles(originPath)
    |> Seq.filter (fun filename -> 
        filename.Replace(originPath, "").StartsWith("fs\\site\\wwwroot\\posts\\") 
        && validImageFileEndings |> Seq.exists (fun fe -> filename.EndsWith(fe)))

let blog (filename:string) = BlogPost.Load(filename)

let formatTags (tags:string array) =
    tags
    |> Array.map (sprintf "\"%s\"")
    |> String.concat ", "

let createHeader (blog:BlogPost.Post) =
    [ "---";
        "layout: single";
        sprintf "title: \"%s\"" blog.Title;
        sprintf "title: %s" (blog.Title.Replace(":", "&#58;"));
        sprintf "date: %s" (blog.PubDate.ToString("yyyy-MM-dd"));
        sprintf "tags: [%s]" (formatTags blog.Categories);
        sprintf "excerpt: '%s'" blog.Excerpt;
        sprintf "slug: \"%s\"" blog.Slug;
        // sprintf "excerpt: \"%s\"" blog.Excerpt;
        "---" ]
    |> String.concat "\n"

let parseBlog outputDirectory blog =
    let header = createHeader blog

    let rawContent = (Converter()).Convert(blog.Content)

    let content = 
        rawContent.Replace("https://mallibone.com/posts/files/", "{{ site.url }}{{ site.baseurl }}/assets/images/")
                // .Replace("https://mallibone-blog.azurewebsites.net/posts/files/", "/assets/images/")
                .Replace("https://mallibone-blog.azurewebsites.net/posts/files/", "{{ site.url }}{{ site.baseurl }}/assets/images/")
    
    let post = header + "\n" + content

    let postname = $"""{blog.PubDate.ToString("yyyy-MM-dd")}-{blog.Slug.Replace("/", "-").Replace(":", "")}.md"""

    Directory.CreateDirectory(outputDirectory) |> ignore

    let file = Path.Combine(outputDirectory, postname)
    File.WriteAllText(file, post)
    file

let copyImage destinationDirectory source =
    Directory.CreateDirectory(path=destinationDirectory) |> ignore
    let filename = FileInfo(source).Name.Replace("fs\\site\\wwwroot\\posts\\files\\", "")
    let destination = Path.Combine(destinationDirectory, filename)
    File.Copy(source, destination, overwrite=true)
    destination

# time
getImages
|> Seq.map (copyImage <| Path.Combine(__SOURCE_DIRECTORY__, "images"))
|> Seq.toList

getOriginBlogPosts
|> Seq.map (blog >> (parseBlog (Path.Combine(__SOURCE_DIRECTORY__, "_posts"))))
|> Seq.toList
