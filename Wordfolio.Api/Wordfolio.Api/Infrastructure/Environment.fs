module Wordfolio.Api.Infrastructure.Environment

open System
open System.Data
open System.Data.Common
open System.Threading
open System.Threading.Tasks

open Npgsql

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.CollectionsHierarchy
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Vocabularies

module DataAccess =
    open Wordfolio.Api.DataAccess

    type Collection = Wordfolio.Api.DataAccess.Collections.Collection
    type CreateCollectionParameters = Wordfolio.Api.DataAccess.Collections.CreateCollectionParameters
    type UpdateCollectionParameters = Wordfolio.Api.DataAccess.Collections.UpdateCollectionParameters
    type Vocabulary = Wordfolio.Api.DataAccess.Vocabularies.Vocabulary
    type VocabularyCreationParameters = Wordfolio.Api.DataAccess.Vocabularies.VocabularyCreationParameters
    type VocabularyUpdateParameters = Wordfolio.Api.DataAccess.Vocabularies.VocabularyUpdateParameters
    type Entry = Wordfolio.Api.DataAccess.Entries.Entry
    type EntryCreationParameters = Wordfolio.Api.DataAccess.Entries.EntryCreationParameters
    type Definition = Wordfolio.Api.DataAccess.Definitions.Definition
    type DefinitionCreationParameters = Wordfolio.Api.DataAccess.Definitions.DefinitionCreationParameters
    type Translation = Wordfolio.Api.DataAccess.Translations.Translation
    type TranslationCreationParameters = Wordfolio.Api.DataAccess.Translations.TranslationCreationParameters
    type Example = Wordfolio.Api.DataAccess.Examples.Example
    type ExampleCreationParameters = Wordfolio.Api.DataAccess.Examples.ExampleCreationParameters

type AppEnv(connection: IDbConnection, transaction: IDbTransaction, cancellationToken: CancellationToken) =

    let toCollectionDomain(c: DataAccess.Collection) : Collection =
        { Id = CollectionId c.Id
          UserId = UserId c.UserId
          Name = c.Name
          Description = c.Description
          CreatedAt = c.CreatedAt
          UpdatedAt = c.UpdatedAt }

    let toVocabularyDomain(v: DataAccess.Vocabulary) : Vocabulary =
        { Id = VocabularyId v.Id
          CollectionId = CollectionId v.CollectionId
          Name = v.Name
          Description = v.Description
          CreatedAt = v.CreatedAt
          UpdatedAt = v.UpdatedAt }

    let toDefinitionSource(source: Wordfolio.Api.DataAccess.Definitions.DefinitionSource) : DefinitionSource =
        match source with
        | Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Api -> DefinitionSource.Api
        | Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual -> DefinitionSource.Manual
        | _ -> DefinitionSource.Manual

    let toTranslationSource(source: Wordfolio.Api.DataAccess.Translations.TranslationSource) : TranslationSource =
        match source with
        | Wordfolio.Api.DataAccess.Translations.TranslationSource.Api -> TranslationSource.Api
        | Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual -> TranslationSource.Manual
        | _ -> TranslationSource.Manual

    let toExampleSource(source: Wordfolio.Api.DataAccess.Examples.ExampleSource) : ExampleSource =
        match source with
        | Wordfolio.Api.DataAccess.Examples.ExampleSource.Api -> ExampleSource.Api
        | Wordfolio.Api.DataAccess.Examples.ExampleSource.Custom -> ExampleSource.Custom
        | _ -> ExampleSource.Custom

    let fromDefinitionSource(source: DefinitionSource) : Wordfolio.Api.DataAccess.Definitions.DefinitionSource =
        match source with
        | DefinitionSource.Api -> Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Api
        | DefinitionSource.Manual -> Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual

    let fromTranslationSource(source: TranslationSource) : Wordfolio.Api.DataAccess.Translations.TranslationSource =
        match source with
        | TranslationSource.Api -> Wordfolio.Api.DataAccess.Translations.TranslationSource.Api
        | TranslationSource.Manual -> Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual

    let fromExampleSource(source: ExampleSource) : Wordfolio.Api.DataAccess.Examples.ExampleSource =
        match source with
        | ExampleSource.Api -> Wordfolio.Api.DataAccess.Examples.ExampleSource.Api
        | ExampleSource.Custom -> Wordfolio.Api.DataAccess.Examples.ExampleSource.Custom

    let toExampleDomain(e: DataAccess.Example) : Example =
        { Id = ExampleId e.Id
          ExampleText = e.ExampleText
          Source = toExampleSource e.Source }

    let toDefinitionDomain(d: DataAccess.Definition, examples: Example list) : Definition =
        { Id = DefinitionId d.Id
          DefinitionText = d.DefinitionText
          Source = toDefinitionSource d.Source
          DisplayOrder = d.DisplayOrder
          Examples = examples }

    let toTranslationDomain(t: DataAccess.Translation, examples: Example list) : Translation =
        { Id = TranslationId t.Id
          TranslationText = t.TranslationText
          Source = toTranslationSource t.Source
          DisplayOrder = t.DisplayOrder
          Examples = examples }

    let toEntryDomain(e: DataAccess.Entry, definitions: Definition list, translations: Translation list) : Entry =
        { Id = EntryId e.Id
          VocabularyId = VocabularyId e.VocabularyId
          EntryText = e.EntryText
          CreatedAt = e.CreatedAt
          UpdatedAt = e.UpdatedAt
          Definitions = definitions
          Translations = translations }

    let toVocabularyWithEntryCountDomain
        (v: Wordfolio.Api.DataAccess.CollectionsHierarchy.VocabularySummary)
        : Wordfolio.Api.Domain.CollectionsHierarchy.VocabularyWithEntryCount =
        { Id = VocabularyId v.Id
          Name = v.Name
          Description = v.Description
          CreatedAt = v.CreatedAt
          UpdatedAt = v.UpdatedAt
          EntryCount = v.EntryCount }

    let toCollectionWithVocabulariesDomain
        (c: Wordfolio.Api.DataAccess.CollectionsHierarchy.CollectionSummary)
        : Wordfolio.Api.Domain.CollectionsHierarchy.CollectionWithVocabularies =
        { Id = CollectionId c.Id
          Name = c.Name
          Description = c.Description
          CreatedAt = c.CreatedAt
          UpdatedAt = c.UpdatedAt
          Vocabularies =
            c.Vocabularies
            |> List.map toVocabularyWithEntryCountDomain }

    let toCollectionOverviewDomain
        (c: Wordfolio.Api.DataAccess.CollectionsHierarchy.CollectionOverview)
        : Wordfolio.Api.Domain.CollectionsHierarchy.CollectionWithVocabularyCount =
        { Id = CollectionId c.Id
          Name = c.Name
          Description = c.Description
          CreatedAt = c.CreatedAt
          UpdatedAt = c.UpdatedAt
          VocabularyCount = c.VocabularyCount }

    let toCollectionSortByDataAccess(sortBy: CollectionSortBy) =
        match sortBy with
        | CollectionSortBy.Name -> Wordfolio.Api.DataAccess.CollectionsHierarchy.CollectionSortBy.Name
        | CollectionSortBy.CreatedAt -> Wordfolio.Api.DataAccess.CollectionsHierarchy.CollectionSortBy.CreatedAt
        | CollectionSortBy.UpdatedAt -> Wordfolio.Api.DataAccess.CollectionsHierarchy.CollectionSortBy.UpdatedAt
        | CollectionSortBy.VocabularyCount ->
            Wordfolio.Api.DataAccess.CollectionsHierarchy.CollectionSortBy.VocabularyCount

    let toSortDirectionDataAccess(sortDirection: SortDirection) =
        match sortDirection with
        | SortDirection.Asc -> Wordfolio.Api.DataAccess.CollectionsHierarchy.SortDirection.Asc
        | SortDirection.Desc -> Wordfolio.Api.DataAccess.CollectionsHierarchy.SortDirection.Desc

    let toSearchUserCollectionsQueryDataAccess
        (query: SearchUserCollectionsQuery)
        : Wordfolio.Api.DataAccess.CollectionsHierarchy.SearchUserCollectionsQuery =
        { Search = query.Search
          SortBy = toCollectionSortByDataAccess query.SortBy
          SortDirection = toSortDirectionDataAccess query.SortDirection }

    let toVocabularySortByDataAccess(sortBy: VocabularySortBy) =
        match sortBy with
        | VocabularySortBy.Name -> Wordfolio.Api.DataAccess.CollectionsHierarchy.VocabularySummarySortBy.Name
        | VocabularySortBy.CreatedAt -> Wordfolio.Api.DataAccess.CollectionsHierarchy.VocabularySummarySortBy.CreatedAt
        | VocabularySortBy.UpdatedAt -> Wordfolio.Api.DataAccess.CollectionsHierarchy.VocabularySummarySortBy.UpdatedAt
        | VocabularySortBy.EntryCount ->
            Wordfolio.Api.DataAccess.CollectionsHierarchy.VocabularySummarySortBy.EntryCount

    let toSearchCollectionVocabulariesQueryDataAccess
        (query: SearchCollectionVocabulariesQuery)
        : Wordfolio.Api.DataAccess.CollectionsHierarchy.VocabularySummaryQuery =
        { Search = query.Search
          SortBy = toVocabularySortByDataAccess query.SortBy
          SortDirection = toSortDirectionDataAccess query.SortDirection }

    interface IGetCollectionById with
        member _.GetCollectionById(CollectionId id) =
            task {
                let! result =
                    Wordfolio.Api.DataAccess.Collections.getCollectionByIdAsync
                        id
                        connection
                        transaction
                        cancellationToken

                return result |> Option.map toCollectionDomain
            }

    interface IGetCollectionsByUserId with
        member _.GetCollectionsByUserId(UserId userId) =
            task {
                let! results =
                    Wordfolio.Api.DataAccess.Collections.getCollectionsByUserIdAsync
                        userId
                        connection
                        transaction
                        cancellationToken

                return results |> List.map toCollectionDomain
            }

    interface ICreateCollection with
        member _.CreateCollection(parameters: CreateCollectionData) =
            task {
                let parameters: DataAccess.CreateCollectionParameters =
                    { UserId = UserId.value parameters.UserId
                      Name = parameters.Name
                      Description = parameters.Description
                      CreatedAt = parameters.CreatedAt }

                let! id =
                    Wordfolio.Api.DataAccess.Collections.createCollectionAsync
                        parameters
                        connection
                        transaction
                        cancellationToken

                return CollectionId id
            }

    interface IUpdateCollection with
        member _.UpdateCollection(parameters: UpdateCollectionData) =
            task {
                let parameters: DataAccess.UpdateCollectionParameters =
                    { Id = CollectionId.value parameters.CollectionId
                      Name = parameters.Name
                      Description = parameters.Description
                      UpdatedAt = parameters.UpdatedAt }

                return!
                    Wordfolio.Api.DataAccess.Collections.updateCollectionAsync
                        parameters
                        connection
                        transaction
                        cancellationToken
            }

    interface IDeleteCollection with
        member _.DeleteCollection(CollectionId id) =
            task {
                return!
                    Wordfolio.Api.DataAccess.Collections.deleteCollectionAsync
                        id
                        connection
                        transaction
                        cancellationToken
            }

    interface IGetVocabularyById with
        member _.GetVocabularyById(VocabularyId id) =
            task {
                let! result =
                    Wordfolio.Api.DataAccess.Vocabularies.getVocabularyByIdAsync
                        id
                        connection
                        transaction
                        cancellationToken

                return result |> Option.map toVocabularyDomain
            }

    interface IGetVocabulariesByCollectionId with
        member _.GetVocabulariesByCollectionId(CollectionId collectionId) =
            task {
                let! results =
                    Wordfolio.Api.DataAccess.Vocabularies.getVocabulariesByCollectionIdAsync
                        collectionId
                        connection
                        transaction
                        cancellationToken

                return results |> List.map toVocabularyDomain
            }

    interface ICreateVocabulary with
        member _.CreateVocabulary(data: CreateVocabularyData) =
            task {
                let parameters: DataAccess.VocabularyCreationParameters =
                    { CollectionId = CollectionId.value data.CollectionId
                      Name = data.Name
                      Description = data.Description
                      CreatedAt = data.CreatedAt }

                let! id =
                    Wordfolio.Api.DataAccess.Vocabularies.createVocabularyAsync
                        parameters
                        connection
                        transaction
                        cancellationToken

                return VocabularyId id
            }

    interface IUpdateVocabulary with
        member _.UpdateVocabulary(data: UpdateVocabularyData) =
            task {
                let parameters: DataAccess.VocabularyUpdateParameters =
                    { Id = VocabularyId.value data.VocabularyId
                      Name = data.Name
                      Description = data.Description
                      UpdatedAt = data.UpdatedAt }

                return!
                    Wordfolio.Api.DataAccess.Vocabularies.updateVocabularyAsync
                        parameters
                        connection
                        transaction
                        cancellationToken
            }

    interface IDeleteVocabulary with
        member _.DeleteVocabulary(VocabularyId id) =
            task {
                return!
                    Wordfolio.Api.DataAccess.Vocabularies.deleteVocabularyAsync
                        id
                        connection
                        transaction
                        cancellationToken
            }

    interface IGetDefaultVocabulary with
        member _.GetDefaultVocabulary(UserId userId) =
            task {
                let! result =
                    Wordfolio.Api.DataAccess.Vocabularies.getDefaultVocabularyByUserIdAsync
                        userId
                        connection
                        transaction
                        cancellationToken

                return result |> Option.map toVocabularyDomain
            }

    interface ICreateDefaultVocabulary with
        member _.CreateDefaultVocabulary(parameters: CreateDefaultVocabularyParameters) =
            task {
                let dataAccessParams: DataAccess.VocabularyCreationParameters =
                    { CollectionId = CollectionId.value parameters.CollectionId
                      Name = parameters.Name
                      Description = parameters.Description
                      CreatedAt = parameters.CreatedAt }

                let! id =
                    Wordfolio.Api.DataAccess.Vocabularies.createDefaultVocabularyAsync
                        dataAccessParams
                        connection
                        transaction
                        cancellationToken

                return VocabularyId id
            }

    interface IGetDefaultCollection with
        member _.GetDefaultCollection(UserId userId) =
            task {
                let! result =
                    Wordfolio.Api.DataAccess.Collections.getDefaultCollectionByUserIdAsync
                        userId
                        connection
                        transaction
                        cancellationToken

                return result |> Option.map toCollectionDomain
            }

    interface ICreateDefaultCollection with
        member _.CreateDefaultCollection(parameters: CreateDefaultCollectionParameters) =
            task {
                let dataAccessParams: DataAccess.CreateCollectionParameters =
                    { UserId = UserId.value parameters.UserId
                      Name = parameters.Name
                      Description = parameters.Description
                      CreatedAt = parameters.CreatedAt }

                let! id =
                    Wordfolio.Api.DataAccess.Collections.createDefaultCollectionAsync
                        dataAccessParams
                        connection
                        transaction
                        cancellationToken

                return CollectionId id
            }

    interface IGetEntryById with
        member _.GetEntryById(EntryId id) =
            task {
                let! maybeEntryWithHierarchy =
                    Wordfolio.Api.DataAccess.EntriesHierarchy.getEntryByIdWithHierarchyAsync
                        id
                        connection
                        transaction
                        cancellationToken

                match maybeEntryWithHierarchy with
                | None -> return None
                | Some entryWithHierarchy ->
                    let definitionsWithExamples =
                        entryWithHierarchy.Definitions
                        |> List.map(fun dwithEx ->
                            let examples =
                                dwithEx.Examples
                                |> List.map toExampleDomain

                            toDefinitionDomain(dwithEx.Definition, examples))

                    let translationsWithExamples =
                        entryWithHierarchy.Translations
                        |> List.map(fun twithEx ->
                            let examples =
                                twithEx.Examples
                                |> List.map toExampleDomain

                            toTranslationDomain(twithEx.Translation, examples))

                    return
                        Some(toEntryDomain(entryWithHierarchy.Entry, definitionsWithExamples, translationsWithExamples))
            }

    interface IGetEntryByTextAndVocabularyId with
        member _.GetEntryByTextAndVocabularyId(data: GetEntryByTextAndVocabularyIdData) =
            task {
                let! maybeEntry =
                    Wordfolio.Api.DataAccess.Entries.getEntryByTextAndVocabularyIdAsync
                        (VocabularyId.value data.VocabularyId)
                        data.EntryText
                        connection
                        transaction
                        cancellationToken

                return
                    maybeEntry
                    |> Option.map(fun entry -> toEntryDomain(entry, [], []))
            }

    interface ICreateEntry with
        member _.CreateEntry(data: CreateEntryData) =
            task {
                let parameters: DataAccess.EntryCreationParameters =
                    { VocabularyId = VocabularyId.value data.VocabularyId
                      EntryText = data.EntryText
                      CreatedAt = data.CreatedAt }

                let! entryId =
                    Wordfolio.Api.DataAccess.Entries.createEntryAsync
                        parameters
                        connection
                        transaction
                        cancellationToken

                return EntryId entryId
            }

    interface ICreateDefinition with
        member _.CreateDefinition(data: CreateDefinitionData) =
            task {
                let parameters: DataAccess.DefinitionCreationParameters =
                    { EntryId = EntryId.value data.EntryId
                      DefinitionText = data.Text
                      Source = fromDefinitionSource data.Source
                      DisplayOrder = data.DisplayOrder }

                let! ids =
                    Wordfolio.Api.DataAccess.Definitions.createDefinitionsAsync
                        [ parameters ]
                        connection
                        transaction
                        cancellationToken

                return DefinitionId ids.[0]
            }

    interface ICreateTranslation with
        member _.CreateTranslation(data: CreateTranslationData) =
            task {
                let parameters: DataAccess.TranslationCreationParameters =
                    { EntryId = EntryId.value data.EntryId
                      TranslationText = data.Text
                      Source = fromTranslationSource data.Source
                      DisplayOrder = data.DisplayOrder }

                let! ids =
                    Wordfolio.Api.DataAccess.Translations.createTranslationsAsync
                        [ parameters ]
                        connection
                        transaction
                        cancellationToken

                return TranslationId ids.[0]
            }

    interface ICreateExamplesForDefinition with
        member _.CreateExamplesForDefinition(data: CreateExamplesForDefinitionData) =
            task {
                let parameters: DataAccess.ExampleCreationParameters list =
                    data.Examples
                    |> List.map(fun ex ->
                        { DefinitionId = Some(DefinitionId.value data.DefinitionId)
                          TranslationId = None
                          ExampleText = ex.ExampleText
                          Source = fromExampleSource ex.Source })

                let! _ =
                    Wordfolio.Api.DataAccess.Examples.createExamplesAsync
                        parameters
                        connection
                        transaction
                        cancellationToken

                return ()
            }

    interface ICreateExamplesForTranslation with
        member _.CreateExamplesForTranslation(data: CreateExamplesForTranslationData) =
            task {
                let parameters: DataAccess.ExampleCreationParameters list =
                    data.Examples
                    |> List.map(fun ex ->
                        { DefinitionId = None
                          TranslationId = Some(TranslationId.value data.TranslationId)
                          ExampleText = ex.ExampleText
                          Source = fromExampleSource ex.Source })

                let! _ =
                    Wordfolio.Api.DataAccess.Examples.createExamplesAsync
                        parameters
                        connection
                        transaction
                        cancellationToken

                return ()
            }

    interface IUpdateEntry with
        member _.UpdateEntry(data: UpdateEntryData) =
            task {
                let parameters: Wordfolio.Api.DataAccess.Entries.EntryUpdateParameters =
                    { Id = EntryId.value data.EntryId
                      EntryText = data.EntryText
                      UpdatedAt = data.UpdatedAt }

                let! _ =
                    Wordfolio.Api.DataAccess.Entries.updateEntryAsync
                        parameters
                        connection
                        transaction
                        cancellationToken

                return ()
            }

    interface IMoveEntry with
        member _.MoveEntry(data: MoveEntryData) =
            task {
                let parameters: Wordfolio.Api.DataAccess.Entries.EntryMoveParameters =
                    { Id = EntryId.value data.EntryId
                      OldVocabularyId = VocabularyId.value data.OldVocabularyId
                      NewVocabularyId = VocabularyId.value data.NewVocabularyId
                      UpdatedAt = data.UpdatedAt }

                let! _ =
                    Wordfolio.Api.DataAccess.Entries.moveEntryAsync parameters connection transaction cancellationToken

                return ()
            }

    interface IClearEntryChildren with
        member _.ClearEntryChildren(EntryId id) =
            task {
                let! _ =
                    Wordfolio.Api.DataAccess.EntriesHierarchy.clearEntryChildrenAsync
                        id
                        connection
                        transaction
                        cancellationToken

                return ()
            }

    interface IHasVocabularyAccess with
        member _.HasVocabularyAccess(data: HasVocabularyAccessData) =
            task {
                let! hasAccess =
                    Wordfolio.Api.DataAccess.Entries.hasVocabularyAccessAsync
                        (VocabularyId.value data.VocabularyId)
                        (UserId.value data.UserId)
                        connection
                        transaction
                        cancellationToken

                return hasAccess
            }

    interface IHasVocabularyAccessInCollection with
        member _.HasVocabularyAccessInCollection(data: HasVocabularyAccessInCollectionData) =
            task {
                let! hasAccess =
                    Wordfolio.Api.DataAccess.Entries.hasVocabularyAccessInCollectionAsync
                        (VocabularyId.value data.VocabularyId)
                        (CollectionId.value data.CollectionId)
                        (UserId.value data.UserId)
                        connection
                        transaction
                        cancellationToken

                return hasAccess
            }

    interface IDeleteEntry with
        member _.DeleteEntry(EntryId id) =
            task {
                return! Wordfolio.Api.DataAccess.Entries.deleteEntryAsync id connection transaction cancellationToken
            }

    interface IGetCollectionsWithVocabularies with
        member _.GetCollectionsWithVocabularies(UserId userId) =
            task {
                let! results =
                    Wordfolio.Api.DataAccess.CollectionsHierarchy.getCollectionsByUserIdAsync
                        userId
                        connection
                        transaction
                        cancellationToken

                return
                    results
                    |> List.map toCollectionWithVocabulariesDomain
            }

    interface ISearchUserCollections with
        member _.SearchUserCollections(data: SearchUserCollectionsData) =
            task {
                let! results =
                    Wordfolio.Api.DataAccess.CollectionsHierarchy.searchUserCollectionsAsync
                        (UserId.value data.UserId)
                        (data.Query
                         |> toSearchUserCollectionsQueryDataAccess)
                        connection
                        transaction
                        cancellationToken

                return
                    results
                    |> List.map toCollectionOverviewDomain
            }

    interface ISearchCollectionVocabularies with
        member _.SearchCollectionVocabularies(data: SearchCollectionVocabulariesData) =
            task {
                let! results =
                    Wordfolio.Api.DataAccess.CollectionsHierarchy.searchCollectionVocabulariesAsync
                        (UserId.value data.UserId)
                        (CollectionId.value data.CollectionId)
                        (data.Query
                         |> toSearchCollectionVocabulariesQueryDataAccess)
                        connection
                        transaction
                        cancellationToken

                return
                    results
                    |> List.map toVocabularyWithEntryCountDomain
            }

    interface IGetDefaultVocabularyWithEntryCount with
        member _.GetDefaultVocabularyWithEntryCount(UserId userId) =
            task {
                let! result =
                    Wordfolio.Api.DataAccess.CollectionsHierarchy.getDefaultVocabularySummaryByUserIdAsync
                        userId
                        connection
                        transaction
                        cancellationToken

                return
                    result
                    |> Option.map(fun v ->
                        { Id = VocabularyId v.Id
                          Name = v.Name
                          Description = v.Description
                          CreatedAt = v.CreatedAt
                          UpdatedAt = v.UpdatedAt
                          EntryCount = v.EntryCount })
            }

    interface IGetEntriesHierarchyByVocabularyId with
        member _.GetEntriesHierarchyByVocabularyId(VocabularyId vocabularyId) =
            task {
                let! entries =
                    Wordfolio.Api.DataAccess.EntriesHierarchy.getEntriesHierarchyByVocabularyIdAsync
                        vocabularyId
                        connection
                        transaction
                        cancellationToken

                return
                    entries
                    |> List.map(fun entryWithHierarchy ->
                        let definitionsWithExamples =
                            entryWithHierarchy.Definitions
                            |> List.map(fun dwithEx ->
                                let examples =
                                    dwithEx.Examples
                                    |> List.map toExampleDomain

                                toDefinitionDomain(dwithEx.Definition, examples))

                        let translationsWithExamples =
                            entryWithHierarchy.Translations
                            |> List.map(fun twithEx ->
                                let examples =
                                    twithEx.Examples
                                    |> List.map toExampleDomain

                                toTranslationDomain(twithEx.Translation, examples))

                        toEntryDomain(entryWithHierarchy.Entry, definitionsWithExamples, translationsWithExamples))
            }

type TransactionalEnv(dataSource: NpgsqlDataSource, cancellationToken: CancellationToken) =
    interface ITransactional<AppEnv> with
        member _.RunInTransaction(operation) =
            task {
                use! connection = dataSource.OpenConnectionAsync(cancellationToken)
                use! transaction = connection.BeginTransactionAsync(cancellationToken)

                let env =
                    AppEnv(connection, transaction, cancellationToken)

                let! result = operation env

                match result with
                | Ok _ -> do! transaction.CommitAsync(cancellationToken)
                | Error _ -> do! transaction.RollbackAsync(cancellationToken)

                return result
            }
