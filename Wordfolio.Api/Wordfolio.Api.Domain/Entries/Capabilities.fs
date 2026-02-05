namespace Wordfolio.Api.Domain.Entries

open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Vocabularies

type IGetEntryById =
    abstract GetEntryById: EntryId -> Task<Entry option>

type IGetEntriesByVocabularyId =
    abstract GetEntriesByVocabularyId: VocabularyId -> Task<Entry list>

type IGetEntryByTextAndVocabularyId =
    abstract GetEntryByTextAndVocabularyId: VocabularyId * string -> Task<Entry option>

type ICreateEntry =
    abstract CreateEntry: VocabularyId * string * System.DateTimeOffset -> Task<EntryId>

type ICreateDefinition =
    abstract CreateDefinition: EntryId * string * DefinitionSource * int -> Task<DefinitionId>

type ICreateTranslation =
    abstract CreateTranslation: EntryId * string * TranslationSource * int -> Task<TranslationId>

type ICreateExamplesForDefinition =
    abstract CreateExamplesForDefinition: DefinitionId * ExampleInput list -> Task<unit>

type ICreateExamplesForTranslation =
    abstract CreateExamplesForTranslation: TranslationId * ExampleInput list -> Task<unit>

type IUpdateEntry =
    abstract UpdateEntry: EntryId * string * System.DateTimeOffset -> Task<unit>

type IClearEntryChildren =
    abstract ClearEntryChildren: EntryId -> Task<unit>

type IGetVocabularyByIdAndUserId =
    abstract GetVocabularyByIdAndUserId: VocabularyId * UserId -> Task<Vocabulary option>

type IDeleteEntry =
    abstract DeleteEntry: EntryId -> Task<int>

type IGetEntriesHierarchy =
    abstract member GetEntriesHierarchy: VocabularyId -> Task<Entry list>

module Capabilities =
    let getEntryById (env: #IGetEntryById) entryId = env.GetEntryById(entryId)

    let getEntriesByVocabularyId (env: #IGetEntriesByVocabularyId) vocabularyId =
        env.GetEntriesByVocabularyId(vocabularyId)

    let getEntryByTextAndVocabularyId (env: #IGetEntryByTextAndVocabularyId) vocabularyId entryText =
        env.GetEntryByTextAndVocabularyId(vocabularyId, entryText)

    let createEntry (env: #ICreateEntry) vocabularyId entryText createdAt =
        env.CreateEntry(vocabularyId, entryText, createdAt)

    let createDefinition (env: #ICreateDefinition) entryId text source displayOrder =
        env.CreateDefinition(entryId, text, source, displayOrder)

    let createTranslation (env: #ICreateTranslation) entryId text source displayOrder =
        env.CreateTranslation(entryId, text, source, displayOrder)

    let createExamplesForDefinition (env: #ICreateExamplesForDefinition) definitionId examples =
        env.CreateExamplesForDefinition(definitionId, examples)

    let createExamplesForTranslation (env: #ICreateExamplesForTranslation) translationId examples =
        env.CreateExamplesForTranslation(translationId, examples)

    let updateEntry (env: #IUpdateEntry) entryId entryText updatedAt =
        env.UpdateEntry(entryId, entryText, updatedAt)

    let clearEntryChildren (env: #IClearEntryChildren) entryId = env.ClearEntryChildren(entryId)

    let getVocabularyByIdAndUserId (env: #IGetVocabularyByIdAndUserId) vocabularyId userId =
        env.GetVocabularyByIdAndUserId(vocabularyId, userId)

    let deleteEntry (env: #IDeleteEntry) entryId = env.DeleteEntry(entryId)

    let getEntriesHierarchy (env: #IGetEntriesHierarchy) vocabularyId = env.GetEntriesHierarchy(vocabularyId)
