module Wordfolio.Api.Domain.CollectionsHierarchy.Operations

open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Capabilities
open Wordfolio.Api.Domain.CollectionsHierarchy.Capabilities

type GetByUserIdParameters = { UserId: UserId }

type SearchUserCollectionsParameters =
    { UserId: UserId
      Query: SearchUserCollectionsQuery }

type SearchCollectionVocabulariesParameters =
    { UserId: UserId
      CollectionId: CollectionId
      Query: SearchCollectionVocabulariesQuery }

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

let searchUserCollections
    env
    (parameters: SearchUserCollectionsParameters)
    : Task<Result<CollectionWithVocabularyCount list, unit>> =
    runInTransaction env (fun appEnv ->
        task {
            let! collections =
                searchUserCollections
                    appEnv
                    { UserId = parameters.UserId
                      Query = parameters.Query }

            return (Ok collections: Result<CollectionWithVocabularyCount list, unit>)
        })

let searchCollectionVocabularies
    env
    (parameters: SearchCollectionVocabulariesParameters)
    : Task<Result<VocabularyWithEntryCount list, unit>> =
    runInTransaction env (fun appEnv ->
        task {
            let! vocabularies =
                searchCollectionVocabularies
                    appEnv
                    { UserId = parameters.UserId
                      CollectionId = parameters.CollectionId
                      Query = parameters.Query }

            return (Ok vocabularies: Result<VocabularyWithEntryCount list, unit>)
        })
