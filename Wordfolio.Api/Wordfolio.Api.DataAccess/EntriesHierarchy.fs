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

        // Read entry
        let! entryResults = reader.ReadAsync<EntryRecord>()

        let entryRecord =
            entryResults |> Seq.tryHead

        match entryRecord with
        | None -> return None
        | Some entryRec ->
            // Read definitions
            let! definitionResults = reader.ReadAsync<DefinitionRecord>()

            let definitions =
                definitionResults |> Seq.toList

            // Read translations
            let! translationResults = reader.ReadAsync<TranslationRecord>()

            let translations =
                translationResults |> Seq.toList

            // Read examples
            let! exampleResults = reader.ReadAsync<ExampleRecord>()
            let examples = exampleResults |> Seq.toList

            // Create maps for efficient lookup
            let definitionMap =
                definitions
                |> List.map(fun d -> (d.Id, d))
                |> Map.ofList

            let translationMap =
                translations
                |> List.map(fun t -> (t.Id, t))
                |> Map.ofList

            // Group examples by definition/translation
            let examplesByDefinition =
                examples
                |> List.filter(fun e -> e.DefinitionId.IsSome)
                |> List.groupBy(fun e -> e.DefinitionId.Value)
                |> Map.ofList

            let examplesByTranslation =
                examples
                |> List.filter(fun e -> e.TranslationId.IsSome)
                |> List.groupBy(fun e -> e.TranslationId.Value)
                |> Map.ofList

            // Build entry
            let entry: Entries.Entry =
                { Id = entryRec.Id
                  VocabularyId = entryRec.VocabularyId
                  EntryText = entryRec.EntryText
                  CreatedAt = entryRec.CreatedAt
                  UpdatedAt =
                    if entryRec.UpdatedAt.HasValue then
                        Some entryRec.UpdatedAt.Value
                    else
                        None }

            // Build definitions with examples
            let definitionsWithExamples =
                definitions
                |> List.map(fun defRec ->
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

            // Build translations with examples
            let translationsWithExamples =
                translations
                |> List.map(fun transRec ->
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

            return
                Some
                    { Entry = entry
                      Definitions = definitionsWithExamples
                      Translations = translationsWithExamples }
    }
