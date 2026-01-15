module Wordfolio.Api.Domain.CollectionsHierarchy.Operations

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.CollectionsHierarchy.Capabilities
open Wordfolio.Api.Domain.Transactions

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
