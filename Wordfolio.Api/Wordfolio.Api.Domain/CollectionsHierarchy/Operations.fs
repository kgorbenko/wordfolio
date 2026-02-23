module Wordfolio.Api.Domain.CollectionsHierarchy.Operations

open Wordfolio.Api.Domain.Capabilities
open Wordfolio.Api.Domain.CollectionsHierarchy.Capabilities

let getByUserId env userId =
    runInTransaction env (fun appEnv ->
        task {
            let! collections = getCollectionsWithVocabularies appEnv userId
            let! defaultVocabulary = getDefaultVocabularySummary appEnv userId

            let filteredDefaultVocabulary =
                defaultVocabulary
                |> Option.filter(fun v -> v.EntryCount > 0)

            return
                Ok
                    { Collections = collections
                      DefaultVocabulary = filteredDefaultVocabulary }
        })

let searchUserCollections env userId query =
    runInTransaction env (fun appEnv ->
        task {
            let! collections = searchUserCollections appEnv userId query
            return Ok collections
        })

let searchCollectionVocabularies env userId collectionId query =
    runInTransaction env (fun appEnv ->
        task {
            let! vocabularies = searchCollectionVocabularies appEnv userId collectionId query
            return Ok vocabularies
        })
