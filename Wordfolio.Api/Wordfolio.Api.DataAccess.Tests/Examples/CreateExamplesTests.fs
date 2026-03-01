namespace Wordfolio.Api.DataAccess.Tests.Examples

open System
open System.Threading.Tasks

open Npgsql
open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.DataAccess.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CreateExamplesTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``createExamplesAsync inserts multiple examples for definition``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 600

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "ephemeral" createdAt None

            let definition =
                Entities.makeDefinition entry "Lasting for a very short time" Definitions.DefinitionSource.Manual 1

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: Examples.CreateExampleParameters list =
                [ { DefinitionId = Some definition.Id
                    TranslationId = None
                    ExampleText = "Fame is ephemeral"
                    Source = Examples.ExampleSource.Api }
                  { DefinitionId = Some definition.Id
                    TranslationId = None
                    ExampleText = "The ephemeral nature of trends"
                    Source = Examples.ExampleSource.Custom } ]

            let! createdIds =
                Examples.createExamplesAsync parameters
                |> fixture.WithConnectionAsync

            Assert.Equal(2, createdIds.Length)

            let! actualExamples =
                fixture.Seeder
                |> Seeder.getAllExamplesAsync

            let expected: Example list =
                [ { Id = createdIds.[0]
                    DefinitionId = Some definition.Id
                    TranslationId = None
                    ExampleText = "Fame is ephemeral"
                    Source = Examples.ExampleSource.Api }
                  { Id = createdIds.[1]
                    DefinitionId = Some definition.Id
                    TranslationId = None
                    ExampleText = "The ephemeral nature of trends"
                    Source = Examples.ExampleSource.Custom } ]

            Assert.Equivalent(expected, actualExamples)
        }

    [<Fact>]
    member _.``createExamplesAsync inserts multiple examples for translation``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 601

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "serendipity" createdAt None

            let translation =
                Entities.makeTranslation entry "fortunate chance" Translations.TranslationSource.Manual 1

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: Examples.CreateExampleParameters list =
                [ { DefinitionId = None
                    TranslationId = Some translation.Id
                    ExampleText = "It was a fortunate chance"
                    Source = Examples.ExampleSource.Api }
                  { DefinitionId = None
                    TranslationId = Some translation.Id
                    ExampleText = "Life is full of fortunate chances"
                    Source = Examples.ExampleSource.Custom } ]

            let! createdIds =
                Examples.createExamplesAsync parameters
                |> fixture.WithConnectionAsync

            Assert.Equal(2, createdIds.Length)

            let! actualExamples =
                fixture.Seeder
                |> Seeder.getAllExamplesAsync

            let expected: Example list =
                [ { Id = createdIds.[0]
                    DefinitionId = None
                    TranslationId = Some translation.Id
                    ExampleText = "It was a fortunate chance"
                    Source = Examples.ExampleSource.Api }
                  { Id = createdIds.[1]
                    DefinitionId = None
                    TranslationId = Some translation.Id
                    ExampleText = "Life is full of fortunate chances"
                    Source = Examples.ExampleSource.Custom } ]

            Assert.Equivalent(expected, actualExamples)
        }

    [<Fact>]
    member _.``createExamplesAsync returns empty list when given empty list``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! createdIds =
                Examples.createExamplesAsync []
                |> fixture.WithConnectionAsync

            Assert.Empty(createdIds)
        }

    [<Fact>]
    member _.``createExamplesAsync fails with foreign key violation for non-existent definition``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let parameters: Examples.CreateExampleParameters list =
                [ { DefinitionId = Some 999
                    TranslationId = None
                    ExampleText = "Test example"
                    Source = Examples.ExampleSource.Custom } ]

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (Examples.createExamplesAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``createExamplesAsync fails with foreign key violation for non-existent translation``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let parameters: Examples.CreateExampleParameters list =
                [ { DefinitionId = None
                    TranslationId = Some 999
                    ExampleText = "Test example"
                    Source = Examples.ExampleSource.Custom } ]

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (Examples.createExamplesAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``createExamplesAsync fails with check constraint violation when both parents provided``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 602

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            let definition =
                Entities.makeDefinition entry "Definition" Definitions.DefinitionSource.Manual 1

            let translation =
                Entities.makeTranslation entry "Translation" Translations.TranslationSource.Manual 1

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: Examples.CreateExampleParameters list =
                [ { DefinitionId = Some definition.Id
                    TranslationId = Some translation.Id
                    ExampleText = "Invalid example"
                    Source = Examples.ExampleSource.Custom } ]

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (Examples.createExamplesAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.CheckConstraintViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``createExamplesAsync fails with check constraint violation when neither parent provided``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let parameters: Examples.CreateExampleParameters list =
                [ { DefinitionId = None
                    TranslationId = None
                    ExampleText = "Invalid example"
                    Source = Examples.ExampleSource.Custom } ]

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (Examples.createExamplesAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.CheckConstraintViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``deleting entry cascades through definitions to delete examples``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 609

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            let definition =
                Entities.makeDefinition entry "Definition" Definitions.DefinitionSource.Manual 1

            let _ =
                Entities.makeExampleForDefinition definition "Example 1" Examples.ExampleSource.Api

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! _ =
                Entries.deleteEntryAsync entry.Id
                |> fixture.WithConnectionAsync

            let! actualExamples =
                fixture.Seeder
                |> Seeder.getAllExamplesAsync

            Assert.Empty(actualExamples)
        }

    [<Fact>]
    member _.``deleting entry cascades through translations to delete examples``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 610

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            let translation =
                Entities.makeTranslation entry "Translation" Translations.TranslationSource.Manual 1

            let _ =
                Entities.makeExampleForTranslation translation "Example 1" Examples.ExampleSource.Api

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! _ =
                Entries.deleteEntryAsync entry.Id
                |> fixture.WithConnectionAsync

            let! actualExamples =
                fixture.Seeder
                |> Seeder.getAllExamplesAsync

            Assert.Empty(actualExamples)
        }
