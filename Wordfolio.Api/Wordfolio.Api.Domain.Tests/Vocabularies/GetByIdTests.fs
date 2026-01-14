module Wordfolio.Api.Domain.Tests.Vocabularies.GetByIdTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Shared
open Wordfolio.Api.Domain.Vocabularies
open Wordfolio.Api.Domain.Vocabularies.Operations

type TestEnv
    (
        getVocabularyById: VocabularyId -> Task<Vocabulary option>,
        getCollectionById: CollectionId -> Task<Collection option>
    ) =
    let getVocabularyByIdCalls =
        ResizeArray<VocabularyId>()

    let getCollectionByIdCalls =
        ResizeArray<CollectionId>()

    member _.GetVocabularyByIdCalls =
        getVocabularyByIdCalls |> Seq.toList

    member _.GetCollectionByIdCalls =
        getCollectionByIdCalls |> Seq.toList

    interface IGetVocabularyById with
        member _.GetVocabularyById(id) =
            getVocabularyByIdCalls.Add(id)
            getVocabularyById id

    interface IGetCollectionById with
        member _.GetCollectionById(id) =
            getCollectionByIdCalls.Add(id)
            getCollectionById id

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
let ``returns vocabulary when found and collection owned by user``() =
    task {
        let vocabulary =
            makeVocabulary 1 1 "Test Vocabulary"

        let collection = makeCollection 1 1

        let env =
            TestEnv(
                getVocabularyById =
                    (fun id ->
                        if id <> VocabularyId 1 then
                            failwith $"Unexpected vocabulary id: {id}"

                        Task.FromResult(Some vocabulary)),
                getCollectionById =
                    (fun id ->
                        if id <> CollectionId 1 then
                            failwith $"Unexpected collection id: {id}"

                        Task.FromResult(Some collection))
            )

        let! result = getById env (UserId 1) (VocabularyId 1)

        Assert.Equal(Ok vocabulary, result)
        Assert.Equal<VocabularyId list>([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
    }

[<Fact>]
let ``returns NotFound when vocabulary does not exist``() =
    task {
        let env =
            TestEnv(
                getVocabularyById =
                    (fun id ->
                        if id <> VocabularyId 1 then
                            failwith $"Unexpected vocabulary id: {id}"

                        Task.FromResult(None)),
                getCollectionById = (fun _ -> failwith "Should not be called")
            )

        let! result = getById env (UserId 1) (VocabularyId 1)

        Assert.Equal(Error(VocabularyNotFound(VocabularyId 1)), result)
        Assert.Equal<VocabularyId list>([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
        Assert.Empty(env.GetCollectionByIdCalls)
    }

[<Fact>]
let ``returns AccessDenied when collection owned by different user``() =
    task {
        let vocabulary =
            makeVocabulary 1 1 "Test Vocabulary"

        let collection = makeCollection 1 2

        let env =
            TestEnv(
                getVocabularyById =
                    (fun id ->
                        if id <> VocabularyId 1 then
                            failwith $"Unexpected vocabulary id: {id}"

                        Task.FromResult(Some vocabulary)),
                getCollectionById =
                    (fun id ->
                        if id <> CollectionId 1 then
                            failwith $"Unexpected collection id: {id}"

                        Task.FromResult(Some collection))
            )

        let! result = getById env (UserId 1) (VocabularyId 1)

        Assert.Equal(Error(VocabularyAccessDenied(VocabularyId 1)), result)
    }

[<Fact>]
let ``returns AccessDenied when collection does not exist``() =
    task {
        let vocabulary =
            makeVocabulary 1 1 "Test Vocabulary"

        let env =
            TestEnv(
                getVocabularyById = (fun _ -> Task.FromResult(Some vocabulary)),
                getCollectionById = (fun _ -> Task.FromResult(None))
            )

        let! result = getById env (UserId 1) (VocabularyId 1)

        Assert.Equal(Error(VocabularyAccessDenied(VocabularyId 1)), result)
    }
