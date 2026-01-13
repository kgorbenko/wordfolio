module Wordfolio.Api.Domain.CollectionsHierarchy.Operations

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.CollectionsHierarchy.Capabilities
open Wordfolio.Api.Domain.Transactions

let getByUserId env userId =
    runInTransaction env (fun appEnv ->
        task {
            let! collections = getCollectionsWithVocabularies appEnv userId
            return Ok collections
        })
