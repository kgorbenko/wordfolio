namespace Wordfolio.Api.Domain.Entries

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain

type CreateEntryData =
    { VocabularyId: VocabularyId
      EntryText: string
      CreatedAt: DateTimeOffset }

type CreateDefinitionData =
    { EntryId: EntryId
      Text: string
      Source: DefinitionSource
      DisplayOrder: int }

type CreateTranslationData =
    { EntryId: EntryId
      Text: string
      Source: TranslationSource
      DisplayOrder: int }

type GetEntryByTextAndVocabularyIdData =
    { VocabularyId: VocabularyId
      EntryText: string }

type CreateExamplesForDefinitionData =
    { DefinitionId: DefinitionId
      Examples: ExampleInput list }

type CreateExamplesForTranslationData =
    { TranslationId: TranslationId
      Examples: ExampleInput list }

type UpdateEntryData =
    { EntryId: EntryId
      EntryText: string
      UpdatedAt: DateTimeOffset }

type MoveEntryData =
    { EntryId: EntryId
      OldVocabularyId: VocabularyId
      NewVocabularyId: VocabularyId
      UpdatedAt: DateTimeOffset }

type HasVocabularyAccessInCollectionData =
    { VocabularyId: VocabularyId
      CollectionId: CollectionId
      UserId: UserId }

type HasVocabularyAccessData =
    { VocabularyId: VocabularyId
      UserId: UserId }

type IGetEntryById =
    abstract GetEntryById: EntryId -> Task<Entry option>

type IGetEntryByTextAndVocabularyId =
    abstract GetEntryByTextAndVocabularyId: GetEntryByTextAndVocabularyIdData -> Task<Entry option>

type ICreateEntry =
    abstract CreateEntry: CreateEntryData -> Task<EntryId>

type ICreateDefinition =
    abstract CreateDefinition: CreateDefinitionData -> Task<DefinitionId>

type ICreateTranslation =
    abstract CreateTranslation: CreateTranslationData -> Task<TranslationId>

type ICreateExamplesForDefinition =
    abstract CreateExamplesForDefinition: CreateExamplesForDefinitionData -> Task<unit>

type ICreateExamplesForTranslation =
    abstract CreateExamplesForTranslation: CreateExamplesForTranslationData -> Task<unit>

type IUpdateEntry =
    abstract UpdateEntry: UpdateEntryData -> Task<unit>

type IMoveEntry =
    abstract MoveEntry: MoveEntryData -> Task<unit>

type IClearEntryChildren =
    abstract ClearEntryChildren: EntryId -> Task<unit>

type IHasVocabularyAccess =
    abstract HasVocabularyAccess: HasVocabularyAccessData -> Task<bool>

type IHasVocabularyAccessInCollection =
    abstract HasVocabularyAccessInCollection: HasVocabularyAccessInCollectionData -> Task<bool>

type IDeleteEntry =
    abstract DeleteEntry: EntryId -> Task<int>

type IGetEntriesHierarchyByVocabularyId =
    abstract GetEntriesHierarchyByVocabularyId: VocabularyId -> Task<Entry list>

module Capabilities =
    let getEntryById (env: #IGetEntryById) entryId = env.GetEntryById(entryId)

    let getEntryByTextAndVocabularyId (env: #IGetEntryByTextAndVocabularyId) data =
        env.GetEntryByTextAndVocabularyId(data)

    let createEntry (env: #ICreateEntry) data = env.CreateEntry(data)

    let createDefinition (env: #ICreateDefinition) data = env.CreateDefinition(data)

    let createTranslation (env: #ICreateTranslation) data = env.CreateTranslation(data)

    let createExamplesForDefinition (env: #ICreateExamplesForDefinition) data = env.CreateExamplesForDefinition(data)

    let createExamplesForTranslation (env: #ICreateExamplesForTranslation) data = env.CreateExamplesForTranslation(data)

    let updateEntry (env: #IUpdateEntry) data = env.UpdateEntry(data)

    let moveEntry (env: #IMoveEntry) data = env.MoveEntry(data)

    let clearEntryChildren (env: #IClearEntryChildren) entryId = env.ClearEntryChildren(entryId)

    let hasVocabularyAccess (env: #IHasVocabularyAccess) data = env.HasVocabularyAccess(data)

    let hasVocabularyAccessInCollection (env: #IHasVocabularyAccessInCollection) data =
        env.HasVocabularyAccessInCollection(data)

    let deleteEntry (env: #IDeleteEntry) entryId = env.DeleteEntry(entryId)

    let getEntriesHierarchyByVocabularyId (env: #IGetEntriesHierarchyByVocabularyId) vocabularyId =
        env.GetEntriesHierarchyByVocabularyId(vocabularyId)
