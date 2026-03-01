module Wordfolio.Api.DataAccess.EntriesHierarchy

open System
open System.Data
open System.Threading
open System.Threading.Tasks

open Microsoft.FSharp.Core.LanguagePrimitives

open Dapper
open Dapper.FSharp.PostgreSQL

open Wordfolio.Api.DataAccess.Dapper

type DefinitionHierarchy =
    { Definition: Definitions.Definition
      Examples: Examples.Example list }

type TranslationHierarchy =
    { Translation: Translations.Translation
      Examples: Examples.Example list }

type EntryHierarchy =
    { Entry: Entries.Entry
      Definitions: DefinitionHierarchy list
      Translations: TranslationHierarchy list }

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

let private assembleEntriesWithHierarchy
    (entries: seq<EntryRecord>)
    (definitions: seq<DefinitionRecord>)
    (translations: seq<TranslationRecord>)
    (examples: seq<ExampleRecord>)
    : EntryHierarchy list =
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

    let definitionsByEntry =
        definitions
        |> Seq.groupBy(fun d -> d.EntryId)
        |> Map.ofSeq

    let translationsByEntry =
        translations
        |> Seq.groupBy(fun t -> t.EntryId)
        |> Map.ofSeq

    entries
    |> Seq.map(fun entryRec ->
        let entry: Entries.Entry =
            { Id = entryRec.Id
              VocabularyId = entryRec.VocabularyId
              EntryText = entryRec.EntryText
              CreatedAt = entryRec.CreatedAt
              UpdatedAt = entryRec.UpdatedAt |> Option.ofNullable }

        let definitionsWithExamples =
            match definitionsByEntry.TryFind(entryRec.Id) with
            | Some defs ->
                defs
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
            | None -> []

        let translationsWithExamples =
            match translationsByEntry.TryFind(entryRec.Id) with
            | Some trans ->
                trans
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
            | None -> []

        { Entry = entry
          Definitions = definitionsWithExamples
          Translations = translationsWithExamples })
    |> Seq.toList

let private getEntryRecordByIdAsync
    (entryId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<EntryRecord option> =
    task {
        let entriesTable =
            table'<EntryRecord> Schema.EntriesTable.Name
            |> inSchema Schema.Name

        return!
            select {
                for e in entriesTable do
                    where(e.Id = entryId)
            }
            |> trySelectFirstAsync connection transaction cancellationToken
    }

let private getEntryRecordsByVocabularyIdAsync
    (vocabularyId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<EntryRecord list> =
    task {
        let entriesTable =
            table'<EntryRecord> Schema.EntriesTable.Name
            |> inSchema Schema.Name

        return!
            select {
                for e in entriesTable do
                    where(e.VocabularyId = vocabularyId)
                    orderByDescending e.CreatedAt
            }
            |> selectAsync connection transaction cancellationToken
    }

let private getDefinitionRecordsByEntryIdsAsync
    (entryIds: int list)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<DefinitionRecord list> =
    task {
        let definitionsTable =
            table'<DefinitionRecord> Schema.DefinitionsTable.Name
            |> inSchema Schema.Name

        if entryIds.IsEmpty then
            return []
        else
            return!
                select {
                    for d in definitionsTable do
                        where(isIn d.EntryId entryIds)
                        orderBy d.EntryId
                        thenBy d.DisplayOrder
                }
                |> selectAsync<DefinitionRecord> connection transaction cancellationToken
    }

let private getTranslationRecordsByEntryIdsAsync
    (entryIds: int list)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<TranslationRecord list> =
    task {
        let translationsTable =
            table'<TranslationRecord> Schema.TranslationsTable.Name
            |> inSchema Schema.Name

        if entryIds.IsEmpty then
            return []
        else
            return!
                select {
                    for t in translationsTable do
                        where(isIn t.EntryId entryIds)
                        orderBy t.EntryId
                        thenBy t.DisplayOrder
                }
                |> selectAsync<TranslationRecord> connection transaction cancellationToken
    }

let private getExampleRecordsByDefinitionIdsAsync
    (definitionIds: int list)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<ExampleRecord list> =
    task {
        if definitionIds.IsEmpty then
            return []
        else
            let sql =
                """
                SELECT "Id", "DefinitionId", "TranslationId", "ExampleText", "Source"
                FROM wordfolio."Examples"
                WHERE "DefinitionId" = ANY(@definitionIds)
                ORDER BY "Id";
                """

            let commandDefinition =
                CommandDefinition(
                    commandText = sql,
                    parameters = {| definitionIds = definitionIds |> List.toArray |},
                    transaction = transaction,
                    cancellationToken = cancellationToken
                )

            let! examples = connection.QueryAsync<ExampleRecord>(commandDefinition)

            return examples |> Seq.toList
    }

let private getExampleRecordsByTranslationIdsAsync
    (translationIds: int list)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<ExampleRecord list> =
    task {
        if translationIds.IsEmpty then
            return []
        else
            let sql =
                """
                SELECT "Id", "DefinitionId", "TranslationId", "ExampleText", "Source"
                FROM wordfolio."Examples"
                WHERE "TranslationId" = ANY(@translationIds)
                ORDER BY "Id";
                """

            let commandDefinition =
                CommandDefinition(
                    commandText = sql,
                    parameters = {| translationIds = translationIds |> List.toArray |},
                    transaction = transaction,
                    cancellationToken = cancellationToken
                )

            let! examples = connection.QueryAsync<ExampleRecord>(commandDefinition)

            return examples |> Seq.toList
    }

let getEntryByIdWithHierarchyAsync
    (entryId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<EntryHierarchy option> =
    task {
        let! entryRecord = getEntryRecordByIdAsync entryId connection transaction cancellationToken

        match entryRecord with
        | None -> return None
        | Some entryRecord ->
            let! definitions = getDefinitionRecordsByEntryIdsAsync [ entryId ] connection transaction cancellationToken

            let! translations =
                getTranslationRecordsByEntryIdsAsync [ entryId ] connection transaction cancellationToken

            let definitionIds =
                definitions
                |> List.map(fun definition -> definition.Id)

            let translationIds =
                translations
                |> List.map(fun translation -> translation.Id)

            let! definitionExamples =
                getExampleRecordsByDefinitionIdsAsync definitionIds connection transaction cancellationToken

            let! translationExamples =
                getExampleRecordsByTranslationIdsAsync translationIds connection transaction cancellationToken

            let examples =
                definitionExamples @ translationExamples

            let entries =
                assembleEntriesWithHierarchy [ entryRecord ] definitions translations examples

            return entries |> List.tryHead
    }

let clearEntryChildrenAsync
    (entryId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        let! deletedDefinitions =
            let definitionsTable =
                table'<DefinitionRecord> Schema.DefinitionsTable.Name
                |> inSchema Schema.Name

            delete {
                for d in definitionsTable do
                    where(d.EntryId = entryId)
            }
            |> deleteAsync connection transaction cancellationToken

        let! deletedTranslations =
            let translationsTable =
                table'<TranslationRecord> Schema.TranslationsTable.Name
                |> inSchema Schema.Name

            delete {
                for t in translationsTable do
                    where(t.EntryId = entryId)
            }
            |> deleteAsync connection transaction cancellationToken

        return deletedDefinitions + deletedTranslations
    }

let getEntriesHierarchyByVocabularyIdAsync
    (vocabularyId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<EntryHierarchy list> =
    task {
        let! entries = getEntryRecordsByVocabularyIdAsync vocabularyId connection transaction cancellationToken

        if entries.IsEmpty then
            return []
        else
            let entryIds =
                entries
                |> List.map(fun entry -> entry.Id)

            let! definitions = getDefinitionRecordsByEntryIdsAsync entryIds connection transaction cancellationToken

            let! translations = getTranslationRecordsByEntryIdsAsync entryIds connection transaction cancellationToken

            let definitionIds =
                definitions
                |> List.map(fun definition -> definition.Id)

            let translationIds =
                translations
                |> List.map(fun translation -> translation.Id)

            let! definitionExamples =
                getExampleRecordsByDefinitionIdsAsync definitionIds connection transaction cancellationToken

            let! translationExamples =
                getExampleRecordsByTranslationIdsAsync translationIds connection transaction cancellationToken

            let examples =
                definitionExamples @ translationExamples

            return assembleEntriesWithHierarchy entries definitions translations examples
    }
