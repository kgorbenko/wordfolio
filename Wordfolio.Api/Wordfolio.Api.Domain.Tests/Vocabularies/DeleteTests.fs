module Wordfolio.Api.Domain.Tests.Vocabularies.DeleteTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Vocabularies
open Wordfolio.Api.Domain.Vocabularies.Operations

type TestEnv
    (
        getVocabularyById: VocabularyId -> Task<Vocabulary option>,
        getCollectionById: CollectionId -> Task<Collection option>,
        deleteVocabulary: VocabularyId -> Task<int>
    ) =
    let getVocabularyByIdCalls = ResizeArray<VocabularyId>()
    let getCollectionByIdCalls = ResizeArray<CollectionId>()
    let deleteVocabularyCalls = ResizeArray<VocabularyId>()

    member _.GetVocabularyByIdCalls = getVocabularyByIdCalls |> Seq.toList
    member _.GetCollectionByIdCalls = getCollectionByIdCalls |> Seq.toList
    member _.DeleteVocabularyCalls = deleteVocabularyCalls |> Seq.toList

    interface IGetVocabularyById with
        member _.GetVocabularyById(id) =
            getVocabularyByIdCalls.Add(id)
            getVocabularyById id

    interface IGetCollectionById with
        member _.GetCollectionById(id) =
            getCollectionByIdCalls.Add(id)
            getCollectionById id

    interface IDeleteVocabulary with
        member _.DeleteVocabulary(id) =
            deleteVocabularyCalls.Add(id)
            deleteVocabulary id

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
let ``deletes vocabulary when collection owned by user`` () =
    let existingVocabulary = makeVocabulary 1 1 "Test Vocabulary"
    let collection = makeCollection 1 1

    let env =
        TestEnv(
            getVocabularyById =
                (fun id ->
                    if id <> VocabularyId 1 then
                        failwith $"Unexpected vocabulary id: {id}"

                    Task.FromResult(Some existingVocabulary)),
            getCollectionById =
                (fun id ->
                    if id <> CollectionId 1 then
                        failwith $"Unexpected collection id: {id}"

                    Task.FromResult(Some collection)),
            deleteVocabulary =
                (fun id ->
                    if id <> VocabularyId 1 then
                        failwith $"Unexpected vocabulary id: {id}"

                    Task.FromResult(1))
        )

    task {
        let! result = delete env (UserId 1) (VocabularyId 1)

        Assert.Equal(Ok(), result)
        Assert.Equal([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
        Assert.Equal([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Equal([ VocabularyId 1 ], env.DeleteVocabularyCalls)
    }

[<Fact>]
let ``returns NotFound when vocabulary does not exist`` () =
    let env =
        TestEnv(
            getVocabularyById =
                (fun id ->
                    if id <> VocabularyId 1 then
                        failwith $"Unexpected vocabulary id: {id}"

                    Task.FromResult(None)),
            getCollectionById = (fun _ -> failwith "Should not be called"),
            deleteVocabulary = (fun _ -> failwith "Should not be called")
        )

    task {
        let! result = delete env (UserId 1) (VocabularyId 1)

        Assert.Equal(Error(VocabularyNotFound(VocabularyId 1)), result)
        Assert.Equal([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
        Assert.Empty(env.DeleteVocabularyCalls)
    }

[<Fact>]
let ``returns AccessDenied when collection owned by different user`` () =
    let existingVocabulary = makeVocabulary 1 1 "Test Vocabulary"
    let collection = makeCollection 1 2

    let env =
        TestEnv(
            getVocabularyById = (fun _ -> Task.FromResult(Some existingVocabulary)),
            getCollectionById = (fun _ -> Task.FromResult(Some collection)),
            deleteVocabulary = (fun _ -> failwith "Should not be called")
        )

    task {
        let! result = delete env (UserId 1) (VocabularyId 1)

        Assert.Equal(Error(VocabularyAccessDenied(VocabularyId 1)), result)
        Assert.Empty(env.DeleteVocabularyCalls)
    }

[<Fact>]
let ``returns AccessDenied when collection does not exist`` () =
    let existingVocabulary = makeVocabulary 1 1 "Test Vocabulary"

    let env =
        TestEnv(
            getVocabularyById = (fun _ -> Task.FromResult(Some existingVocabulary)),
            getCollectionById = (fun _ -> Task.FromResult(None)),
            deleteVocabulary = (fun _ -> failwith "Should not be called")
        )

    task {
        let! result = delete env (UserId 1) (VocabularyId 1)

        Assert.Equal(Error(VocabularyAccessDenied(VocabularyId 1)), result)
        Assert.Empty(env.DeleteVocabularyCalls)
    }

[<Fact>]
let ``returns NotFound when delete affects no rows`` () =
    let existingVocabulary = makeVocabulary 1 1 "Test Vocabulary"
    let collection = makeCollection 1 1

    let env =
        TestEnv(
            getVocabularyById = (fun _ -> Task.FromResult(Some existingVocabulary)),
            getCollectionById = (fun _ -> Task.FromResult(Some collection)),
            deleteVocabulary = (fun _ -> Task.FromResult(0))
        )

    task {
        let! result = delete env (UserId 1) (VocabularyId 1)

        Assert.Equal(Error(VocabularyNotFound(VocabularyId 1)), result)
        Assert.Equal([ VocabularyId 1 ], env.DeleteVocabularyCalls)
    }
