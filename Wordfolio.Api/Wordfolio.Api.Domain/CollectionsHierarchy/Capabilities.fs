namespace Wordfolio.Api.Domain.CollectionsHierarchy

open System.Threading.Tasks

open Wordfolio.Api.Domain

type SearchCollectionVocabulariesData =
    { UserId: UserId
      CollectionId: CollectionId
      Query: SearchCollectionVocabulariesQuery }

type IGetCollectionsWithVocabularies =
    abstract GetCollectionsWithVocabularies: UserId -> Task<CollectionWithVocabularies list>

type IGetCollectionsWithVocabularyCount =
    abstract GetCollectionsWithVocabularyCount: UserId -> Task<CollectionWithVocabularyCount list>

type ISearchCollectionVocabularies =
    abstract SearchCollectionVocabularies: SearchCollectionVocabulariesData -> Task<VocabularyWithEntryCount list>

type IGetDefaultVocabularyWithEntryCount =
    abstract GetDefaultVocabularyWithEntryCount: UserId -> Task<VocabularyWithEntryCount option>

module Capabilities =
    let getCollectionsWithVocabularies (env: #IGetCollectionsWithVocabularies) userId =
        env.GetCollectionsWithVocabularies(userId)

    let getCollectionsWithVocabularyCount (env: #IGetCollectionsWithVocabularyCount) userId =
        env.GetCollectionsWithVocabularyCount(userId)

    let searchCollectionVocabularies (env: #ISearchCollectionVocabularies) data = env.SearchCollectionVocabularies(data)

    let getDefaultVocabularyWithEntryCount (env: #IGetDefaultVocabularyWithEntryCount) userId =
        env.GetDefaultVocabularyWithEntryCount(userId)
