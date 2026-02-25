module Wordfolio.Api.Domain.Tests.Collections.UpdateTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Collections.Operations

type UpdateCollectionCall =
    { CollectionId: CollectionId
      Name: string
      Description: string option
      UpdatedAt: DateTimeOffset }

type TestEnv
    (getCollectionById: CollectionId -> Task<Collection option>, updateCollection: UpdateCollectionData -> Task<int>) =
    let getCollectionByIdCalls =
        ResizeArray<CollectionId>()

    let updateCollectionCalls =
        ResizeArray<UpdateCollectionCall>()

    member _.GetCollectionByIdCalls =
        getCollectionByIdCalls |> Seq.toList

    member _.UpdateCollectionCalls =
        updateCollectionCalls |> Seq.toList

    interface IGetCollectionById with
        member _.GetCollectionById(id) =
            getCollectionByIdCalls.Add(id)
            getCollectionById id

    interface IUpdateCollection with
        member _.UpdateCollection(parameters) =
            updateCollectionCalls.Add(
                { CollectionId = parameters.CollectionId
                  Name = parameters.Name
                  Description = parameters.Description
                  UpdatedAt = parameters.UpdatedAt }
            )

            updateCollection(parameters)

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let makeCollection id userId name description =
    { Id = CollectionId id
      UserId = UserId userId
      Name = name
      Description = description
      CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
      UpdatedAt = None }

[<Fact>]
let ``updates collection when owned by user``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingCollection =
            makeCollection 1 1 "Old Name" None

        let env =
            TestEnv(
                getCollectionById =
                    (fun id ->
                        if id <> CollectionId 1 then
                            failwith $"Unexpected id: {id}"

                        Task.FromResult(Some existingCollection)),
                updateCollection =
                    (fun parameters ->
                        if
                            parameters.CollectionId
                            <> CollectionId 1
                        then
                            failwith $"Unexpected collectionId: {parameters.CollectionId}"

                        if parameters.Name <> "New Name" then
                            failwith $"Unexpected name: {parameters.Name}"

                        if
                            parameters.Description
                            <> Some "New Description"
                        then
                            failwith $"Unexpected description: {parameters.Description}"

                        if parameters.UpdatedAt <> now then
                            failwith $"Unexpected updatedAt: {parameters.UpdatedAt}"

                        Task.FromResult(1))
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = "New Name"
                  Description = Some "New Description"
                  UpdatedAt = now }

        match result with
        | Ok updated ->
            Assert.Equal(CollectionId 1, updated.Id)
            Assert.Equal(UserId 1, updated.UserId)
            Assert.Equal("New Name", updated.Name)
            Assert.Equal(Some "New Description", updated.Description)
            Assert.Equal(Some now, updated.UpdatedAt)
            Assert.Equal(existingCollection.CreatedAt, updated.CreatedAt)
        | Error e -> failwith $"Expected Ok, got Error: {e}"

        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
        Assert.Equal(1, env.UpdateCollectionCalls.Length)
    }

[<Fact>]
let ``trims whitespace from name``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingCollection =
            makeCollection 1 1 "Old Name" None

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some existingCollection)),
                updateCollection =
                    (fun parameters ->
                        if parameters.Name <> "New Name" then
                            failwith $"Expected trimmed name 'New Name', got: '{parameters.Name}'"

                        Task.FromResult(1))
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = "  New Name  "
                  Description = None
                  UpdatedAt = now }

        Assert.True(Result.isOk result)

        let call =
            env.UpdateCollectionCalls |> List.head

        Assert.Equal("New Name", call.Name)
    }

[<Fact>]
let ``clears description when updated to None``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingCollection =
            makeCollection 1 1 "Test Collection" (Some "old description")

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some existingCollection)),
                updateCollection = (fun _ -> Task.FromResult(1))
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = "Test Collection"
                  Description = None
                  UpdatedAt = now }

        match result with
        | Ok updated -> Assert.Equal(None, updated.Description)
        | Error e -> failwith $"Expected Ok, got Error: {e}"
    }

[<Fact>]
let ``returns NotFound when collection does not exist``() =
    task {
        let env =
            TestEnv(
                getCollectionById =
                    (fun id ->
                        if id <> CollectionId 1 then
                            failwith $"Unexpected id: {id}"

                        Task.FromResult(None)),
                updateCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = "New Name"
                  Description = None
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateCollectionError.CollectionNotFound(CollectionId 1)), result)
        Assert.Empty(env.UpdateCollectionCalls)
    }

[<Fact>]
let ``returns AccessDenied when collection owned by different user``() =
    task {
        let existingCollection =
            makeCollection 1 2 "Test Collection" None

        let env =
            TestEnv(
                getCollectionById =
                    (fun id ->
                        if id <> CollectionId 1 then
                            failwith $"Unexpected id: {id}"

                        Task.FromResult(Some existingCollection)),
                updateCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = "New Name"
                  Description = None
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateCollectionError.CollectionAccessDenied(CollectionId 1)), result)
        Assert.Empty(env.UpdateCollectionCalls)
    }

[<Fact>]
let ``returns error when name is empty``() =
    task {
        let existingCollection =
            makeCollection 1 1 "Test Collection" None

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some existingCollection)),
                updateCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = ""
                  Description = None
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error UpdateCollectionError.CollectionNameRequired, result)
        Assert.Empty(env.UpdateCollectionCalls)
    }

[<Fact>]
let ``returns error when name exceeds max length``() =
    task {
        let existingCollection =
            makeCollection 1 1 "Test Collection" None

        let longName = String.replicate 256 "a"

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some existingCollection)),
                updateCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = longName
                  Description = None
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateCollectionError.CollectionNameTooLong MaxNameLength), result)
        Assert.Empty(env.UpdateCollectionCalls)
    }

[<Fact>]
let ``returns error when name is whitespace only``() =
    task {
        let existingCollection =
            makeCollection 1 1 "Test Collection" None

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some existingCollection)),
                updateCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = "   "
                  Description = None
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error UpdateCollectionError.CollectionNameRequired, result)
        Assert.Empty(env.UpdateCollectionCalls)
    }

[<Fact>]
let ``accepts name at exact max length``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingCollection =
            makeCollection 1 1 "Old Name" None

        let maxLengthName =
            String.replicate MaxNameLength "a"

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some existingCollection)),
                updateCollection =
                    (fun parameters ->
                        if parameters.Name <> maxLengthName then
                            failwith $"Expected name with {MaxNameLength} characters"

                        Task.FromResult(1))
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = maxLengthName
                  Description = None
                  UpdatedAt = now }

        Assert.True(Result.isOk result)
    }

[<Fact>]
let ``returns NotFound when update affects no rows``() =
    task {
        let existingCollection =
            makeCollection 1 1 "Test Collection" None

        let env =
            TestEnv(
                getCollectionById = (fun _ -> Task.FromResult(Some existingCollection)),
                updateCollection = (fun _ -> Task.FromResult(0))
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = "New Name"
                  Description = None
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateCollectionError.CollectionNotFound(CollectionId 1)), result)
        Assert.Equal(1, env.UpdateCollectionCalls.Length)
    }
