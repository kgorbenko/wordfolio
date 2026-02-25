module Wordfolio.Api.Domain.Tests.Vocabularies.CreateTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Vocabularies
open Wordfolio.Api.Domain.Vocabularies.Operations

type CreateVocabularyCall =
    { CollectionId: CollectionId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset }

type TestEnv
    (
        getCollectionById: CollectionId -> Task<Collection option>,
        createVocabulary: CreateVocabularyData -> Task<VocabularyId>,
        getVocabularyById: VocabularyId -> Task<Vocabulary option>
    ) =
    let getCollectionByIdCalls =
        ResizeArray<CollectionId>()

    let createVocabularyCalls =
        ResizeArray<CreateVocabularyCall>()

    let getVocabularyByIdCalls =
        ResizeArray<VocabularyId>()

    member _.GetCollectionByIdCalls =
        getCollectionByIdCalls |> Seq.toList

    member _.CreateVocabularyCalls =
        createVocabularyCalls |> Seq.toList

    member _.GetVocabularyByIdCalls =
        getVocabularyByIdCalls |> Seq.toList

    interface IGetCollectionById with
        member _.GetCollectionById(id) =
            getCollectionByIdCalls.Add(id)
            getCollectionById id

    interface ICreateVocabulary with
        member _.CreateVocabulary(data) =
            createVocabularyCalls.Add(
                { CollectionId = data.CollectionId
                  Name = data.Name
                  Description = data.Description
                  CreatedAt = data.CreatedAt }
            )

            createVocabulary(data)

    interface IGetVocabularyById with
        member _.GetVocabularyById(id) =
            getVocabularyByIdCalls.Add(id)
            getVocabularyById id

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let makeCollection id userId =
    { Id = CollectionId id
      UserId = UserId userId
      Name = "Test Collection"
      Description = None
      CreatedAt = DateTimeOffset.UtcNow
      UpdatedAt = None }

let makeVocabulary id collectionId name description createdAt =
    { Id = VocabularyId id
      CollectionId = CollectionId collectionId
      Name = name
      Description = description
      CreatedAt = createdAt
      UpdatedAt = None }

[<Fact>]
let ``creates vocabulary with valid name``() =
    task {
        let now = DateTimeOffset.UtcNow
        let collection = makeCollection 1 1

        let createdVocabulary =
            makeVocabulary 1 1 "Test Vocabulary" None now

        let env =
            TestEnv(
                getCollectionById =
                    (fun id ->
                        if id <> CollectionId 1 then
                            failwith $"Unexpected collection id: {id}"

                        Task.FromResult(Some collection)),
                createVocabulary =
                    (fun data ->
                        if data.CollectionId <> CollectionId 1 then
                            failwith $"Unexpected collectionId: {data.CollectionId}"

                        if data.Name <> "Test Vocabulary" then
                            failwith $"Unexpected name: {data.Name}"

                        if data.Description <> None then
                            failwith $"Unexpected description: {data.Description}"

                        if data.CreatedAt <> now then
                            failwith $"Unexpected createdAt: {data.CreatedAt}"

                        Task.FromResult(VocabularyId 1)),
                getVocabularyById =
                    (fun id ->
                        if id <> VocabularyId 1 then
                            failwith $"Unexpected vocabulary id: {id}"

                        Task.FromResult(Some createdVocabulary))
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = "Test Vocabulary"
                  Description = None
                  CreatedAt = now }

        Assert.Equal(Ok createdVocabulary, result)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Equal(1, env.CreateVocabularyCalls.Length)
        Assert.Equal<VocabularyId list>([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
    }

[<Fact>]
let ``creates vocabulary with description``() =
    task {
        let now = DateTimeOffset.UtcNow
        let collection = makeCollection 1 1
        let description = Some "A test description"

        let createdVocabulary =
            makeVocabulary 1 1 "Test Vocabulary" description now

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                createVocabulary =
                    (fun data ->
                        if data.Description <> description then
                            failwith $"Unexpected description: {data.Description}"

                        Task.FromResult(VocabularyId 1)),
                getVocabularyById = (fun _ -> Task.FromResult(Some createdVocabulary))
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = "Test Vocabulary"
                  Description = description
                  CreatedAt = now }

        Assert.Equal(Ok createdVocabulary, result)
    }

[<Fact>]
let ``trims whitespace from name``() =
    task {
        let now = DateTimeOffset.UtcNow
        let collection = makeCollection 1 1

        let createdVocabulary =
            makeVocabulary 1 1 "Test Vocabulary" None now

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                createVocabulary =
                    (fun data ->
                        if data.Name <> "Test Vocabulary" then
                            failwith $"Expected trimmed name 'Test Vocabulary', got: '{data.Name}'"

                        Task.FromResult(VocabularyId 1)),
                getVocabularyById = (fun _ -> Task.FromResult(Some createdVocabulary))
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = "  Test Vocabulary  "
                  Description = None
                  CreatedAt = now }

        Assert.True(Result.isOk result)

        let call =
            env.CreateVocabularyCalls |> List.head

        Assert.Equal("Test Vocabulary", call.Name)
    }

[<Fact>]
let ``returns CollectionNotFound when collection does not exist``() =
    task {
        let env =
            TestEnv(
                getCollectionById =
                    (fun id ->
                        if id <> CollectionId 1 then
                            failwith $"Unexpected collection id: {id}"

                        Task.FromResult(None)),
                createVocabulary = (fun _ -> failwith "Should not be called"),
                getVocabularyById = (fun _ -> failwith "Should not be called")
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = "Test Vocabulary"
                  Description = None
                  CreatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(CreateVocabularyError.VocabularyCollectionNotFound(CollectionId 1)), result)
        Assert.Empty(env.CreateVocabularyCalls)
    }

[<Fact>]
let ``returns CollectionNotFound when collection owned by different user``() =
    task {
        let collection = makeCollection 1 2

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                createVocabulary = (fun _ -> failwith "Should not be called"),
                getVocabularyById = (fun _ -> failwith "Should not be called")
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = "Test Vocabulary"
                  Description = None
                  CreatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(CreateVocabularyError.VocabularyCollectionNotFound(CollectionId 1)), result)
        Assert.Empty(env.CreateVocabularyCalls)
    }

[<Fact>]
let ``returns error when name is empty``() =
    task {
        let collection = makeCollection 1 1

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                createVocabulary = (fun _ -> failwith "Should not be called"),
                getVocabularyById = (fun _ -> failwith "Should not be called")
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = ""
                  Description = None
                  CreatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error CreateVocabularyError.VocabularyNameRequired, result)
        Assert.Empty(env.CreateVocabularyCalls)
    }

[<Fact>]
let ``returns error when name is whitespace only``() =
    task {
        let collection = makeCollection 1 1

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                createVocabulary = (fun _ -> failwith "Should not be called"),
                getVocabularyById = (fun _ -> failwith "Should not be called")
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = "   "
                  Description = None
                  CreatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error CreateVocabularyError.VocabularyNameRequired, result)
        Assert.Empty(env.CreateVocabularyCalls)
    }

[<Fact>]
let ``returns error when name exceeds max length``() =
    task {
        let collection = makeCollection 1 1
        let longName = String.replicate 256 "a"

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                createVocabulary = (fun _ -> failwith "Should not be called"),
                getVocabularyById = (fun _ -> failwith "Should not be called")
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = longName
                  Description = None
                  CreatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(CreateVocabularyError.VocabularyNameTooLong MaxNameLength), result)
        Assert.Empty(env.CreateVocabularyCalls)
    }

[<Fact>]
let ``accepts name at exact max length``() =
    task {
        let now = DateTimeOffset.UtcNow
        let collection = makeCollection 1 1

        let maxLengthName =
            String.replicate MaxNameLength "a"

        let createdVocabulary =
            makeVocabulary 1 1 maxLengthName None now

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                createVocabulary =
                    (fun data ->
                        if data.Name <> maxLengthName then
                            failwith $"Expected name with {MaxNameLength} characters"

                        Task.FromResult(VocabularyId 1)),
                getVocabularyById = (fun _ -> Task.FromResult(Some createdVocabulary))
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = maxLengthName
                  Description = None
                  CreatedAt = now }

        Assert.Equal(Ok createdVocabulary, result)
    }

[<Fact>]
let ``throws when post-creation vocabulary fetch returns None``() =
    task {
        let now = DateTimeOffset.UtcNow
        let collection = makeCollection 1 1

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                createVocabulary = (fun _ -> Task.FromResult(VocabularyId 1)),
                getVocabularyById = (fun _ -> Task.FromResult(None))
            )

        let! ex =
            Assert.ThrowsAsync<Exception>(fun () ->
                create
                    env
                    { UserId = UserId 1
                      CollectionId = CollectionId 1
                      Name = "Test Vocabulary"
                      Description = None
                      CreatedAt = now }
                :> Task)

        Assert.Equal("Vocabulary VocabularyId 1 not found after creation", ex.Message)
    }
