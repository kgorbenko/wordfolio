namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type ExamplesTests(fixture: WordfolioTestFixture) =
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

            let parameters: Examples.ExampleCreationParameters list =
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
                Entities.makeTranslation entry "счастливая случайность" Translations.TranslationSource.Manual 1

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: Examples.ExampleCreationParameters list =
                [ { DefinitionId = None
                    TranslationId = Some translation.Id
                    ExampleText = "Это была счастливая случайность"
                    Source = Examples.ExampleSource.Api }
                  { DefinitionId = None
                    TranslationId = Some translation.Id
                    ExampleText = "Жизнь полна счастливых случайностей"
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
                    ExampleText = "Это была счастливая случайность"
                    Source = Examples.ExampleSource.Api }
                  { Id = createdIds.[1]
                    DefinitionId = None
                    TranslationId = Some translation.Id
                    ExampleText = "Жизнь полна счастливых случайностей"
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

            let parameters: Examples.ExampleCreationParameters list =
                [ { DefinitionId = Some 999
                    TranslationId = None
                    ExampleText = "Test example"
                    Source = Examples.ExampleSource.Custom } ]

            let! ex =
                Assert.ThrowsAsync<Npgsql.PostgresException>(fun () ->
                    (Examples.createExamplesAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``createExamplesAsync fails with foreign key violation for non-existent translation``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let parameters: Examples.ExampleCreationParameters list =
                [ { DefinitionId = None
                    TranslationId = Some 999
                    ExampleText = "Test example"
                    Source = Examples.ExampleSource.Custom } ]

            let! ex =
                Assert.ThrowsAsync<Npgsql.PostgresException>(fun () ->
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

            let parameters: Examples.ExampleCreationParameters list =
                [ { DefinitionId = Some definition.Id
                    TranslationId = Some translation.Id
                    ExampleText = "Invalid example"
                    Source = Examples.ExampleSource.Custom } ]

            let! ex =
                Assert.ThrowsAsync<Npgsql.PostgresException>(fun () ->
                    (Examples.createExamplesAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.CheckConstraintViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``createExamplesAsync fails with check constraint violation when neither parent provided``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let parameters: Examples.ExampleCreationParameters list =
                [ { DefinitionId = None
                    TranslationId = None
                    ExampleText = "Invalid example"
                    Source = Examples.ExampleSource.Custom } ]

            let! ex =
                Assert.ThrowsAsync<Npgsql.PostgresException>(fun () ->
                    (Examples.createExamplesAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.CheckConstraintViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``getExamplesByDefinitionIdAsync returns examples for definition``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 603

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "ephemeral" createdAt None

            let definition1 =
                Entities.makeDefinition entry "Definition 1" Definitions.DefinitionSource.Manual 1

            let definition2 =
                Entities.makeDefinition entry "Definition 2" Definitions.DefinitionSource.Manual 2

            let example1 =
                Entities.makeExampleForDefinition definition1 "Example 1" Examples.ExampleSource.Api

            let example2 =
                Entities.makeExampleForDefinition definition1 "Example 2" Examples.ExampleSource.Custom

            let _ =
                Entities.makeExampleForDefinition definition2 "Other example" Examples.ExampleSource.Api

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Examples.getExamplesByDefinitionIdAsync definition1.Id
                |> fixture.WithConnectionAsync

            let expected: Examples.Example list =
                [ { Id = example1.Id
                    DefinitionId = Some definition1.Id
                    TranslationId = None
                    ExampleText = "Example 1"
                    Source = Examples.ExampleSource.Api }
                  { Id = example2.Id
                    DefinitionId = Some definition1.Id
                    TranslationId = None
                    ExampleText = "Example 2"
                    Source = Examples.ExampleSource.Custom } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getExamplesByDefinitionIdAsync returns empty list when definition has no examples``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 604

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            let definition =
                Entities.makeDefinition entry "Definition" Definitions.DefinitionSource.Manual 1

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Examples.getExamplesByDefinitionIdAsync definition.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``getExamplesByTranslationIdAsync returns examples for translation``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 605

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "serendipity" createdAt None

            let translation1 =
                Entities.makeTranslation entry "Translation 1" Translations.TranslationSource.Manual 1

            let translation2 =
                Entities.makeTranslation entry "Translation 2" Translations.TranslationSource.Manual 2

            let example1 =
                Entities.makeExampleForTranslation translation1 "Example 1" Examples.ExampleSource.Api

            let example2 =
                Entities.makeExampleForTranslation translation1 "Example 2" Examples.ExampleSource.Custom

            let _ =
                Entities.makeExampleForTranslation translation2 "Other example" Examples.ExampleSource.Api

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Examples.getExamplesByTranslationIdAsync translation1.Id
                |> fixture.WithConnectionAsync

            let expected: Examples.Example list =
                [ { Id = example1.Id
                    DefinitionId = None
                    TranslationId = Some translation1.Id
                    ExampleText = "Example 1"
                    Source = Examples.ExampleSource.Api }
                  { Id = example2.Id
                    DefinitionId = None
                    TranslationId = Some translation1.Id
                    ExampleText = "Example 2"
                    Source = Examples.ExampleSource.Custom } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getExamplesByTranslationIdAsync returns empty list when translation has no examples``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 606

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            let translation =
                Entities.makeTranslation entry "Translation" Translations.TranslationSource.Manual 1

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Examples.getExamplesByTranslationIdAsync translation.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``updateExamplesAsync updates multiple examples``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 607

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            let definition =
                Entities.makeDefinition entry "Definition" Definitions.DefinitionSource.Manual 1

            let example1 =
                Entities.makeExampleForDefinition definition "Original 1" Examples.ExampleSource.Api

            let example2 =
                Entities.makeExampleForDefinition definition "Original 2" Examples.ExampleSource.Api

            let example3 =
                Entities.makeExampleForDefinition definition "Original 3" Examples.ExampleSource.Api

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: Examples.ExampleUpdateParameters list =
                [ { Id = example1.Id
                    ExampleText = "Updated 1"
                    Source = Examples.ExampleSource.Custom }
                  { Id = example2.Id
                    ExampleText = "Updated 2"
                    Source = Examples.ExampleSource.Custom } ]

            let! affectedRows =
                Examples.updateExamplesAsync parameters
                |> fixture.WithConnectionAsync

            Assert.Equal(2, affectedRows)

            let! actual =
                fixture.Seeder
                |> Seeder.getAllExamplesAsync

            let expected: Example list =
                [ { Id = example1.Id
                    DefinitionId = Some definition.Id
                    TranslationId = None
                    ExampleText = "Updated 1"
                    Source = Examples.ExampleSource.Custom }
                  { Id = example2.Id
                    DefinitionId = Some definition.Id
                    TranslationId = None
                    ExampleText = "Updated 2"
                    Source = Examples.ExampleSource.Custom }
                  { Id = example3.Id
                    DefinitionId = Some definition.Id
                    TranslationId = None
                    ExampleText = "Original 3"
                    Source = Examples.ExampleSource.Api } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``updateExamplesAsync returns 0 when given empty list``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! affectedRows =
                Examples.updateExamplesAsync []
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``updateExamplesAsync returns 0 for non-existent examples``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let parameters: Examples.ExampleUpdateParameters list =
                [ { Id = 999
                    ExampleText = "Updated"
                    Source = Examples.ExampleSource.Custom } ]

            let! affectedRows =
                Examples.updateExamplesAsync parameters
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``deleteExamplesAsync deletes multiple examples``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 608

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            let definition =
                Entities.makeDefinition entry "Definition" Definitions.DefinitionSource.Manual 1

            let example1 =
                Entities.makeExampleForDefinition definition "Example 1" Examples.ExampleSource.Api

            let example2 =
                Entities.makeExampleForDefinition definition "Example 2" Examples.ExampleSource.Custom

            let example3 =
                Entities.makeExampleForDefinition definition "Example 3" Examples.ExampleSource.Api

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Examples.deleteExamplesAsync [ example1.Id; example2.Id ]
                |> fixture.WithConnectionAsync

            Assert.Equal(2, affectedRows)

            let! actual =
                fixture.Seeder
                |> Seeder.getAllExamplesAsync

            let expected: Example list =
                [ { Id = example3.Id
                    DefinitionId = Some definition.Id
                    TranslationId = None
                    ExampleText = "Example 3"
                    Source = Examples.ExampleSource.Api } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``deleteExamplesAsync returns 0 when given empty list``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! affectedRows =
                Examples.deleteExamplesAsync []
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``deleteExamplesAsync returns 0 for non-existent examples``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! affectedRows =
                Examples.deleteExamplesAsync [ 999; 1000 ]
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
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
