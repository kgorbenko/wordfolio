namespace Wordfolio.Api.DataAccess.Tests

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CollectionsTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``createCollectionAsync inserts a collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            do!
                Collections.createCollectionAsync
                    { UserId = user.Id
                      Name = "My Collection"
                      Description = Some "Test collection"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! actual = fixture.Seeder |> Seeder.getAllCollectionsAsync

            Assert.Single(actual) |> ignore

            let expected: Collection list =
                [ { Id = actual.[0].Id
                    UserId = user.Id
                    Name = "My Collection"
                    Description = Some "Test collection"
                    CreatedAt = createdAt
                    UpdatedAt = None } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getCollectionByIdAsync returns collection when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100
            let collection = Entities.makeCollection user "My Collection" (Some "Test collection") createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getCollectionByIdAsync collection.Id
                |> fixture.WithConnectionAsync

            let expected: Collections.Collection =
                { Id = collection.Id
                  UserId = user.Id
                  Name = "My Collection"
                  Description = Some "Test collection"
                  CreatedAt = createdAt
                  UpdatedAt = None }

            Assert.Equivalent(Some expected, actual)
        }

    [<Fact>]
    member _.``getCollectionByIdAsync returns None when collection does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                Collections.getCollectionByIdAsync 999
                |> fixture.WithConnectionAsync

            Assert.True(actual.IsNone)
        }

    [<Fact>]
    member _.``getCollectionsByUserIdAsync returns collections for user``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user1 = Entities.makeUser 100
            let user2 = Entities.makeUser 101
            let collection1 = Entities.makeCollection user1 "Collection 1" None createdAt None
            let collection2 = Entities.makeCollection user1 "Collection 2" None createdAt None
            let _ = Entities.makeCollection user2 "Collection 3" None createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user1; user2 ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getCollectionsByUserIdAsync user1.Id
                |> fixture.WithConnectionAsync

            let expected: Collections.Collection list =
                [ { Id = collection1.Id
                    UserId = user1.Id
                    Name = "Collection 1"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None }
                  { Id = collection2.Id
                    UserId = user1.Id
                    Name = "Collection 2"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getCollectionsByUserIdAsync returns empty list when user has no collections``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let user = Entities.makeUser 100

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getCollectionsByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``updateCollectionAsync updates an existing collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
            let updatedAt = DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 101
            let collection = Entities.makeCollection user "Original Name" (Some "Original Description") createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Collections.updateCollectionAsync
                    { Id = collection.Id
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual = Seeder.getCollectionByIdAsync collection.Id fixture.Seeder

            let expected: Collection =
                { Id = collection.Id
                  UserId = user.Id
                  Name = "Updated Name"
                  Description = Some "Updated Description"
                  CreatedAt = createdAt
                  UpdatedAt = Some updatedAt }

            Assert.Equal(Some expected, actual)
        }

    [<Fact>]
    member _.``updateCollectionAsync returns 0 when collection does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let updatedAt = DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let! affectedRows =
                Collections.updateCollectionAsync
                    { Id = 999
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``deleteCollectionAsync deletes an existing collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 102
            let collection = Entities.makeCollection user "Collection to delete" None createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Collections.deleteCollectionAsync collection.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual = fixture.Seeder |> Seeder.getAllCollectionsAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``deleteCollectionAsync returns 0 when collection does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! affectedRows =
                Collections.deleteCollectionAsync 999
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }
