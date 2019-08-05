#r "packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#r "packages/FSharp.Data/lib/net45/FSharp.Data.DesignTime.dll"

open FSharp.Data
open System
open System.IO

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

type BlogPost = XmlProvider<"C:/Work/blog/migrateToHugo/posts/06f314df-5ea2-41e2-9024-193077376e41.xml">

let filename = "C:/Work/blog/migrateToHugo/posts/06f314df-5ea2-41e2-9024-193077376e41.xml"
let blog = BlogPost.Load(filename)

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


let paragraphTokens = ("<p>","</p>")
let codeTokens = ("<pre ","</pre>")
let headerTokens = ("<h2>","</h2>")

let (|BlockBetween|_|) (openToken:string,closeToken:string) (content:string) =
    if content.Length = 0 then None
    elif content.StartsWith openToken then
        let endIndex = content.IndexOf closeToken
        let block = content.Substring(0, endIndex + closeToken.Length)
        let rest = content.Substring(endIndex + closeToken.Length)
        Some(block,rest)
    else None

let rec pageComponents acc (txt:string) =
    match txt with
    | BlockBetween paragraphTokens (block,rest) -> 
        let blocks = parseParagraph block
        pageComponents ((Paragraph blocks) :: acc) rest
    | BlockBetween codeTokens (block,rest) -> 
        let code = parseCode block
        pageComponents (code :: acc) rest
    | BlockBetween headerTokens (block,rest) -> 
        let header = parseHeader block
        pageComponents (header :: acc) rest
    | Malformed ["<p>";"<pre ";"<h2>"] (block,rest) -> 
        pageComponents ((Problem block) :: acc) rest
    | _ -> acc |> List.rev 

let header = createHeader blog
let content = "TODO"

let post = header + "\n" + content
let postname = sprintf "%s.md" blog.Slug
let outputDirectory = Path.Combine(__SOURCE_DIRECTORY__, "_posts")

Directory.CreateDirectory(outputDirectory)

File.WriteAllText(Path.Combine(outputDirectory, postname), post)

blog.Content

