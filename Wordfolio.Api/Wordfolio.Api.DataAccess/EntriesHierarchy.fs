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
type private HierarchyQueryResult =
    { EntryId: int
      VocabularyId: int
      EntryText: string
      EntryCreatedAt: DateTimeOffset
      EntryUpdatedAt: Nullable<DateTimeOffset>
      DefinitionId: Nullable<int>
      DefinitionText: string
      DefinitionSource: Nullable<int16>
      DefinitionDisplayOrder: Nullable<int>
      TranslationId: Nullable<int>
      TranslationText: string
      TranslationSource: Nullable<int16>
      TranslationDisplayOrder: Nullable<int>
      DefExampleId: Nullable<int>
      DefExampleText: string
      DefExampleSource: Nullable<int16>
      TransExampleId: Nullable<int>
      TransExampleText: string
      TransExampleSource: Nullable<int16> }

let getEntryByIdWithHierarchyAsync
    (entryId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<EntryWithHierarchy option> =
    task {
        let sql =
            """
            SELECT
                e."Id" as EntryId,
                e."VocabularyId" as VocabularyId,
                e."EntryText" as EntryText,
                e."CreatedAt" as EntryCreatedAt,
                e."UpdatedAt" as EntryUpdatedAt,
                d."Id" as DefinitionId,
                d."DefinitionText" as DefinitionText,
                d."Source" as DefinitionSource,
                d."DisplayOrder" as DefinitionDisplayOrder,
                t."Id" as TranslationId,
                t."TranslationText" as TranslationText,
                t."Source" as TranslationSource,
                t."DisplayOrder" as TranslationDisplayOrder,
                de."Id" as DefExampleId,
                de."ExampleText" as DefExampleText,
                de."Source" as DefExampleSource,
                te."Id" as TransExampleId,
                te."ExampleText" as TransExampleText,
                te."Source" as TransExampleSource
            FROM wordfolio."Entries" e
            LEFT JOIN wordfolio."Definitions" d ON d."EntryId" = e."Id"
            LEFT JOIN wordfolio."Translations" t ON t."EntryId" = e."Id"
            LEFT JOIN wordfolio."Examples" de ON de."DefinitionId" = d."Id"
            LEFT JOIN wordfolio."Examples" te ON te."TranslationId" = t."Id"
            WHERE e."Id" = @entryId
            ORDER BY d."DisplayOrder", t."DisplayOrder"
            """

        let! results =
            connection.QueryAsync<HierarchyQueryResult>(
                sql,
                {| entryId = entryId |},
                transaction = transaction,
                commandTimeout = Nullable()
            )

        let resultsList = results |> Seq.toList

        if resultsList.IsEmpty then
            return None
        else
            let firstResult = resultsList.[0]

            let entry: Entries.Entry =
                { Id = firstResult.EntryId
                  VocabularyId = firstResult.VocabularyId
                  EntryText = firstResult.EntryText
                  CreatedAt = firstResult.EntryCreatedAt
                  UpdatedAt =
                    if firstResult.EntryUpdatedAt.HasValue then
                        Some firstResult.EntryUpdatedAt.Value
                    else
                        None }

            // Group results by definition
            let definitionsMap =
                resultsList
                |> List.filter(fun r -> r.DefinitionId.HasValue)
                |> List.groupBy(fun r -> r.DefinitionId.Value)
                |> List.map(fun (defId, rows) ->
                    let firstRow = rows.[0]

                    let definition: Definitions.Definition =
                        { Id = defId
                          EntryId = firstResult.EntryId
                          DefinitionText = firstRow.DefinitionText
                          Source = EnumOfValue<int16, Definitions.DefinitionSource>(firstRow.DefinitionSource.Value)
                          DisplayOrder = firstRow.DefinitionDisplayOrder.Value }

                    let examples =
                        rows
                        |> List.filter(fun r -> r.DefExampleId.HasValue)
                        |> List.distinctBy(fun r -> r.DefExampleId.Value)
                        |> List.map(fun r ->
                            { Examples.Example.Id = r.DefExampleId.Value
                              Examples.Example.DefinitionId = Some defId
                              Examples.Example.TranslationId = None
                              Examples.Example.ExampleText = r.DefExampleText
                              Examples.Example.Source =
                                EnumOfValue<int16, Examples.ExampleSource>(r.DefExampleSource.Value) })

                    { Definition = definition
                      Examples = examples })
                |> List.sortBy(fun d -> d.Definition.DisplayOrder)

            // Group results by translation
            let translationsMap =
                resultsList
                |> List.filter(fun r -> r.TranslationId.HasValue)
                |> List.groupBy(fun r -> r.TranslationId.Value)
                |> List.map(fun (transId, rows) ->
                    let firstRow = rows.[0]

                    let translation: Translations.Translation =
                        { Id = transId
                          EntryId = firstResult.EntryId
                          TranslationText = firstRow.TranslationText
                          Source = EnumOfValue<int16, Translations.TranslationSource>(firstRow.TranslationSource.Value)
                          DisplayOrder = firstRow.TranslationDisplayOrder.Value }

                    let examples =
                        rows
                        |> List.filter(fun r -> r.TransExampleId.HasValue)
                        |> List.distinctBy(fun r -> r.TransExampleId.Value)
                        |> List.map(fun r ->
                            { Examples.Example.Id = r.TransExampleId.Value
                              Examples.Example.DefinitionId = None
                              Examples.Example.TranslationId = Some transId
                              Examples.Example.ExampleText = r.TransExampleText
                              Examples.Example.Source =
                                EnumOfValue<int16, Examples.ExampleSource>(r.TransExampleSource.Value) })

                    { Translation = translation
                      Examples = examples })
                |> List.sortBy(fun t -> t.Translation.DisplayOrder)

            return
                Some
                    { Entry = entry
                      Definitions = definitionsMap
                      Translations = translationsMap }
    }
