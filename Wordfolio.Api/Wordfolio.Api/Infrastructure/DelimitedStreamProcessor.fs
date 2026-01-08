module internal Wordfolio.Api.Infrastructure.DelimitedStreamProcessor

open System.Collections.Generic
open System.Text
open System.Threading

open FSharp.Control

type StreamEvent =
    | TextChunk of string
    | ResultChunk of string

let private findDelimiterPrefixLength (text: string) (delimiter: string) : int =
    let maxPrefixLength =
        min text.Length (delimiter.Length - 1)

    let rec findPrefix length =
        if length = 0 then
            0
        else
            let suffix =
                text.Substring(text.Length - length)

            let prefix = delimiter.Substring(0, length)

            if suffix = prefix then
                length
            else
                findPrefix(length - 1)

    findPrefix maxPrefixLength

let processStream
    (delimiter: string)
    (stream: IAsyncEnumerable<string>)
    (cancellationToken: CancellationToken)
    : TaskSeq<StreamEvent> =
    taskSeq {
        let buffer = StringBuilder()
        let mutable inResultPhase = false

        for chunk in stream do
            cancellationToken.ThrowIfCancellationRequested()

            if not(System.String.IsNullOrEmpty chunk) then
                buffer.Append(chunk) |> ignore

                if not inResultPhase then
                    let bufferText = buffer.ToString()

                    match bufferText.IndexOf(delimiter) with
                    | -1 ->
                        let prefixLength =
                            findDelimiterPrefixLength bufferText delimiter

                        let safeEnd =
                            bufferText.Length - prefixLength

                        if safeEnd > 0 then
                            yield TextChunk(bufferText.Substring(0, safeEnd))

                            buffer.Clear() |> ignore

                            if prefixLength > 0 then
                                buffer.Append(bufferText.Substring(safeEnd))
                                |> ignore
                    | idx ->
                        inResultPhase <- true

                        if idx > 0 then
                            yield TextChunk(bufferText.Substring(0, idx))

                        buffer.Clear().Append(bufferText.Substring(idx + delimiter.Length))
                        |> ignore

        let remaining = buffer.ToString()
        let trimmedRemaining = remaining.Trim()

        if trimmedRemaining.Length > 0 then
            if inResultPhase then
                yield ResultChunk trimmedRemaining
            else
                yield TextChunk remaining
    }
