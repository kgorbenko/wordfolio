module Wordfolio.Api.Domain.Tests.Vocabularies.GetByCollectionIdTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Vocabularies
open Wordfolio.Api.Domain.Vocabularies.Operations

type TestEnv
    (
        getCollectionById: CollectionId -> Task<Collection option>,
        getVocabulariesByCollectionId: CollectionId -> Task<Vocabulary list>
    ) =
    let getCollectionByIdCalls =
        ResizeArray<CollectionId>()

    let getVocabulariesByCollectionIdCalls =
        ResizeArray<CollectionId>()

    member _.GetCollectionByIdCalls =
        getCollectionByIdCalls |> Seq.toList

    member _.GetVocabulariesByCollectionIdCalls =
        getVocabulariesByCollectionIdCalls
        |> Seq.toList

    interface IGetCollectionById with
        member _.GetCollectionById(id) =
            getCollectionByIdCalls.Add(id)
            getCollectionById id

    interface IGetVocabulariesByCollectionId with
        member _.GetVocabulariesByCollectionId(id) =
            getVocabulariesByCollectionIdCalls.Add(id)
            getVocabulariesByCollectionId id

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let makeCollection id userId =
    { Id = CollectionId id
      UserId = UserId userId
      Name = "Test Collection"
      Description = None
      CreatedAt = DateTimeOffset.UtcNow
      UpdatedAt = None }

let makeVocabulary id collectionId name =
    { Id = VocabularyId id
      CollectionId = CollectionId collectionId
      Name = name
      Description = None
      CreatedAt = DateTimeOffset.UtcNow
      UpdatedAt = None }

[<Fact>]
let ``returns vocabularies when collection owned by user``() =
    let collection = makeCollection 1 1

    let vocabularies =
        [ makeVocabulary 1 1 "Vocabulary 1"; makeVocabulary 2 1 "Vocabulary 2" ]

    let env =
        TestEnv(
            getCollectionById =
                (fun id ->
                    if id <> CollectionId 1 then
                        failwith $"Unexpected collection id: {id}"

                    Task.FromResult(Some collection)),
            getVocabulariesByCollectionId =
                (fun id ->
                    if id <> CollectionId 1 then
                        failwith $"Unexpected collection id: {id}"

                    Task.FromResult(vocabularies))
        )

    task {
        let! result = getByCollectionId env (UserId 1) (CollectionId 1)

        Assert.Equal(Ok vocabularies, result)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetVocabulariesByCollectionIdCalls)
    }

[<Fact>]
let ``returns empty list when collection has no vocabularies``() =
    let collection = makeCollection 1 1

    let env =
        TestEnv(
            getCollectionById = (fun _ -> Task.FromResult(Some collection)),
            getVocabulariesByCollectionId = (fun _ -> Task.FromResult([]))
        )

    task {
        let! result = getByCollectionId env (UserId 1) (CollectionId 1)

        match result with
        | Ok vocabularies -> Assert.Empty(vocabularies)
        | Error e -> failwith $"Expected Ok, got Error: {e}"
    }

[<Fact>]
let ``returns CollectionNotFound when collection does not exist``() =
    let env =
        TestEnv(
            getCollectionById =
                (fun id ->
                    if id <> CollectionId 1 then
                        failwith $"Unexpected collection id: {id}"

                    Task.FromResult(None)),
            getVocabulariesByCollectionId = (fun _ -> failwith "Should not be called")
        )

    task {
        let! result = getByCollectionId env (UserId 1) (CollectionId 1)

        Assert.Equal(Error(VocabularyCollectionNotFound(CollectionId 1)), result)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Empty(env.GetVocabulariesByCollectionIdCalls)
    }

[<Fact>]
let ``returns CollectionNotFound when collection owned by different user``() =
    let collection = makeCollection 1 2

    let env =
        TestEnv(
            getCollectionById =
                (fun id ->
                    if id <> CollectionId 1 then
                        failwith $"Unexpected collection id: {id}"

                    Task.FromResult(Some collection)),
            getVocabulariesByCollectionId = (fun _ -> failwith "Should not be called")
        )

    task {
        let! result = getByCollectionId env (UserId 1) (CollectionId 1)

        Assert.Equal(Error(VocabularyCollectionNotFound(CollectionId 1)), result)
        Assert.Empty(env.GetVocabulariesByCollectionIdCalls)
    }
