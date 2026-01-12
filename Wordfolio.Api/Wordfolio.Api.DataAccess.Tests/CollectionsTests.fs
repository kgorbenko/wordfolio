namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading.Tasks

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

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! createdId =
                Collections.createCollectionAsync
                    { UserId = user.Id
                      Name = "My Collection"
                      Description = Some "Test collection"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! actualCollection =
                fixture.Seeder
                |> Seeder.getCollectionByIdAsync createdId

            let expected: Collection option =
                Some
                    { Id = createdId
                      UserId = user.Id
                      Name = "My Collection"
                      Description = Some "Test collection"
                      CreatedAt = createdAt
                      UpdatedAt = None
                      IsSystem = false }

            Assert.Equivalent(expected, actualCollection)
        }

    [<Fact>]
    member _.``createCollectionAsync inserts a collection with None description``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 103

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! createdId =
                Collections.createCollectionAsync
                    { UserId = user.Id
                      Name = "My Collection"
                      Description = None
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! actualCollection =
                fixture.Seeder
                |> Seeder.getCollectionByIdAsync createdId

            let expected: Collection option =
                Some
                    { Id = createdId
                      UserId = user.Id
                      Name = "My Collection"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = None
                      IsSystem = false }

            Assert.Equivalent(expected, actualCollection)
        }

    [<Fact>]
    member _.``createCollectionAsync fails with foreign key violation for non-existent user``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let! ex =
                Assert.ThrowsAsync<Npgsql.PostgresException>(fun () ->
                    (Collections.createCollectionAsync
                        { UserId = 999
                          Name = "My Collection"
                          Description = Some "Test collection"
                          CreatedAt = createdAt }
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``getCollectionByIdAsync returns collection when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "My Collection" (Some "Test collection") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getCollectionByIdAsync collection.Id
                |> fixture.WithConnectionAsync

            let expected: Collections.Collection option =
                Some
                    { Id = collection.Id
                      UserId = user.Id
                      Name = "My Collection"
                      Description = Some "Test collection"
                      CreatedAt = createdAt
                      UpdatedAt = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getCollectionByIdAsync returns None when collection does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                Collections.getCollectionByIdAsync 999
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getCollectionsByUserIdAsync returns collections for user``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user1 = Entities.makeUser 100
            let user2 = Entities.makeUser 101

            let collection1 =
                Entities.makeCollection user1 "Collection 1" None createdAt None false

            let collection2 =
                Entities.makeCollection user1 "Collection 2" None createdAt None false

            let _ =
                Entities.makeCollection user2 "Collection 3" None createdAt None false

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

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 101

            let collection =
                Entities.makeCollection user "Original Name" (Some "Original Description") createdAt None false

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

            let expected: Collection option =
                Some
                    { Id = collection.Id
                      UserId = user.Id
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      CreatedAt = createdAt
                      UpdatedAt = Some updatedAt
                      IsSystem = false }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``updateCollectionAsync can clear description by setting it to None``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 104

            let collection =
                Entities.makeCollection user "Collection Name" (Some "Original Description") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Collections.updateCollectionAsync
                    { Id = collection.Id
                      Name = "Collection Name"
                      Description = None
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual = Seeder.getCollectionByIdAsync collection.Id fixture.Seeder

            let expected: Collection option =
                Some
                    { Id = collection.Id
                      UserId = user.Id
                      Name = "Collection Name"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = Some updatedAt
                      IsSystem = false }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``updateCollectionAsync returns 0 when collection does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

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

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 102

            let collection =
                Entities.makeCollection user "Collection to delete" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Collections.deleteCollectionAsync collection.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

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

    [<Fact>]
    member _.``getCollectionByIdAsync returns None when collection is system``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let systemCollection =
                Entities.makeCollection user "Unsorted" None createdAt None true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getCollectionByIdAsync systemCollection.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getCollectionsByUserIdAsync filters out system collections``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let systemCollection =
                Entities.makeCollection user "Unsorted" None createdAt None true

            let regularCollection =
                Entities.makeCollection user "Regular Collection" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ systemCollection; regularCollection ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getCollectionsByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: Collections.Collection list =
                [ { Id = regularCollection.Id
                    UserId = user.Id
                    Name = "Regular Collection"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None } ]

            Assert.Equal<Collections.Collection list>(expected, actual)
        }

    [<Fact>]
    member _.``updateCollectionAsync returns 0 when collection is system``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let systemCollection =
                Entities.makeCollection user "Unsorted" None createdAt None true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Collections.updateCollectionAsync
                    { Id = systemCollection.Id
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)

            let! actual = Seeder.getCollectionByIdAsync systemCollection.Id fixture.Seeder

            let expected: Collection option =
                Some
                    { Id = systemCollection.Id
                      UserId = user.Id
                      Name = "Unsorted"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = None
                      IsSystem = true }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``deleteCollectionAsync returns 0 when collection is system``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let systemCollection =
                Entities.makeCollection user "Unsorted" None createdAt None true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Collections.deleteCollectionAsync systemCollection.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)

            let! actual = Seeder.getCollectionByIdAsync systemCollection.Id fixture.Seeder

            Assert.True(actual.IsSome)
        }
