module Wordfolio.Api.Domain.CollectionsHierarchy.Operations

open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Capabilities
open Wordfolio.Api.Domain.CollectionsHierarchy.Capabilities

type GetByUserIdParameters = { UserId: UserId }

type GetVocabulariesWithEntryCountByCollectionIdParameters =
    { UserId: UserId
      CollectionId: CollectionId }

let getByUserId env (parameters: GetByUserIdParameters) : Task<Result<CollectionsHierarchyResult, unit>> =
    runInTransaction env (fun appEnv ->
        task {
            let! collections = getCollectionsWithVocabularies appEnv parameters.UserId
            let! defaultVocabulary = getDefaultVocabularyWithEntryCount appEnv parameters.UserId

            let filteredDefaultVocabulary =
                defaultVocabulary
                |> Option.filter(fun v -> v.EntryCount > 0)

            return
                Ok
                    { Collections = collections
                      DefaultVocabulary = filteredDefaultVocabulary }
        })

let getCollectionsWithVocabularyCount env (userId: UserId) : Task<Result<CollectionWithVocabularyCount list, unit>> =
    runInTransaction env (fun appEnv ->
        task {
            let! collections = getCollectionsWithVocabularyCount appEnv userId
            return (Ok collections: Result<CollectionWithVocabularyCount list, unit>)
        })

let getVocabulariesWithEntryCountByCollectionId
    env
    (parameters: GetVocabulariesWithEntryCountByCollectionIdParameters)
    : Task<Result<VocabularyWithEntryCount list, unit>> =
    runInTransaction env (fun appEnv ->
        task {
            let! vocabularies =
                getVocabulariesWithEntryCountByCollectionId appEnv parameters.UserId parameters.CollectionId

            return (Ok vocabularies: Result<VocabularyWithEntryCount list, unit>)
        })
