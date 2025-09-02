namespace Wordfolio.Api.Tests

open System.Net
open System.Threading.Tasks

open Xunit

type StatusTests() =
    [<Fact>]
    member _.AliveEndpointReturnsOk() : Task =
        task {
            use factory = new WebAppFactory()
            use client = factory.CreateClient()
            let! response = client.GetAsync("/status")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)
        }
