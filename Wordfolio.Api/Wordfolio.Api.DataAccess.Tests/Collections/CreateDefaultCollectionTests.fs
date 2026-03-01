namespace Wordfolio.Api.DataAccess.Tests

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CreateDefaultCollectionTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``createDefaultCollectionAsync creates system collection``() =
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
                Collections.createDefaultCollectionAsync
                    { UserId = user.Id
                      Name = "Unsorted"
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
                      Name = "Unsorted"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = None
                      IsSystem = true }

            Assert.Equivalent(expected, actualCollection)
        }
