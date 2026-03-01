namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading.Tasks

open Npgsql
open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CreateDefinitionsTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``createDefinitionsAsync inserts multiple definitions``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 400

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "serendipity" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: Definitions.CreateDefinitionParameters list =
                [ { EntryId = entry.Id
                    DefinitionText = "The occurrence of events by chance"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 1 }
                  { EntryId = entry.Id
                    DefinitionText = "Finding something good without looking"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 2 }
                  { EntryId = entry.Id
                    DefinitionText = "Happy accident"
                    Source = Definitions.DefinitionSource.Api
                    DisplayOrder = 3 } ]

            let! createdIds =
                Definitions.createDefinitionsAsync parameters
                |> fixture.WithConnectionAsync

            Assert.Equal(3, createdIds.Length)

            let! actualDefinitions =
                fixture.Seeder
                |> Seeder.getAllDefinitionsAsync

            let expected: Definition list =
                [ { Id = createdIds.[0]
                    EntryId = entry.Id
                    DefinitionText = "The occurrence of events by chance"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 1 }
                  { Id = createdIds.[1]
                    EntryId = entry.Id
                    DefinitionText = "Finding something good without looking"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 2 }
                  { Id = createdIds.[2]
                    EntryId = entry.Id
                    DefinitionText = "Happy accident"
                    Source = Definitions.DefinitionSource.Api
                    DisplayOrder = 3 } ]

            Assert.Equivalent(expected, actualDefinitions)
        }

    [<Fact>]
    member _.``createDefinitionsAsync returns empty list when given empty list``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! createdIds =
                Definitions.createDefinitionsAsync []
                |> fixture.WithConnectionAsync

            Assert.Empty(createdIds)
        }

    [<Fact>]
    member _.``createDefinitionsAsync fails with foreign key violation for non-existent entry``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let parameters: Definitions.CreateDefinitionParameters list =
                [ { EntryId = 999
                    DefinitionText = "Test definition"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 1 } ]

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (Definitions.createDefinitionsAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``createDefinitionsAsync fails with unique constraint violation for duplicate DisplayOrder``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 401

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: Definitions.CreateDefinitionParameters list =
                [ { EntryId = entry.Id
                    DefinitionText = "Definition 1"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 1 }
                  { EntryId = entry.Id
                    DefinitionText = "Definition 2"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 1 } ]

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (Definitions.createDefinitionsAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.UniqueViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``createDefinitionsAsync returns ids in the same order as input parameters``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 402

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "ordered" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: Definitions.CreateDefinitionParameters list =
                [ { EntryId = entry.Id
                    DefinitionText = "Third"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 3 }
                  { EntryId = entry.Id
                    DefinitionText = "First"
                    Source = Definitions.DefinitionSource.Api
                    DisplayOrder = 1 }
                  { EntryId = entry.Id
                    DefinitionText = "Second"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 2 } ]

            let! createdIds =
                Definitions.createDefinitionsAsync parameters
                |> fixture.WithConnectionAsync

            let! actualDefinitions =
                fixture.Seeder
                |> Seeder.getAllDefinitionsAsync

            let definitionsById =
                actualDefinitions
                |> List.map(fun definition -> definition.Id, definition)
                |> Map.ofList

            let createdDefinitions =
                createdIds
                |> List.map(fun id -> definitionsById.[id])

            let actualDisplayOrders =
                createdDefinitions
                |> List.map(fun definition -> definition.DisplayOrder)

            let actualDefinitionTexts =
                createdDefinitions
                |> List.map(fun definition -> definition.DefinitionText)

            Assert.Equal<int list>([ 3; 1; 2 ], actualDisplayOrders)
            Assert.Equal<string list>([ "Third"; "First"; "Second" ], actualDefinitionTexts)
        }
