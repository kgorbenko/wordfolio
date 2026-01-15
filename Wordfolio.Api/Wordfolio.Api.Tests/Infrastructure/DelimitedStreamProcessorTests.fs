namespace Wordfolio.Api.Tests.Infrastructure

open System.Collections.Generic
open System.Threading
open System.Threading.Tasks

open FSharp.Control

open Xunit

open Wordfolio.Api.Infrastructure.DelimitedStreamProcessor

module DelimitedStreamProcessorTests =

    let private createAsyncEnumerable(chunks: string list) : IAsyncEnumerable<string> =
        taskSeq {
            for chunk in chunks do
                yield chunk
        }

    let private collectEvents(stream: IAsyncEnumerable<StreamEvent>) : Task<StreamEvent list> =
        task {
            let events = ResizeArray<StreamEvent>()

            for event in stream do
                events.Add(event)

            return events |> List.ofSeq
        }

    let private runProcessor (delimiter: string) (chunks: string list) : Task<StreamEvent list> =
        task {
            let stream = createAsyncEnumerable chunks

            let processed =
                processStream delimiter stream CancellationToken.None

            return! collectEvents processed
        }

    [<Fact>]
    let ``passes chunks through as-is until delimiter is found``() : Task =
        task {
            let chunks =
                [ "Hello "; "world"; "---JSON---"; "{}" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "Hello "; TextChunk "world"; ResultChunk "{}" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``buffers chunk ending with potential delimiter prefix``() : Task =
        task {
            let chunks = [ "Hello-"; "--JSON---result" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "Hello"; ResultChunk "result" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``handles delimiter split across two chunks``() : Task =
        task {
            let chunks =
                [ "Hello---JS"; "ON---{\"key\":\"value\"}" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "Hello"; ResultChunk "{\"key\":\"value\"}" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``handles delimiter split across multiple chunks``() : Task =
        task {
            let chunks =
                [ "Text---"; "JSON"; "---result" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "Text"; ResultChunk "result" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``handles delimiter in single chunk``() : Task =
        task {
            let chunks = [ "Before---JSON---After" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "Before"; ResultChunk "After" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``passes text through when no delimiter present``() : Task =
        task {
            let chunks = [ "Just "; "some "; "text" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "Just "; TextChunk "some "; TextChunk "text" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``handles only result after delimiter``() : Task =
        task {
            let chunks =
                [ "---JSON---"; "{\"result\": true}" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ ResultChunk "{\"result\": true}" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``handles empty chunks``() : Task =
        task {
            let chunks =
                [ "Hello"; ""; ""; "---JSON---"; ""; "{}" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "Hello"; ResultChunk "{}" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``handles whitespace result``() : Task =
        task {
            let chunks = [ "Text---JSON---"; "   " ]

            let! events = runProcessor "---JSON---" chunks

            let expected = [ TextChunk "Text" ]
            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``handles empty stream``() : Task =
        task {
            let chunks: string list = []

            let! actual = runProcessor "---JSON---" chunks

            let expected: StreamEvent list = []
            Assert.Equal<StreamEvent list>(expected, actual)
        }

    [<Fact>]
    let ``handles delimiter at very start``() : Task =
        task {
            let chunks = [ "---JSON---{\"data\": 123}" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ ResultChunk "{\"data\": 123}" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``handles delimiter at very end``() : Task =
        task {
            let chunks = [ "Some text here---JSON---" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "Some text here" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``handles multiline text content``() : Task =
        task {
            let chunks =
                [ "[verb] To run.\n"; "---JSON---"; "{}" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "[verb] To run.\n"; ResultChunk "{}" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``handles custom delimiter``() : Task =
        task {
            let chunks = [ "Before"; "|||"; "After" ]

            let! events = runProcessor "|||" chunks

            let expected =
                [ TextChunk "Before"; ResultChunk "After" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``handles single character chunks building delimiter``() : Task =
        task {
            let chunks =
                [ "A"
                  "B"
                  "C"
                  "-"
                  "-"
                  "-"
                  "J"
                  "S"
                  "O"
                  "N"
                  "-"
                  "-"
                  "-"
                  "X"
                  "Y"
                  "Z" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "A"; TextChunk "B"; TextChunk "C"; ResultChunk "XYZ" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``emits buffered prefix when delimiter does not complete``() : Task =
        task {
            let chunks =
                [ "Start-"; "--; not delimiter" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "Start"; TextChunk "---; not delimiter" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``handles chunk ending with single dash``() : Task =
        task {
            let chunks = [ "Text-"; "more text" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "Text"; TextChunk "-more text" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``passes through text not starting with delimiter char``() : Task =
        task {
            let chunks = [ "Hello "; "world "; "here" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "Hello "; TextChunk "world "; TextChunk "here" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``handles Unicode content``() : Task =
        task {
            let chunks =
                [ "Привет "; "мир"; "---JSON---"; "{\"greeting\": \"Привет\"}" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "Привет "
                  TextChunk "мир"
                  ResultChunk "{\"greeting\": \"Привет\"}" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``emits result as single chunk``() : Task =
        task {
            let chunks =
                [ "Text---JSON---"; "{\"a\":1}"; "{\"b\":2}" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "Text"; ResultChunk "{\"a\":1}{\"b\":2}" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``handles false positive delimiter prefix mid-chunk``() : Task =
        task {
            let chunks = [ "Hello---world" ]

            let! events = runProcessor "---JSON---" chunks

            let expected = [ TextChunk "Hello---world" ]
            Assert.Equal<StreamEvent list>(expected, events)
        }

    [<Fact>]
    let ``buffers only necessary characters at chunk boundary``() : Task =
        task {
            let chunks = [ "End---"; "JSON---start" ]

            let! events = runProcessor "---JSON---" chunks

            let expected =
                [ TextChunk "End"; ResultChunk "start" ]

            Assert.Equal<StreamEvent list>(expected, events)
        }
