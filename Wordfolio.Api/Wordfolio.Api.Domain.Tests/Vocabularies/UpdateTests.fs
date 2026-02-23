module Wordfolio.Api.Domain.Tests.Vocabularies.UpdateTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Vocabularies
open Wordfolio.Api.Domain.Vocabularies.Operations

type UpdateVocabularyCall =
    { VocabularyId: VocabularyId
      Name: string
      Description: string option
      UpdatedAt: DateTimeOffset }

type TestEnv
    (
        getVocabularyById: VocabularyId -> Task<Vocabulary option>,
        getCollectionById: CollectionId -> Task<Collection option>,
        updateVocabulary: VocabularyId * string * string option * DateTimeOffset -> Task<int>
    ) =
    let getVocabularyByIdCalls =
        ResizeArray<VocabularyId>()

    let getCollectionByIdCalls =
        ResizeArray<CollectionId>()

    let updateVocabularyCalls =
        ResizeArray<UpdateVocabularyCall>()

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
        member _.UpdateVocabulary(vocabularyId, name, description, updatedAt) =
            updateVocabularyCalls.Add(
                { VocabularyId = vocabularyId
                  Name = name
                  Description = description
                  UpdatedAt = updatedAt }
            )

            updateVocabulary(vocabularyId, name, description, updatedAt)

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
                    (fun (vocabularyId, name, description, updatedAt) ->
                        if vocabularyId <> VocabularyId 1 then
                            failwith $"Unexpected vocabularyId: {vocabularyId}"

                        if name <> "New Name" then
                            failwith $"Unexpected name: {name}"

                        if description <> Some "New Description" then
                            failwith $"Unexpected description: {description}"

                        if updatedAt <> now then
                            failwith $"Unexpected updatedAt: {updatedAt}"

                        Task.FromResult(1))
            )

        let! result = update env (UserId 1) (VocabularyId 1) "New Name" (Some "New Description") now

        match result with
        | Ok updated ->
            Assert.Equal("New Name", updated.Name)
            Assert.Equal(Some "New Description", updated.Description)
            Assert.Equal(Some now, updated.UpdatedAt)
        | Error e -> failwith $"Expected Ok, got Error: {e}"

        Assert.Equal<VocabularyId list>([ VocabularyId 1 ], env.GetVocabularyByIdCalls)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Equal(1, env.UpdateVocabularyCalls.Length)
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
                    (fun (_, name, _, _) ->
                        if name <> "New Name" then
                            failwith $"Expected trimmed name 'New Name', got: '{name}'"

                        Task.FromResult(1))
            )

        let! result = update env (UserId 1) (VocabularyId 1) "  New Name  " None now

        Assert.True(Result.isOk result)

        let call =
            env.UpdateVocabularyCalls |> List.head

        Assert.Equal("New Name", call.Name)
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

        let! result = update env (UserId 1) (VocabularyId 1) "New Name" None DateTimeOffset.UtcNow

        Assert.Equal(Error(VocabularyNotFound(VocabularyId 1)), result)
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

        let! result = update env (UserId 1) (VocabularyId 1) "New Name" None DateTimeOffset.UtcNow

        Assert.Equal(Error(VocabularyAccessDenied(VocabularyId 1)), result)
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

        let! result = update env (UserId 1) (VocabularyId 1) "New Name" None DateTimeOffset.UtcNow

        Assert.Equal(Error(VocabularyAccessDenied(VocabularyId 1)), result)
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

        let! result = update env (UserId 1) (VocabularyId 1) "" None DateTimeOffset.UtcNow

        Assert.Equal(Error VocabularyNameRequired, result)
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

        let! result = update env (UserId 1) (VocabularyId 1) longName None DateTimeOffset.UtcNow

        Assert.Equal(Error(VocabularyNameTooLong MaxNameLength), result)
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

        let! result = update env (UserId 1) (VocabularyId 1) "   " None DateTimeOffset.UtcNow

        Assert.Equal(Error VocabularyNameRequired, result)
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
                    (fun (_, name, _, _) ->
                        if name <> maxLengthName then
                            failwith $"Expected name with {MaxNameLength} characters"

                        Task.FromResult(1))
            )

        let! result = update env (UserId 1) (VocabularyId 1) maxLengthName None now

        Assert.True(Result.isOk result)
    }

[<Fact>]
let ``returns NotFound when update affects no rows``() =
    task {
        let existingVocabulary =
            makeVocabulary 1 1 "Test Vocabulary" None

        let collection = makeCollection 1 1

        let env =
            TestEnv(
                getVocabularyById = (fun _ -> Task.FromResult(Some existingVocabulary)),
                getCollectionById = (fun _ -> Task.FromResult(Some collection)),
                updateVocabulary = (fun _ -> Task.FromResult(0))
            )

        let! result = update env (UserId 1) (VocabularyId 1) "New Name" None DateTimeOffset.UtcNow

        Assert.Equal(Error(VocabularyNotFound(VocabularyId 1)), result)
        Assert.Equal(1, env.UpdateVocabularyCalls.Length)
    }
