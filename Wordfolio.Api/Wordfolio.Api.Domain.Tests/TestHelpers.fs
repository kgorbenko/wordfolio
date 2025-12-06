module Wordfolio.Api.Domain.Tests.TestHelpers

open System
open System.Threading
open System.Threading.Tasks

open Wordfolio.Api.Domain

type MockCollectionRepository() =
    let mutable collections: Map<int, CollectionData> = Map.empty
    let mutable nextId = 1

    member _.Collections = collections

    member _.AddCollection(data: CollectionData) =
        collections <- collections |> Map.add (CollectionId.value data.Id) data

    member _.Clear() =
        collections <- Map.empty
        nextId <- 1

    interface ICollectionRepository with
        member _.GetByIdAsync(CollectionId id, _cancellationToken) =
            Task.FromResult(collections |> Map.tryFind id)

        member _.GetByUserIdAsync(UserId userId, _cancellationToken) =
            let result =
                collections
                |> Map.toList
                |> List.map snd
                |> List.filter(fun c -> c.UserId = UserId userId)

            Task.FromResult(result)

        member this.CreateAsync(userId, name, description, createdAt, _cancellationToken) =
            let id = nextId
            nextId <- nextId + 1

            let data =
                { Id = CollectionId id
                  UserId = userId
                  Name = name
                  Description = description
                  CreatedAt = createdAt
                  UpdatedAt = None }

            collections <- collections |> Map.add id data
            Task.FromResult(data)

        member _.UpdateAsync(CollectionId id, name, description, updatedAt, _cancellationToken) =
            match collections |> Map.tryFind id with
            | None -> Task.FromResult(false)
            | Some existing ->
                let updated =
                    { existing with
                        Name = name
                        Description = description
                        UpdatedAt = Some updatedAt }

                collections <- collections |> Map.add id updated
                Task.FromResult(true)

        member _.DeleteAsync(CollectionId id, _cancellationToken) =
            match collections |> Map.tryFind id with
            | None -> Task.FromResult(false)
            | Some _ ->
                collections <- collections |> Map.remove id
                Task.FromResult(true)

type MockVocabularyRepository() =
    let mutable vocabularies: Map<int, VocabularyData> = Map.empty
    let mutable nextId = 1

    member _.Vocabularies = vocabularies

    member _.AddVocabulary(data: VocabularyData) =
        vocabularies <- vocabularies |> Map.add (VocabularyId.value data.Id) data

    member _.Clear() =
        vocabularies <- Map.empty
        nextId <- 1

    interface IVocabularyRepository with
        member _.GetByIdAsync(VocabularyId id, _cancellationToken) =
            Task.FromResult(vocabularies |> Map.tryFind id)

        member _.GetByCollectionIdAsync(CollectionId collectionId, _cancellationToken) =
            let result =
                vocabularies
                |> Map.toList
                |> List.map snd
                |> List.filter(fun v -> v.CollectionId = CollectionId collectionId)

            Task.FromResult(result)

        member this.CreateAsync(collectionId, name, description, createdAt, _cancellationToken) =
            let id = nextId
            nextId <- nextId + 1

            let data =
                { Id = VocabularyId id
                  CollectionId = collectionId
                  Name = name
                  Description = description
                  CreatedAt = createdAt
                  UpdatedAt = None }

            vocabularies <- vocabularies |> Map.add id data
            Task.FromResult(data)

        member _.UpdateAsync(VocabularyId id, name, description, updatedAt, _cancellationToken) =
            match vocabularies |> Map.tryFind id with
            | None -> Task.FromResult(false)
            | Some existing ->
                let updated =
                    { existing with
                        Name = name
                        Description = description
                        UpdatedAt = Some updatedAt }

                vocabularies <- vocabularies |> Map.add id updated
                Task.FromResult(true)

        member _.DeleteAsync(VocabularyId id, _cancellationToken) =
            match vocabularies |> Map.tryFind id with
            | None -> Task.FromResult(false)
            | Some _ ->
                vocabularies <- vocabularies |> Map.remove id
                Task.FromResult(true)

let makeCollectionData (id: int) (userId: int) (name: string) (description: string option) : CollectionData =
    { Id = CollectionId id
      UserId = UserId userId
      Name = name
      Description = description
      CreatedAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
      UpdatedAt = None }

let makeVocabularyData
    (id: int)
    (collectionId: int)
    (name: string)
    (description: string option)
    : VocabularyData =
    { Id = VocabularyId id
      CollectionId = CollectionId collectionId
      Name = name
      Description = description
      CreatedAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
      UpdatedAt = None }
