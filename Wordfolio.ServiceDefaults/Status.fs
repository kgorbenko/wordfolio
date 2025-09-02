module Wordfolio.ServiceDefaults.Status

open System
open System.Reflection

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http

type ApplicationStatus =
    { Application: string
      Version: string
      Environment: string }

[<Literal>]
let StatusUrl = "/status"

let private getStatus(webHostEnvironment: IWebHostEnvironment) =
    let assembly =
        Assembly.GetEntryAssembly().GetName()

    { Application = assembly.Name
      Version = assembly.Version |> string
      Environment = webHostEnvironment.EnvironmentName }

let mapStatusEndpoint(app: WebApplication) =
    app
        .MapGet(StatusUrl, Func<IWebHostEnvironment, ApplicationStatus>(getStatus))
        .WithTags([| "Status" |])
    |> ignore

    app
