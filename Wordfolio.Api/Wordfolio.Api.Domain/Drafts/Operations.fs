module Wordfolio.Api.Domain.Drafts.Operations

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Shared
open Wordfolio.Api.Domain.Transactions

let get (env: #ITransactional<#IGetDefaultVocabulary & #IGetEntriesHierarchy>) (userId: UserId) =
    runInTransaction env (fun appEnv ->
        task {
            // First check if default vocabulary exists
            match! Shared.Capabilities.getDefaultVocabulary appEnv userId with
            | None -> return Ok None // Valid case - no default vocabulary yet
            | Some vocabulary ->
                // Load entries by vocabulary ID
                let! entries = Capabilities.getEntriesHierarchy appEnv vocabulary.Id

                return
                    Ok(
                        Some
                            { Vocabulary = vocabulary
                              Entries = entries }
                    )
        })
