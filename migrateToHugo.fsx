#r "packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#r "packages/FSharp.Data/lib/net45/FSharp.Data.DesignTime.dll"

open FSharp.Data
open System
open System.IO

let filename = "C:/Work/blog/migrateToHugo/posts/06f314df-5ea2-41e2-9024-193077376e41.xml"

type BlogPost = XmlProvider<"C:/Work/blog/migrateToHugo/posts/06f314df-5ea2-41e2-9024-193077376e41.xml">
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

let header = createHeader blog
let content = "TODO"

let post = header + "\n" + content
let postname = sprintf "%s.md" blog.Slug
let outputDirectory = Path.Combine(__SOURCE_DIRECTORY__, "_posts")

Directory.CreateDirectory(outputDirectory)

File.WriteAllText(Path.Combine(outputDirectory, postname), post)

