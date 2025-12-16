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
        createCollection: UserId * string * string option * DateTimeOffset -> Task<CollectionId>,
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
        member _.CreateCollection(userId, name, description, createdAt) =
            createCollectionCalls.Add(
                { UserId = userId
                  Name = name
                  Description = description
                  CreatedAt = createdAt }
            )

            createCollection(userId, name, description, createdAt)

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
                    (fun (userId, name, description, createdAt) ->
                        if userId <> UserId 1 then
                            failwith $"Unexpected userId: {userId}"

                        if name <> "Test Collection" then
                            failwith $"Unexpected name: {name}"

                        if description <> None then
                            failwith $"Unexpected description: {description}"

                        if createdAt <> now then
                            failwith $"Unexpected createdAt: {createdAt}"

                        Task.FromResult(CollectionId 1)),
                getCollectionById =
                    (fun id ->
                        if id <> CollectionId 1 then
                            failwith $"Unexpected id: {id}"

                        Task.FromResult(Some createdCollection))
            )

        let! result = create env (UserId 1) "Test Collection" None now

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
                    (fun (userId, name, desc, createdAt) ->
                        if desc <> description then
                            failwith $"Unexpected description: {desc}"

                        Task.FromResult(CollectionId 1)),
                getCollectionById = (fun _ -> Task.FromResult(Some createdCollection))
            )

        let! result = create env (UserId 1) "Test Collection" description now

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
                    (fun (_, name, _, _) ->
                        if name <> "Test Collection" then
                            failwith $"Expected trimmed name 'Test Collection', got: '{name}'"

                        Task.FromResult(CollectionId 1)),
                getCollectionById = (fun _ -> Task.FromResult(Some createdCollection))
            )

        let! result = create env (UserId 1) "  Test Collection  " None now

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

        let! result = create env (UserId 1) "" None DateTimeOffset.UtcNow

        Assert.Equal(Error CollectionNameRequired, result)
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

        let! result = create env (UserId 1) "   " None DateTimeOffset.UtcNow

        Assert.Equal(Error CollectionNameRequired, result)
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

        let! result = create env (UserId 1) longName None DateTimeOffset.UtcNow

        Assert.Equal(Error(CollectionNameTooLong MaxNameLength), result)
        Assert.Empty(env.CreateCollectionCalls)
    }
