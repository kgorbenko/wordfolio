namespace Wordfolio.Api.Domain.CollectionsHierarchy

open System.Threading.Tasks

open Wordfolio.Api.Domain

type IGetCollectionsWithVocabularies =
    abstract GetCollectionsWithVocabularies: UserId -> Task<CollectionWithVocabularies list>

type IGetCollectionsWithVocabularyCount =
    abstract GetCollectionsWithVocabularyCount: UserId -> Task<CollectionWithVocabularyCount list>

type IGetVocabulariesWithEntryCountByCollectionId =
    abstract GetVocabulariesWithEntryCountByCollectionId: UserId -> CollectionId -> Task<VocabularyWithEntryCount list>

type IGetDefaultVocabularyWithEntryCount =
    abstract GetDefaultVocabularyWithEntryCount: UserId -> Task<VocabularyWithEntryCount option>

module Capabilities =
    let getCollectionsWithVocabularies (env: #IGetCollectionsWithVocabularies) userId =
        env.GetCollectionsWithVocabularies(userId)

    let getCollectionsWithVocabularyCount (env: #IGetCollectionsWithVocabularyCount) userId =
        env.GetCollectionsWithVocabularyCount(userId)

    let getVocabulariesWithEntryCountByCollectionId
        (env: #IGetVocabulariesWithEntryCountByCollectionId)
        userId
        collectionId
        =
        env.GetVocabulariesWithEntryCountByCollectionId userId collectionId

    let getDefaultVocabularyWithEntryCount (env: #IGetDefaultVocabularyWithEntryCount) userId =
        env.GetDefaultVocabularyWithEntryCount(userId)
