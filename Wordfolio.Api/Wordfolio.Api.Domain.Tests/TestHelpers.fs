module Wordfolio.Api.Domain.Tests.TestHelpers

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Vocabularies

type TestCollectionsEnv(collections: Map<int, Collection> ref) =
    interface IGetCollectionById with
        member _.GetCollectionById(CollectionId id) =
            Task.FromResult(collections.Value |> Map.tryFind id)

    interface IGetCollectionsByUserId with
        member _.GetCollectionsByUserId(UserId userId) =
            let result =
                collections.Value
                |> Map.toList
                |> List.map snd
                |> List.filter(fun c -> c.UserId = UserId userId)

            Task.FromResult(result)

    interface ICreateCollection with
        member _.CreateCollection(userId, name, description, createdAt) =
            let nextId =
                if Map.isEmpty collections.Value then
                    1
                else
                    (collections.Value |> Map.keys |> Seq.max)
                    + 1

            let collection =
                { Id = CollectionId nextId
                  UserId = userId
                  Name = name
                  Description = description
                  CreatedAt = createdAt
                  UpdatedAt = None }

            collections.Value <-
                collections.Value
                |> Map.add nextId collection

            Task.FromResult(collection)

    interface IUpdateCollection with
        member _.UpdateCollection(CollectionId id, name, description, updatedAt) =
            match collections.Value |> Map.tryFind id with
            | None -> Task.FromResult(false)
            | Some existing ->
                let updated =
                    { existing with
                        Name = name
                        Description = description
                        UpdatedAt = Some updatedAt }

                collections.Value <- collections.Value |> Map.add id updated
                Task.FromResult(true)

    interface IDeleteCollection with
        member _.DeleteCollection(CollectionId id) =
            match collections.Value |> Map.tryFind id with
            | None -> Task.FromResult(false)
            | Some _ ->
                collections.Value <- collections.Value |> Map.remove id
                Task.FromResult(true)

type TestVocabulariesEnv(collections: Map<int, Collection> ref, vocabularies: Map<int, Vocabulary> ref) =
    interface IGetCollectionById with
        member _.GetCollectionById(CollectionId id) =
            Task.FromResult(collections.Value |> Map.tryFind id)

    interface IGetVocabularyById with
        member _.GetVocabularyById(VocabularyId id) =
            Task.FromResult(vocabularies.Value |> Map.tryFind id)

    interface IGetVocabulariesByCollectionId with
        member _.GetVocabulariesByCollectionId(CollectionId collectionId) =
            let result =
                vocabularies.Value
                |> Map.toList
                |> List.map snd
                |> List.filter(fun v -> v.CollectionId = CollectionId collectionId)

            Task.FromResult(result)

    interface ICreateVocabulary with
        member _.CreateVocabulary(collectionId, name, description, createdAt) =
            let nextId =
                if Map.isEmpty vocabularies.Value then
                    1
                else
                    (vocabularies.Value
                     |> Map.keys
                     |> Seq.max)
                    + 1

            let vocabulary =
                { Id = VocabularyId nextId
                  CollectionId = collectionId
                  Name = name
                  Description = description
                  CreatedAt = createdAt
                  UpdatedAt = None }

            vocabularies.Value <-
                vocabularies.Value
                |> Map.add nextId vocabulary

            Task.FromResult(vocabulary)

    interface IUpdateVocabulary with
        member _.UpdateVocabulary(VocabularyId id, name, description, updatedAt) =
            match vocabularies.Value |> Map.tryFind id with
            | None -> Task.FromResult(false)
            | Some existing ->
                let updated =
                    { existing with
                        Name = name
                        Description = description
                        UpdatedAt = Some updatedAt }

                vocabularies.Value <- vocabularies.Value |> Map.add id updated
                Task.FromResult(true)

    interface IDeleteVocabulary with
        member _.DeleteVocabulary(VocabularyId id) =
            match vocabularies.Value |> Map.tryFind id with
            | None -> Task.FromResult(false)
            | Some _ ->
                vocabularies.Value <- vocabularies.Value |> Map.remove id
                Task.FromResult(true)

type TestTransactionalEnv<'env>(appEnv: 'env) =
    let mutable committed = false
    let mutable rolledBack = false

    member _.WasCommitted = committed
    member _.WasRolledBack = rolledBack

    interface ITransactional<'env> with
        member _.RunInTransaction(operation) =
            task {
                let! result = operation appEnv

                match result with
                | Ok _ -> committed <- true
                | Error _ -> rolledBack <- true

                return result
            }

let makeCollection (id: int) (userId: int) (name: string) (description: string option) : Collection =
    { Id = CollectionId id
      UserId = UserId userId
      Name = name
      Description = description
      CreatedAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
      UpdatedAt = None }

let makeVocabulary (id: int) (collectionId: int) (name: string) (description: string option) : Vocabulary =
    { Id = VocabularyId id
      CollectionId = CollectionId collectionId
      Name = name
      Description = description
      CreatedAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
      UpdatedAt = None }
