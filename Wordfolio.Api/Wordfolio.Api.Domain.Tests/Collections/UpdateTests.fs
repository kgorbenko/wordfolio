module Wordfolio.Api.Domain.Tests.Collections.UpdateTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Shared
open Wordfolio.Api.Domain.Collections.Operations

type UpdateCollectionCall =
    { CollectionId: CollectionId
      Name: string
      Description: string option
      UpdatedAt: DateTimeOffset }

type TestEnv
    (
        getCollectionById: CollectionId -> Task<Collection option>,
        updateCollection: CollectionId * string * string option * DateTimeOffset -> Task<int>
    ) =
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
        member _.UpdateCollection(collectionId, name, description, updatedAt) =
            updateCollectionCalls.Add(
                { CollectionId = collectionId
                  Name = name
                  Description = description
                  UpdatedAt = updatedAt }
            )

            updateCollection(collectionId, name, description, updatedAt)

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
                    (fun (collectionId, name, description, updatedAt) ->
                        if collectionId <> CollectionId 1 then
                            failwith $"Unexpected collectionId: {collectionId}"

                        if name <> "New Name" then
                            failwith $"Unexpected name: {name}"

                        if description <> Some "New Description" then
                            failwith $"Unexpected description: {description}"

                        if updatedAt <> now then
                            failwith $"Unexpected updatedAt: {updatedAt}"

                        Task.FromResult(1))
            )

        let! result = update env (UserId 1) (CollectionId 1) "New Name" (Some "New Description") now

        match result with
        | Ok updated ->
            Assert.Equal("New Name", updated.Name)
            Assert.Equal(Some "New Description", updated.Description)
            Assert.Equal(Some now, updated.UpdatedAt)
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
                    (fun (_, name, _, _) ->
                        if name <> "New Name" then
                            failwith $"Expected trimmed name 'New Name', got: '{name}'"

                        Task.FromResult(1))
            )

        let! result = update env (UserId 1) (CollectionId 1) "  New Name  " None now

        Assert.True(Result.isOk result)

        let call =
            env.UpdateCollectionCalls |> List.head

        Assert.Equal("New Name", call.Name)
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

        let! result = update env (UserId 1) (CollectionId 1) "New Name" None DateTimeOffset.UtcNow

        Assert.Equal(Error(CollectionNotFound(CollectionId 1)), result)
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

        let! result = update env (UserId 1) (CollectionId 1) "New Name" None DateTimeOffset.UtcNow

        Assert.Equal(Error(CollectionAccessDenied(CollectionId 1)), result)
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

        let! result = update env (UserId 1) (CollectionId 1) "" None DateTimeOffset.UtcNow

        Assert.Equal(Error CollectionNameRequired, result)
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

        let! result = update env (UserId 1) (CollectionId 1) longName None DateTimeOffset.UtcNow

        Assert.Equal(Error(CollectionNameTooLong MaxNameLength), result)
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

        let! result = update env (UserId 1) (CollectionId 1) "   " None DateTimeOffset.UtcNow

        Assert.Equal(Error CollectionNameRequired, result)
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
                    (fun (_, name, _, _) ->
                        if name <> maxLengthName then
                            failwith $"Expected name with {MaxNameLength} characters"

                        Task.FromResult(1))
            )

        let! result = update env (UserId 1) (CollectionId 1) maxLengthName None now

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

        let! result = update env (UserId 1) (CollectionId 1) "New Name" None DateTimeOffset.UtcNow

        Assert.Equal(Error(CollectionNotFound(CollectionId 1)), result)
        Assert.Equal(1, env.UpdateCollectionCalls.Length)
    }
