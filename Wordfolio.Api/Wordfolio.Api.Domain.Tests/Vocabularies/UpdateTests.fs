module Wordfolio.Api.Domain.Tests.Vocabularies.UpdateTests

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
        updateVocabulary: UpdateVocabularyData -> Task<int>
    ) =
    let getVocabularyByIdCalls =
        ResizeArray<VocabularyId>()

    let getCollectionByIdCalls =
        ResizeArray<CollectionId>()

    let updateVocabularyCalls =
        ResizeArray<UpdateVocabularyData>()

    member _.GetVocabularyByIdCalls =
        getVocabularyByIdCalls |> Seq.toList

    member _.GetCollectionByIdCalls =
        getCollectionByIdCalls |> Seq.toList

    member _.UpdateVocabularyCalls =
        updateVocabularyCalls |> Seq.toList

    interface IGetVocabularyById with
        member _.GetVocabularyById(id) =
            getVocabularyByIdCalls.Add(id)
            getVocabularyById id

    interface IGetCollectionById with
        member _.GetCollectionById(id) =
            getCollectionByIdCalls.Add(id)
            getCollectionById id

    interface IUpdateVocabulary with
        member _.UpdateVocabulary(data) =
            updateVocabularyCalls.Add(data)
            updateVocabulary(data)

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let makeCollection id userId =
    { Id = CollectionId id
      UserId = UserId userId
      Name = "Test Collection"
      Description = None
      CreatedAt = DateTimeOffset.UtcNow
      UpdatedAt = None }

let makeVocabulary id collectionId name description =
    { Id = VocabularyId id
      CollectionId = CollectionId collectionId
      Name = name
      Description = description
      CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
      UpdatedAt = None }

[<Fact>]
let ``updates vocabulary when collection owned by user``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingVocabulary =
            makeVocabulary 1 1 "Old Name" None

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
                updateVocabulary =
                    (fun data ->
                        if data.VocabularyId <> VocabularyId 1 then
                            failwith $"Unexpected vocabularyId: {data.VocabularyId}"

                        if data.Name <> "New Name" then
                            failwith $"Unexpected name: {data.Name}"

                        if
                            data.Description
                            <> Some "New Description"
                        then
                            failwith $"Unexpected description: {data.Description}"

                        if data.UpdatedAt <> now then
                            failwith $"Unexpected updatedAt: {data.UpdatedAt}"

                        Task.FromResult(1))
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  VocabularyId = VocabularyId 1
                  Name = "New Name"
                  Description = Some "New Description"
                  UpdatedAt = now }

        match result with
        | Ok updated ->
            Assert.Equal("New Name", updated.Name)
            Assert.Equal(Some "New Description", updated.Description)
            Assert.Equal(Some now, updated.UpdatedAt)
        | Error e -> failwith $"Expected Ok, got Error: {e}"

        Assert.Equal<VocabularyId list>([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)

        Assert.Equal<UpdateVocabularyData list>(
            [ { VocabularyId = VocabularyId 1
                Name = "New Name"
                Description = Some "New Description"
                UpdatedAt = now } ],
            env.UpdateVocabularyCalls
        )
    }

[<Fact>]
let ``trims whitespace from name``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingVocabulary =
            makeVocabulary 1 1 "Old Name" None

        let collection = makeCollection 1 1

        let env =
            TestEnv(
                getVocabularyById = (fun _ -> Task.FromResult(Some existingVocabulary)),
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                updateVocabulary =
                    (fun data ->
                        if data.Name <> "New Name" then
                            failwith $"Expected trimmed name 'New Name', got: '{data.Name}'"

                        Task.FromResult(1))
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  VocabularyId = VocabularyId 1
                  Name = "  New Name  "
                  Description = None
                  UpdatedAt = now }

        Assert.True(Result.isOk result)

        let call =
            env.UpdateVocabularyCalls |> List.head

        Assert.Equal("New Name", call.Name)
        Assert.Equal<VocabularyId list>([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Equal(1, env.UpdateVocabularyCalls.Length)
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
                getCollectionById = (fun _ -> failwith "Should not be called"),
                updateVocabulary = (fun _ -> failwith "Should not be called")
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  VocabularyId = VocabularyId 1
                  Name = "New Name"
                  Description = None
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateVocabularyError.VocabularyNotFound(VocabularyId 1)), result)
        Assert.Equal<VocabularyId list>([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
        Assert.Empty(env.GetCollectionByIdCalls)
        Assert.Empty(env.UpdateVocabularyCalls)
    }

[<Fact>]
let ``returns AccessDenied when collection owned by different user``() =
    task {
        let existingVocabulary =
            makeVocabulary 1 1 "Test Vocabulary" None

        let collection = makeCollection 1 2

        let env =
            TestEnv(
                getVocabularyById = (fun _ -> Task.FromResult(Some existingVocabulary)),
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                updateVocabulary = (fun _ -> failwith "Should not be called")
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  VocabularyId = VocabularyId 1
                  Name = "New Name"
                  Description = None
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateVocabularyError.VocabularyAccessDenied(VocabularyId 1)), result)
        Assert.Equal<VocabularyId list>([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Empty(env.UpdateVocabularyCalls)
    }

[<Fact>]
let ``returns AccessDenied when collection does not exist``() =
    task {
        let existingVocabulary =
            makeVocabulary 1 1 "Test Vocabulary" None

        let env =
            TestEnv(
                getVocabularyById = (fun _ -> Task.FromResult(Some existingVocabulary)),
                getCollectionById = (fun _ -> Task.FromResult(None)),
                updateVocabulary = (fun _ -> failwith "Should not be called")
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  VocabularyId = VocabularyId 1
                  Name = "New Name"
                  Description = None
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateVocabularyError.VocabularyAccessDenied(VocabularyId 1)), result)
        Assert.Equal<VocabularyId list>([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Empty(env.UpdateVocabularyCalls)
    }

[<Fact>]
let ``returns error when name is empty``() =
    task {
        let existingVocabulary =
            makeVocabulary 1 1 "Test Vocabulary" None

        let collection = makeCollection 1 1

        let env =
            TestEnv(
                getVocabularyById = (fun _ -> Task.FromResult(Some existingVocabulary)),
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                updateVocabulary = (fun _ -> failwith "Should not be called")
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  VocabularyId = VocabularyId 1
                  Name = ""
                  Description = None
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error UpdateVocabularyError.VocabularyNameRequired, result)
        Assert.Equal<VocabularyId list>([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Empty(env.UpdateVocabularyCalls)
    }

[<Fact>]
let ``returns error when name exceeds max length``() =
    task {
        let existingVocabulary =
            makeVocabulary 1 1 "Test Vocabulary" None

        let collection = makeCollection 1 1
        let longName = String.replicate 256 "a"

        let env =
            TestEnv(
                getVocabularyById = (fun _ -> Task.FromResult(Some existingVocabulary)),
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                updateVocabulary = (fun _ -> failwith "Should not be called")
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  VocabularyId = VocabularyId 1
                  Name = longName
                  Description = None
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateVocabularyError.VocabularyNameTooLong MaxNameLength), result)
        Assert.Equal<VocabularyId list>([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Empty(env.UpdateVocabularyCalls)
    }

[<Fact>]
let ``returns error when name is whitespace only``() =
    task {
        let existingVocabulary =
            makeVocabulary 1 1 "Test Vocabulary" None

        let collection = makeCollection 1 1

        let env =
            TestEnv(
                getVocabularyById = (fun _ -> Task.FromResult(Some existingVocabulary)),
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                updateVocabulary = (fun _ -> failwith "Should not be called")
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  VocabularyId = VocabularyId 1
                  Name = "   "
                  Description = None
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error UpdateVocabularyError.VocabularyNameRequired, result)
        Assert.Equal<VocabularyId list>([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Empty(env.UpdateVocabularyCalls)
    }

[<Fact>]
let ``accepts name at exact max length``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingVocabulary =
            makeVocabulary 1 1 "Old Name" None

        let collection = makeCollection 1 1

        let maxLengthName =
            String.replicate MaxNameLength "a"

        let env =
            TestEnv(
                getVocabularyById = (fun _ -> Task.FromResult(Some existingVocabulary)),
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                updateVocabulary =
                    (fun data ->
                        if data.Name <> maxLengthName then
                            failwith $"Expected name with {MaxNameLength} characters"

                        Task.FromResult(1))
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  VocabularyId = VocabularyId 1
                  Name = maxLengthName
                  Description = None
                  UpdatedAt = now }

        Assert.True(Result.isOk result)
        Assert.Equal<VocabularyId list>([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)

        Assert.Equal<UpdateVocabularyData list>(
            [ { VocabularyId = VocabularyId 1
                Name = maxLengthName
                Description = None
                UpdatedAt = now } ],
            env.UpdateVocabularyCalls
        )
    }

[<Fact>]
let ``returns NotFound when update affects no rows``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingVocabulary =
            makeVocabulary 1 1 "Test Vocabulary" None

        let collection = makeCollection 1 1

        let env =
            TestEnv(
                getVocabularyById = (fun _ -> Task.FromResult(Some existingVocabulary)),
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                updateVocabulary = (fun _ -> Task.FromResult(0))
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  VocabularyId = VocabularyId 1
                  Name = "New Name"
                  Description = None
                  UpdatedAt = now }

        Assert.Equal(Error(UpdateVocabularyError.VocabularyNotFound(VocabularyId 1)), result)
        Assert.Equal<VocabularyId list>([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)

        Assert.Equal<UpdateVocabularyData list>(
            [ { VocabularyId = VocabularyId 1
                Name = "New Name"
                Description = None
                UpdatedAt = now } ],
            env.UpdateVocabularyCalls
        )
    }
