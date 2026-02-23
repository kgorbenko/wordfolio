module Wordfolio.Api.Domain.Operations

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Capabilities

[<Literal>]
let SystemCollectionName =
    "[System] Unsorted"

[<Literal>]
let DefaultVocabularyName = "[Default]"

let getOrCreateDefaultVocabulary env (userId: UserId) (now: DateTimeOffset) : Task<VocabularyId> =
    task {
        let! maybeVocabulary = getDefaultVocabulary env userId

        match maybeVocabulary with
        | Some vocabulary -> return vocabulary.Id
        | None ->
            let! maybeCollection = getDefaultCollection env userId

            let! collectionId =
                match maybeCollection with
                | Some collection -> collection.Id |> Task.FromResult
                | None ->
                    let collectionParams: CreateCollectionParameters =
                        { UserId = userId
                          Name = SystemCollectionName
                          Description = None
                          CreatedAt = now }

                    createDefaultCollection env collectionParams

            let vocabularyParams: CreateVocabularyParameters =
                { CollectionId = collectionId
                  Name = DefaultVocabularyName
                  Description = None
                  CreatedAt = now }

            return! createDefaultVocabulary env vocabularyParams
    }
