namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type DefinitionsTests(fixture: WordfolioTestFixture) =
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

            let parameters: Definitions.DefinitionCreationParameters list =
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

            let parameters: Definitions.DefinitionCreationParameters list =
                [ { EntryId = 999
                    DefinitionText = "Test definition"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 1 } ]

            let! ex =
                Assert.ThrowsAsync<Npgsql.PostgresException>(fun () ->
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

            let parameters: Definitions.DefinitionCreationParameters list =
                [ { EntryId = entry.Id
                    DefinitionText = "Definition 1"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 1 }
                  { EntryId = entry.Id
                    DefinitionText = "Definition 2"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 1 } ]

            let! ex =
                Assert.ThrowsAsync<Npgsql.PostgresException>(fun () ->
                    (Definitions.createDefinitionsAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.UniqueViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``getDefinitionsByEntryIdAsync returns definitions ordered by DisplayOrder``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 402

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry1 =
                Entities.makeEntry vocabulary "ephemeral" createdAt None

            let entry2 =
                Entities.makeEntry vocabulary "ubiquitous" createdAt None

            let def1 =
                Entities.makeDefinition entry1 "Definition 1" Definitions.DefinitionSource.Manual 2

            let def2 =
                Entities.makeDefinition entry1 "Definition 2" Definitions.DefinitionSource.Manual 1

            let def3 =
                Entities.makeDefinition entry1 "Definition 3" Definitions.DefinitionSource.Api 3

            let _ =
                Entities.makeDefinition entry2 "Other definition" Definitions.DefinitionSource.Manual 1

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Definitions.getDefinitionsByEntryIdAsync entry1.Id
                |> fixture.WithConnectionAsync

            let expected: Definitions.Definition list =
                [ { Id = def2.Id
                    EntryId = entry1.Id
                    DefinitionText = "Definition 2"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 1 }
                  { Id = def1.Id
                    EntryId = entry1.Id
                    DefinitionText = "Definition 1"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 2 }
                  { Id = def3.Id
                    EntryId = entry1.Id
                    DefinitionText = "Definition 3"
                    Source = Definitions.DefinitionSource.Api
                    DisplayOrder = 3 } ]

            Assert.Equal<Definitions.Definition list>(expected, actual)
        }

    [<Fact>]
    member _.``getDefinitionsByEntryIdAsync returns empty list when entry has no definitions``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 403

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

            let! actual =
                Definitions.getDefinitionsByEntryIdAsync entry.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``updateDefinitionsAsync updates multiple definitions``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 404

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            let def1 =
                Entities.makeDefinition entry "Original 1" Definitions.DefinitionSource.Manual 1

            let def2 =
                Entities.makeDefinition entry "Original 2" Definitions.DefinitionSource.Manual 2

            let def3 =
                Entities.makeDefinition entry "Original 3" Definitions.DefinitionSource.Manual 3

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: Definitions.DefinitionUpdateParameters list =
                [ { Id = def1.Id
                    DefinitionText = "Updated 1"
                    Source = Definitions.DefinitionSource.Api
                    DisplayOrder = 9 }
                  { Id = def2.Id
                    DefinitionText = "Updated 2"
                    Source = Definitions.DefinitionSource.Api
                    DisplayOrder = 8 } ]

            let! affectedRows =
                Definitions.updateDefinitionsAsync parameters
                |> fixture.WithConnectionAsync

            Assert.Equal(2, affectedRows)

            let! actual =
                fixture.Seeder
                |> Seeder.getAllDefinitionsAsync

            let expected: Definition list =
                [ { Id = def1.Id
                    EntryId = entry.Id
                    DefinitionText = "Updated 1"
                    Source = Definitions.DefinitionSource.Api
                    DisplayOrder = 9 }
                  { Id = def2.Id
                    EntryId = entry.Id
                    DefinitionText = "Updated 2"
                    Source = Definitions.DefinitionSource.Api
                    DisplayOrder = 8 }
                  { Id = def3.Id
                    EntryId = entry.Id
                    DefinitionText = "Original 3"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 3 } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``updateDefinitionsAsync returns 0 when given empty list``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! affectedRows =
                Definitions.updateDefinitionsAsync []
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``updateDefinitionsAsync returns 0 for non-existent definitions``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let parameters: Definitions.DefinitionUpdateParameters list =
                [ { Id = 999
                    DefinitionText = "Updated"
                    Source = Definitions.DefinitionSource.Api
                    DisplayOrder = 1 } ]

            let! affectedRows =
                Definitions.updateDefinitionsAsync parameters
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``deleteDefinitionsAsync deletes multiple definitions``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 405

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            let def1 =
                Entities.makeDefinition entry "Definition 1" Definitions.DefinitionSource.Manual 1

            let def2 =
                Entities.makeDefinition entry "Definition 2" Definitions.DefinitionSource.Manual 2

            let def3 =
                Entities.makeDefinition entry "Definition 3" Definitions.DefinitionSource.Manual 3

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Definitions.deleteDefinitionsAsync [ def1.Id; def2.Id ]
                |> fixture.WithConnectionAsync

            Assert.Equal(2, affectedRows)

            let! actual =
                fixture.Seeder
                |> Seeder.getAllDefinitionsAsync

            let expected: Definition list =
                [ { Id = def3.Id
                    EntryId = entry.Id
                    DefinitionText = "Definition 3"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 3 } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``deleteDefinitionsAsync returns 0 when given empty list``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! affectedRows =
                Definitions.deleteDefinitionsAsync []
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``deleteDefinitionsAsync returns 0 for non-existent definitions``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! affectedRows =
                Definitions.deleteDefinitionsAsync [ 999; 1000 ]
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``deleting entry cascades to delete definitions``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 406

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            let _ =
                Entities.makeDefinition entry "Definition 1" Definitions.DefinitionSource.Manual 1

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! _ =
                Entries.deleteEntryAsync entry.Id
                |> fixture.WithConnectionAsync

            let! actualDefinitions =
                fixture.Seeder
                |> Seeder.getAllDefinitionsAsync

            Assert.Empty(actualDefinitions)
        }
