namespace Wordfolio.Api.Domain.CollectionsHierarchy

open System.Threading.Tasks

open Wordfolio.Api.Domain

type IGetCollectionsWithVocabularies =
    abstract GetCollectionsWithVocabularies: UserId -> Task<CollectionSummary list>

type ISearchUserCollections =
    abstract SearchUserCollections: UserId * SearchUserCollectionsQuery -> Task<CollectionOverview list>

type ISearchCollectionVocabularies =
    abstract SearchCollectionVocabularies:
        UserId * CollectionId * VocabularySummaryQuery -> Task<VocabularySummary list>

type IGetDefaultVocabularySummary =
    abstract GetDefaultVocabularySummary: UserId -> Task<VocabularySummary option>

module Capabilities =
    let getCollectionsWithVocabularies (env: #IGetCollectionsWithVocabularies) userId =
        env.GetCollectionsWithVocabularies(userId)

    let searchUserCollections (env: #ISearchUserCollections) userId query =
        env.SearchUserCollections(userId, query)

    let searchCollectionVocabularies (env: #ISearchCollectionVocabularies) userId collectionId query =
        env.SearchCollectionVocabularies(userId, collectionId, query)

    let getDefaultVocabularySummary (env: #IGetDefaultVocabularySummary) userId =
        env.GetDefaultVocabularySummary(userId)
