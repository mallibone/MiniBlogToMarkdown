#r  "nuget: FSharp.Data"
#r  "nuget: RevewrseMarkdown"

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

type BlogPost = XmlProvider<"/Users/mallibone/Downloads/mallibone-blog_202012051920/fs\\site\\wwwroot\\posts\\6a84e03b-d7f0-4691-a67c-7caa7875f429.xml">

let converter = Converter()
let originPath = "/Users/mallibone/Downloads/mallibone-blog_202012051920/"

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

let formatDate (date:DateTime) = date.ToString("yyyy-MM-dd")

let createHeader (blog:BlogPost.Post) =
    [ "---";
        sprintf "title: %s" blog.Title;
        sprintf "date: %s" (formatDate blog.PubDate) ;
        sprintf "tags: [%s]" (formatTags blog.Categories);
        sprintf "slug: %s" blog.Slug;
        "---" ]
    |> String.concat "\n"

let copyImage source =
    let destinationDirectory = Path.Combine(__SOURCE_DIRECTORY__, "images")
    Directory.CreateDirectory(path=destinationDirectory) |> ignore
    let filename = FileInfo(source).Name.Replace("fs\\site\\wwwroot\\posts\\files\\", "")
    let destination = Path.Combine(destinationDirectory, filename)
    File.Copy(source, destination, overwrite=true)
    destination


let parseBlog blog =
    let header = createHeader blog
    // let content = converter.Convert(blog.Content.Replace("<code", "<pre"))
    let content = converter.Convert(blog.Content)
    
    let post = header + "\n" + content

    let postname = $"""{blog.PubDate.ToString("yyyyMMdd")}_{blog.Slug.Replace("/", "-")}.md"""
    let outputDirectory = Path.Combine(__SOURCE_DIRECTORY__, "_posts")

    Directory.CreateDirectory(outputDirectory) |> ignore

    let file = Path.Combine(outputDirectory, postname)
    File.WriteAllText(file, post)
    file

# time
getImages
|> Seq.map copyImage
|> Seq.toList


getOriginBlogPosts
|> Seq.map (blog >> parseBlog)
|> Seq.toList


