module Wordfolio.Api.Domain.Tests.Collections.CreateTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Collections.Operations

type CreateCollectionCall =
    { UserId: UserId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset }

type TestEnv
    (
        createCollection: CreateCollectionData -> Task<CollectionId>,
        getCollectionById: CollectionId -> Task<Collection option>
    ) =
    let createCollectionCalls =
        ResizeArray<CreateCollectionCall>()

    let getCollectionByIdCalls =
        ResizeArray<CollectionId>()

    member _.CreateCollectionCalls =
        createCollectionCalls |> Seq.toList

    member _.GetCollectionByIdCalls =
        getCollectionByIdCalls |> Seq.toList

    interface ICreateCollection with
        member _.CreateCollection(parameters) =
            createCollectionCalls.Add(
                { UserId = parameters.UserId
                  Name = parameters.Name
                  Description = parameters.Description
                  CreatedAt = parameters.CreatedAt }
            )

            createCollection(parameters)

    interface IGetCollectionById with
        member _.GetCollectionById(id) =
            getCollectionByIdCalls.Add(id)
            getCollectionById id

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let makeCollection id userId name description createdAt =
    { Id = CollectionId id
      UserId = UserId userId
      Name = name
      Description = description
      CreatedAt = createdAt
      UpdatedAt = None }

[<Fact>]
let ``creates collection with valid name``() =
    task {
        let now = DateTimeOffset.UtcNow

        let createdCollection =
            makeCollection 1 1 "Test Collection" None now

        let env =
            TestEnv(
                createCollection =
                    (fun parameters ->
                        if parameters.UserId <> UserId 1 then
                            failwith $"Unexpected userId: {parameters.UserId}"

                        if parameters.Name <> "Test Collection" then
                            failwith $"Unexpected name: {parameters.Name}"

                        if parameters.Description <> None then
                            failwith $"Unexpected description: {parameters.Description}"

                        if parameters.CreatedAt <> now then
                            failwith $"Unexpected createdAt: {parameters.CreatedAt}"

                        Task.FromResult(CollectionId 1)),
                getCollectionById =
                    (fun id ->
                        if id <> CollectionId 1 then
                            failwith $"Unexpected id: {id}"

                        Task.FromResult(Some createdCollection))
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  Name = "Test Collection"
                  Description = None
                  CreatedAt = now }

        Assert.Equal(Ok createdCollection, result)
        Assert.Equal(1, env.CreateCollectionCalls.Length)
        Assert.Equal<CollectionId list>([ CollectionId 1 ], env.GetCollectionByIdCalls)
    }

[<Fact>]
let ``creates collection with description``() =
    task {
        let now = DateTimeOffset.UtcNow
        let description = Some "A test description"

        let createdCollection =
            makeCollection 1 1 "Test Collection" description now

        let env =
            TestEnv(
                createCollection =
                    (fun parameters ->
                        if parameters.Description <> description then
                            failwith $"Unexpected description: {parameters.Description}"

                        Task.FromResult(CollectionId 1)),
                getCollectionById = (fun _ -> Task.FromResult(Some createdCollection))
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  Name = "Test Collection"
                  Description = description
                  CreatedAt = now }

        Assert.Equal(Ok createdCollection, result)
    }

[<Fact>]
let ``trims whitespace from name``() =
    task {
        let now = DateTimeOffset.UtcNow

        let createdCollection =
            makeCollection 1 1 "Test Collection" None now

        let env =
            TestEnv(
                createCollection =
                    (fun parameters ->
                        if parameters.Name <> "Test Collection" then
                            failwith $"Expected trimmed name 'Test Collection', got: '{parameters.Name}'"

                        Task.FromResult(CollectionId 1)),
                getCollectionById = (fun _ -> Task.FromResult(Some createdCollection))
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  Name = "  Test Collection  "
                  Description = None
                  CreatedAt = now }

        Assert.True(Result.isOk result)

        let call =
            env.CreateCollectionCalls |> List.head

        Assert.Equal("Test Collection", call.Name)
    }

[<Fact>]
let ``returns error when name is empty``() =
    task {
        let env =
            TestEnv(
                createCollection = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> failwith "Should not be called")
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  Name = ""
                  Description = None
                  CreatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error CreateCollectionError.CollectionNameRequired, result)
        Assert.Empty(env.CreateCollectionCalls)
    }

[<Fact>]
let ``returns error when name is whitespace only``() =
    task {
        let env =
            TestEnv(
                createCollection = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> failwith "Should not be called")
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  Name = "   "
                  Description = None
                  CreatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error CreateCollectionError.CollectionNameRequired, result)
        Assert.Empty(env.CreateCollectionCalls)
    }

[<Fact>]
let ``returns error when name exceeds max length``() =
    task {
        let longName = String.replicate 256 "a"

        let env =
            TestEnv(
                createCollection = (fun _ -> failwith "Should not be called"),
                getCollectionById = (fun _ -> failwith "Should not be called")
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  Name = longName
                  Description = None
                  CreatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(CreateCollectionError.CollectionNameTooLong MaxNameLength), result)
        Assert.Empty(env.CreateCollectionCalls)
    }

[<Fact>]
let ``accepts name at exact max length``() =
    task {
        let now = DateTimeOffset.UtcNow

        let maxLengthName =
            String.replicate MaxNameLength "a"

        let createdCollection =
            makeCollection 1 1 maxLengthName None now

        let env =
            TestEnv(
                createCollection =
                    (fun parameters ->
                        if parameters.Name <> maxLengthName then
                            failwith $"Expected name with {MaxNameLength} characters"

                        Task.FromResult(CollectionId 1)),
                getCollectionById = (fun _ -> Task.FromResult(Some createdCollection))
            )

        let! result =
            create
                env
                { UserId = UserId 1
                  Name = maxLengthName
                  Description = None
                  CreatedAt = now }

        Assert.Equal(Ok createdCollection, result)
    }

[<Fact>]
let ``throws when post-creation collection fetch returns None``() =
    task {
        let now = DateTimeOffset.UtcNow

        let env =
            TestEnv(
                createCollection = (fun _ -> Task.FromResult(CollectionId 1)),
                getCollectionById = (fun _ -> Task.FromResult(None))
            )

        let! ex =
            Assert.ThrowsAsync<Exception>(fun () ->
                create
                    env
                    { UserId = UserId 1
                      Name = "Test Collection"
                      Description = None
                      CreatedAt = now }
                :> Task)

        Assert.Equal("Collection CollectionId 1 not found after creation", ex.Message)
    }
