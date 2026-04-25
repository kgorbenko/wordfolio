namespace Wordfolio.Api.Domain.Exercises

open System
open System.Text.Json

open Wordfolio.Api.Domain

module MultipleChoice =
    [<CLIMutable>]
    type OptionDto = { id: string; text: string }

    [<CLIMutable>]
    type PromptDto =
        { entryText: string
          options: OptionDto array
          correctOptionId: string }

    let private optionIds =
        [| "a"; "b"; "c"; "d" |]

    let private tryDeserialize(json: string) : PromptDto option =
        try
            let result =
                JsonSerializer.Deserialize<PromptDto>(json)

            if isNull(box result) then
                None
            else
                Some result
        with :? JsonException ->
            None

    let generatePrompt(entry: Entry) : GeneratedPrompt =
        let correctText =
            entry.Translations
            |> List.tryHead
            |> Option.map(fun t -> t.TranslationText)
            |> Option.defaultValue ""

        let distractors =
            (entry.Definitions
             |> List.map(fun d -> d.DefinitionText))
            @ (entry.Definitions
               |> List.collect(fun d ->
                   d.Examples
                   |> List.map(fun e -> e.ExampleText)))
            @ (entry.Translations
               |> List.collect(fun t ->
                   t.Examples
                   |> List.map(fun e -> e.ExampleText)))
            |> List.distinct
            |> List.filter(fun text ->
                not(String.IsNullOrEmpty text)
                && not(String.Equals(text, correctText, StringComparison.OrdinalIgnoreCase)))
            |> List.truncate 3

        let sortedTexts =
            (correctText :: distractors)
            |> List.sort

        let options =
            sortedTexts
            |> List.mapi(fun i text -> { id = optionIds[i]; text = text })
            |> Array.ofList

        let correctOptionId =
            options
            |> Array.tryFind(fun opt -> String.Equals(opt.text, correctText, StringComparison.OrdinalIgnoreCase))
            |> Option.map(fun opt -> opt.id)
            |> Option.defaultValue "a"

        let promptDto =
            { entryText = entry.EntryText
              options = options
              correctOptionId = correctOptionId }

        { PromptData = PromptData(JsonSerializer.Serialize promptDto)
          PromptSchemaVersion = 1s }

    let evaluate
        (promptSchemaVersion: int16)
        (promptData: PromptData)
        (rawAnswer: RawAnswer)
        : Result<bool, EvaluateError> =
        if promptSchemaVersion <> 1s then
            Error EvaluateError.UnsupportedPromptSchemaVersion
        else
            let (PromptData json) = promptData

            match tryDeserialize json with
            | None -> Error EvaluateError.MalformedPromptData
            | Some dto when isNull dto.correctOptionId -> Error EvaluateError.MalformedPromptData
            | Some dto ->
                let (RawAnswer answer) = rawAnswer

                Ok(String.Equals(dto.correctOptionId.Trim(), answer.Trim(), StringComparison.OrdinalIgnoreCase))

module Translation =
    [<CLIMutable>]
    type PromptDto =
        { entryText: string
          acceptedTranslations: string array }

    let private tryDeserialize(json: string) : PromptDto option =
        try
            let result =
                JsonSerializer.Deserialize<PromptDto>(json)

            if isNull(box result) then
                None
            else
                Some result
        with :? JsonException ->
            None

    let generatePrompt(entry: Entry) : GeneratedPrompt =
        let promptDto =
            { entryText = entry.EntryText
              acceptedTranslations =
                entry.Translations
                |> List.map(fun t -> t.TranslationText)
                |> Array.ofList }

        { PromptData = PromptData(JsonSerializer.Serialize promptDto)
          PromptSchemaVersion = 1s }

    let evaluate
        (promptSchemaVersion: int16)
        (promptData: PromptData)
        (rawAnswer: RawAnswer)
        : Result<bool, EvaluateError> =
        if promptSchemaVersion <> 1s then
            Error EvaluateError.UnsupportedPromptSchemaVersion
        else
            let (PromptData json) = promptData

            match tryDeserialize json with
            | None -> Error EvaluateError.MalformedPromptData
            | Some dto when isNull dto.acceptedTranslations -> Error EvaluateError.MalformedPromptData
            | Some dto ->
                let (RawAnswer answer) = rawAnswer

                let normalizedAnswer =
                    answer.Trim().ToLowerInvariant()

                let isCorrect =
                    dto.acceptedTranslations
                    |> Array.exists(fun t -> t.Trim().ToLowerInvariant() = normalizedAnswer)

                Ok isCorrect
