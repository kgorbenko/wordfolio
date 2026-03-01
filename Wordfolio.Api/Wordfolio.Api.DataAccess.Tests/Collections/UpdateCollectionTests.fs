namespace Wordfolio.Api.DataAccess.Tests.Collections

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type UpdateCollectionTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

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
