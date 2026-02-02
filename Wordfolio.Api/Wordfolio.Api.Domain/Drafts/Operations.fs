module Wordfolio.Api.Domain.Drafts.Operations

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Shared
open Wordfolio.Api.Domain.Transactions

let get (env: #ITransactional<#IGetDefaultVocabulary & #IGetEntriesHierarchy>) (userId: UserId) =
    runInTransaction env (fun appEnv ->
        task {
            match! Shared.Capabilities.getDefaultVocabulary appEnv userId with
            | None -> return Ok None
            | Some vocabulary ->
                let! entries = Capabilities.getEntriesHierarchy appEnv vocabulary.Id

                return
                    Ok(
                        Some
                            { Vocabulary = vocabulary
                              Entries = entries }
                    )
        })
