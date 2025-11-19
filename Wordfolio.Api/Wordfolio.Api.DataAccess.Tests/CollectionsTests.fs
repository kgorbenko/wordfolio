namespace Wordfolio.Api.DataAccess.Tests

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CollectionsTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``CRUD operations work``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.FromHours(0.0))

            do!
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 100 } ]
                |> Seeder.saveChangesAsync

            // Create
            let collectionId = 1

            do!
                Collections.createCollectionAsync
                    { Id = collectionId
                      UserId = 100
                      Name = "My Collection"
                      Description = Some "Test collection"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            // Read by ID
            let! collection =
                Collections.getCollectionByIdAsync collectionId
                |> fixture.WithConnectionAsync

            Assert.True(collection.IsSome)
            Assert.Equal("My Collection", collection.Value.Name)
            Assert.Equal(Some "Test collection", collection.Value.Description)

            // Update
            let! affectedRows =
                Collections.updateCollectionAsync
                    { Id = collectionId
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            // Read updated
            let! updatedCollection =
                Collections.getCollectionByIdAsync collectionId
                |> fixture.WithConnectionAsync

            Assert.Equal("Updated Name", updatedCollection.Value.Name)
            Assert.Equal(Some "Updated Description", updatedCollection.Value.Description)

            // Delete
            let! deleteAffectedRows =
                Collections.deleteCollectionAsync collectionId
                |> fixture.WithConnectionAsync

            Assert.Equal(1, deleteAffectedRows)

            // Verify deleted
            let! deletedCollection =
                Collections.getCollectionByIdAsync collectionId
                |> fixture.WithConnectionAsync

            Assert.True(deletedCollection.IsNone)
        }
