module Wordfolio.Api.DataAccess.EntriesHierarchy

open System
open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper
open Microsoft.FSharp.Core.LanguagePrimitives

type DefinitionWithExamples =
    { Definition: Definitions.Definition
      Examples: Examples.Example list }

type TranslationWithExamples =
    { Translation: Translations.Translation
      Examples: Examples.Example list }

type EntryWithHierarchy =
    { Entry: Entries.Entry
      Definitions: DefinitionWithExamples list
      Translations: TranslationWithExamples list }

[<CLIMutable>]
type private EntryRecord =
    { Id: int
      VocabularyId: int
      EntryText: string
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset> }

[<CLIMutable>]
type private DefinitionRecord =
    { Id: int
      EntryId: int
      DefinitionText: string
      Source: int16
      DisplayOrder: int }

[<CLIMutable>]
type private TranslationRecord =
    { Id: int
      EntryId: int
      TranslationText: string
      Source: int16
      DisplayOrder: int }

[<CLIMutable>]
type private ExampleRecord =
    { Id: int
      DefinitionId: int option
      TranslationId: int option
      ExampleText: string
      Source: int16 }

let getEntryByIdWithHierarchyAsync
    (entryId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<EntryWithHierarchy option> =
    task {
        let sql =
            """
            SELECT "Id", "VocabularyId", "EntryText", "CreatedAt", "UpdatedAt"
            FROM wordfolio."Entries"
            WHERE "Id" = @entryId;

            SELECT "Id", "EntryId", "DefinitionText", "Source", "DisplayOrder"
            FROM wordfolio."Definitions"
            WHERE "EntryId" = @entryId
            ORDER BY "DisplayOrder";

            SELECT "Id", "EntryId", "TranslationText", "Source", "DisplayOrder"
            FROM wordfolio."Translations"
            WHERE "EntryId" = @entryId
            ORDER BY "DisplayOrder";

            SELECT "Id", "DefinitionId", "TranslationId", "ExampleText", "Source"
            FROM wordfolio."Examples"
            WHERE "DefinitionId" IN (SELECT "Id" FROM wordfolio."Definitions" WHERE "EntryId" = @entryId)
               OR "TranslationId" IN (SELECT "Id" FROM wordfolio."Translations" WHERE "EntryId" = @entryId);
            """

        let commandDefinition =
            CommandDefinition(
                commandText = sql,
                parameters = {| entryId = entryId |},
                transaction = transaction,
                cancellationToken = cancellationToken
            )

        use! reader = connection.QueryMultipleAsync(commandDefinition)

        let! entryRecord = reader.ReadFirstOrDefaultAsync<EntryRecord>()

        match box entryRecord with
        | null -> return None
        | _ ->
            let! definitions = reader.ReadAsync<DefinitionRecord>()
            let! translations = reader.ReadAsync<TranslationRecord>()
            let! examples = reader.ReadAsync<ExampleRecord>()

            let examplesByDefinition =
                examples
                |> Seq.choose(fun e ->
                    e.DefinitionId
                    |> Option.map(fun defId -> (defId, e)))
                |> Seq.groupBy fst
                |> Seq.map(fun (defId, pairs) -> (defId, pairs |> Seq.map snd |> Seq.toList))
                |> Map.ofSeq

            let examplesByTranslation =
                examples
                |> Seq.choose(fun e ->
                    e.TranslationId
                    |> Option.map(fun transId -> (transId, e)))
                |> Seq.groupBy fst
                |> Seq.map(fun (transId, pairs) -> (transId, pairs |> Seq.map snd |> Seq.toList))
                |> Map.ofSeq

            let entry: Entries.Entry =
                { Id = entryRecord.Id
                  VocabularyId = entryRecord.VocabularyId
                  EntryText = entryRecord.EntryText
                  CreatedAt = entryRecord.CreatedAt
                  UpdatedAt =
                    if entryRecord.UpdatedAt.HasValue then
                        Some entryRecord.UpdatedAt.Value
                    else
                        None }

            let definitionsWithExamples =
                definitions
                |> Seq.map(fun defRec ->
                    let definition: Definitions.Definition =
                        { Id = defRec.Id
                          EntryId = defRec.EntryId
                          DefinitionText = defRec.DefinitionText
                          Source = EnumOfValue<int16, Definitions.DefinitionSource>(defRec.Source)
                          DisplayOrder = defRec.DisplayOrder }

                    let exampleList =
                        match examplesByDefinition.TryFind(defRec.Id) with
                        | Some exs ->
                            exs
                            |> List.map(fun exRec ->
                                { Examples.Example.Id = exRec.Id
                                  Examples.Example.DefinitionId = Some defRec.Id
                                  Examples.Example.TranslationId = None
                                  Examples.Example.ExampleText = exRec.ExampleText
                                  Examples.Example.Source = EnumOfValue<int16, Examples.ExampleSource>(exRec.Source) })
                        | None -> []

                    { Definition = definition
                      Examples = exampleList })
                |> Seq.toList

            let translationsWithExamples =
                translations
                |> Seq.map(fun transRec ->
                    let translation: Translations.Translation =
                        { Id = transRec.Id
                          EntryId = transRec.EntryId
                          TranslationText = transRec.TranslationText
                          Source = EnumOfValue<int16, Translations.TranslationSource>(transRec.Source)
                          DisplayOrder = transRec.DisplayOrder }

                    let exampleList =
                        match examplesByTranslation.TryFind(transRec.Id) with
                        | Some exs ->
                            exs
                            |> List.map(fun exRec ->
                                { Examples.Example.Id = exRec.Id
                                  Examples.Example.DefinitionId = None
                                  Examples.Example.TranslationId = Some transRec.Id
                                  Examples.Example.ExampleText = exRec.ExampleText
                                  Examples.Example.Source = EnumOfValue<int16, Examples.ExampleSource>(exRec.Source) })
                        | None -> []

                    { Translation = translation
                      Examples = exampleList })
                |> Seq.toList

            return
                Some
                    { Entry = entry
                      Definitions = definitionsWithExamples
                      Translations = translationsWithExamples }
    }
