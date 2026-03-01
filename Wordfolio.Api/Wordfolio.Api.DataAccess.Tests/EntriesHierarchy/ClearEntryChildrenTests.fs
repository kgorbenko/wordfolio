namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type EntriesHierarchyClearEntryChildrenTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``clearEntryChildrenAsync removes definitions for entry``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 500

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "ephemeral" createdAt None

            let _definition1 =
                Entities.makeDefinition entry "Lasting for a short time" Definitions.DefinitionSource.Api 0

            let _definition2 =
                Entities.makeDefinition entry "Temporary" Definitions.DefinitionSource.Manual 1

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! deletedCount =
                EntriesHierarchy.clearEntryChildrenAsync entry.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(2, deletedCount)

            let! actualDefinitions =
                fixture.Seeder
                |> Seeder.getAllDefinitionsAsync

            Assert.Empty(actualDefinitions)
        }

    [<Fact>]
    member _.``clearEntryChildrenAsync removes translations for entry``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 501

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "serendipity" createdAt None

            let _translation1 =
                Entities.makeTranslation entry "счастливый случай" Translations.TranslationSource.Api 0

            let _translation2 =
                Entities.makeTranslation entry "удачное стечение" Translations.TranslationSource.Manual 1

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! deletedCount =
                EntriesHierarchy.clearEntryChildrenAsync entry.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(2, deletedCount)

            let! actualTranslations =
                fixture.Seeder
                |> Seeder.getAllTranslationsAsync

            Assert.Empty(actualTranslations)
        }

    [<Fact>]
    member _.``clearEntryChildrenAsync removes both definitions and translations``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 502

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "ubiquitous" createdAt None

            let _definition =
                Entities.makeDefinition entry "Present everywhere" Definitions.DefinitionSource.Api 0

            let _translation =
                Entities.makeTranslation entry "вездесущий" Translations.TranslationSource.Manual 0

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! deletedCount =
                EntriesHierarchy.clearEntryChildrenAsync entry.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(2, deletedCount)

            let! actualDefinitions =
                fixture.Seeder
                |> Seeder.getAllDefinitionsAsync

            let! actualTranslations =
                fixture.Seeder
                |> Seeder.getAllTranslationsAsync

            Assert.Empty(actualDefinitions)
            Assert.Empty(actualTranslations)
        }

    [<Fact>]
    member _.``clearEntryChildrenAsync cascades to examples when deleting definitions``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 503

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "meticulous" createdAt None

            let definition =
                Entities.makeDefinition entry "Careful about details" Definitions.DefinitionSource.Api 0

            let _example1 =
                Entities.makeExampleForDefinition definition "He is meticulous" Examples.ExampleSource.Api

            let _example2 =
                Entities.makeExampleForDefinition definition "She is meticulous" Examples.ExampleSource.Custom

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! deletedCount =
                EntriesHierarchy.clearEntryChildrenAsync entry.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(1, deletedCount)

            let! actualDefinitions =
                fixture.Seeder
                |> Seeder.getAllDefinitionsAsync

            let! actualExamples =
                fixture.Seeder
                |> Seeder.getAllExamplesAsync

            Assert.Empty(actualDefinitions)
            Assert.Empty(actualExamples)
        }

    [<Fact>]
    member _.``clearEntryChildrenAsync cascades to examples when deleting translations``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 504

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "tenacious" createdAt None

            let translation =
                Entities.makeTranslation entry "упорный" Translations.TranslationSource.Manual 0

            let _example1 =
                Entities.makeExampleForTranslation translation "He is tenacious" Examples.ExampleSource.Api

            let _example2 =
                Entities.makeExampleForTranslation translation "She is tenacious" Examples.ExampleSource.Custom

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! deletedCount =
                EntriesHierarchy.clearEntryChildrenAsync entry.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(1, deletedCount)

            let! actualTranslations =
                fixture.Seeder
                |> Seeder.getAllTranslationsAsync

            let! actualExamples =
                fixture.Seeder
                |> Seeder.getAllExamplesAsync

            Assert.Empty(actualTranslations)
            Assert.Empty(actualExamples)
        }

    [<Fact>]
    member _.``clearEntryChildrenAsync returns zero for existing entry with no children``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 507

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "standalone" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! deletedCount =
                EntriesHierarchy.clearEntryChildrenAsync entry.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(0, deletedCount)

            let! actualEntries =
                fixture.Seeder
                |> Seeder.getAllEntriesAsync

            let expectedEntries: Entry list =
                [ { Id = entry.Id
                    VocabularyId = vocabulary.Id
                    EntryText = "standalone"
                    CreatedAt = createdAt
                    UpdatedAt = None } ]

            Assert.Equal<Entry list>(expectedEntries, actualEntries)
        }

    [<Fact>]
    member _.``clearEntryChildrenAsync does nothing for non-existent entry``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 505

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "solitary" createdAt None

            let definition =
                Entities.makeDefinition entry "Alone" Definitions.DefinitionSource.Manual 0

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! deletedCount =
                EntriesHierarchy.clearEntryChildrenAsync 999
                |> fixture.WithConnectionAsync

            Assert.Equal(0, deletedCount)

            let! actualDefinitions =
                fixture.Seeder
                |> Seeder.getAllDefinitionsAsync

            let expectedDefinition: Definition =
                { Id = definition.Id
                  EntryId = entry.Id
                  DefinitionText = "Alone"
                  Source = Definitions.DefinitionSource.Manual
                  DisplayOrder = 0 }

            Assert.Single(actualDefinitions)
            |> ignore

            Assert.Equal(expectedDefinition, actualDefinitions.[0])
        }

    [<Fact>]
    member _.``clearEntryChildrenAsync does not affect other entries``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 506

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry1 =
                Entities.makeEntry vocabulary "resilient" createdAt None

            let _definition1 =
                Entities.makeDefinition entry1 "Able to recover" Definitions.DefinitionSource.Api 0

            let _translation1 =
                Entities.makeTranslation entry1 "устойчивый" Translations.TranslationSource.Manual 0

            let entry2 =
                Entities.makeEntry vocabulary "benevolent" createdAt None

            let definition2 =
                Entities.makeDefinition entry2 "Kind and generous" Definitions.DefinitionSource.Manual 0

            let translation2 =
                Entities.makeTranslation entry2 "доброжелательный" Translations.TranslationSource.Api 0

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! deletedCount =
                EntriesHierarchy.clearEntryChildrenAsync entry1.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(2, deletedCount)

            let! actualDefinitions =
                fixture.Seeder
                |> Seeder.getAllDefinitionsAsync

            let! actualTranslations =
                fixture.Seeder
                |> Seeder.getAllTranslationsAsync

            let expectedDefinitions: Definition list =
                [ { Id = definition2.Id
                    EntryId = entry2.Id
                    DefinitionText = "Kind and generous"
                    Source = Definitions.DefinitionSource.Manual
                    DisplayOrder = 0 } ]

            let expectedTranslations: Translation list =
                [ { Id = translation2.Id
                    EntryId = entry2.Id
                    TranslationText = "доброжелательный"
                    Source = Translations.TranslationSource.Api
                    DisplayOrder = 0 } ]

            Assert.Equal<Definition list>(expectedDefinitions, actualDefinitions)
            Assert.Equal<Translation list>(expectedTranslations, actualTranslations)
        }
