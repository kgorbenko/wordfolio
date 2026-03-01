namespace Wordfolio.Api.DataAccess.Tests

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetCollectionsByUserIdTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

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
