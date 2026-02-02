namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type EntriesHierarchyTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getEntryByIdWithHierarchyAsync returns None when entry does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                EntriesHierarchy.getEntryByIdWithHierarchyAsync 999
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getEntryByIdWithHierarchyAsync returns entry with no definitions or translations``() =
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
                Entities.makeEntry vocabulary "solitary" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntryByIdWithHierarchyAsync entry.Id
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryWithHierarchy option =
                Some
                    { Entry =
                        { Id = entry.Id
                          VocabularyId = vocabulary.Id
                          EntryText = "solitary"
                          CreatedAt = createdAt
                          UpdatedAt = None }
                      Definitions = []
                      Translations = [] }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getEntryByIdWithHierarchyAsync returns entry with single definition and no examples``() =
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
                Entities.makeEntry vocabulary "ephemeral" createdAt None

            let definition =
                Entities.makeDefinition entry "Lasting for a very short time" Definitions.DefinitionSource.Manual 0

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntryByIdWithHierarchyAsync entry.Id
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryWithHierarchy option =
                Some
                    { Entry =
                        { Id = entry.Id
                          VocabularyId = vocabulary.Id
                          EntryText = "ephemeral"
                          CreatedAt = createdAt
                          UpdatedAt = None }
                      Definitions =
                        [ { Definition =
                              { Id = definition.Id
                                EntryId = entry.Id
                                DefinitionText = "Lasting for a very short time"
                                Source = Definitions.DefinitionSource.Manual
                                DisplayOrder = 0 }
                            Examples = [] } ]
                      Translations = [] }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getEntryByIdWithHierarchyAsync returns entry with single translation and no examples``() =
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
                Entities.makeEntry vocabulary "serendipity" createdAt None

            let translation =
                Entities.makeTranslation entry "счастливая случайность" Translations.TranslationSource.Api 0

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntryByIdWithHierarchyAsync entry.Id
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryWithHierarchy option =
                Some
                    { Entry =
                        { Id = entry.Id
                          VocabularyId = vocabulary.Id
                          EntryText = "serendipity"
                          CreatedAt = createdAt
                          UpdatedAt = None }
                      Definitions = []
                      Translations =
                        [ { Translation =
                              { Id = translation.Id
                                EntryId = entry.Id
                                TranslationText = "счастливая случайность"
                                Source = Translations.TranslationSource.Api
                                DisplayOrder = 0 }
                            Examples = [] } ] }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getEntryByIdWithHierarchyAsync returns entry with definition and examples``() =
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
                Entities.makeEntry vocabulary "ubiquitous" createdAt None

            let definition =
                Entities.makeDefinition entry "Present everywhere" Definitions.DefinitionSource.Api 0

            let example1 =
                Entities.makeExampleForDefinition definition "The mobile phone is ubiquitous" Examples.ExampleSource.Api

            let example2 =
                Entities.makeExampleForDefinition
                    definition
                    "Coffee shops are ubiquitous in the city"
                    Examples.ExampleSource.Custom

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntryByIdWithHierarchyAsync entry.Id
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryWithHierarchy option =
                Some
                    { Entry =
                        { Id = entry.Id
                          VocabularyId = vocabulary.Id
                          EntryText = "ubiquitous"
                          CreatedAt = createdAt
                          UpdatedAt = None }
                      Definitions =
                        [ { Definition =
                              { Id = definition.Id
                                EntryId = entry.Id
                                DefinitionText = "Present everywhere"
                                Source = Definitions.DefinitionSource.Api
                                DisplayOrder = 0 }
                            Examples =
                              [ { Id = example1.Id
                                  DefinitionId = Some definition.Id
                                  TranslationId = None
                                  ExampleText = "The mobile phone is ubiquitous"
                                  Source = Examples.ExampleSource.Api }
                                { Id = example2.Id
                                  DefinitionId = Some definition.Id
                                  TranslationId = None
                                  ExampleText = "Coffee shops are ubiquitous in the city"
                                  Source = Examples.ExampleSource.Custom } ] } ]
                      Translations = [] }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getEntryByIdWithHierarchyAsync returns entry with translation and examples``() =
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
                Entities.makeEntry vocabulary "meticulous" createdAt None

            let translation =
                Entities.makeTranslation entry "тщательный" Translations.TranslationSource.Manual 0

            let example1 =
                Entities.makeExampleForTranslation
                    translation
                    "He is meticulous about details"
                    Examples.ExampleSource.Api

            let example2 =
                Entities.makeExampleForTranslation translation "Her work is meticulous" Examples.ExampleSource.Custom

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntryByIdWithHierarchyAsync entry.Id
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryWithHierarchy option =
                Some
                    { Entry =
                        { Id = entry.Id
                          VocabularyId = vocabulary.Id
                          EntryText = "meticulous"
                          CreatedAt = createdAt
                          UpdatedAt = None }
                      Definitions = []
                      Translations =
                        [ { Translation =
                              { Id = translation.Id
                                EntryId = entry.Id
                                TranslationText = "тщательный"
                                Source = Translations.TranslationSource.Manual
                                DisplayOrder = 0 }
                            Examples =
                              [ { Id = example1.Id
                                  DefinitionId = None
                                  TranslationId = Some translation.Id
                                  ExampleText = "He is meticulous about details"
                                  Source = Examples.ExampleSource.Api }
                                { Id = example2.Id
                                  DefinitionId = None
                                  TranslationId = Some translation.Id
                                  ExampleText = "Her work is meticulous"
                                  Source = Examples.ExampleSource.Custom } ] } ] }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getEntryByIdWithHierarchyAsync returns entry with multiple definitions and translations``() =
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
                Entities.makeEntry vocabulary "resilient" createdAt None

            let definition1 =
                Entities.makeDefinition entry "Able to recover quickly" Definitions.DefinitionSource.Api 0

            let definition2 =
                Entities.makeDefinition entry "Able to withstand shock" Definitions.DefinitionSource.Manual 1

            let translation1 =
                Entities.makeTranslation entry "устойчивый" Translations.TranslationSource.Api 0

            let translation2 =
                Entities.makeTranslation entry "жизнеспособный" Translations.TranslationSource.Manual 1

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntryByIdWithHierarchyAsync entry.Id
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryWithHierarchy option =
                Some
                    { Entry =
                        { Id = entry.Id
                          VocabularyId = vocabulary.Id
                          EntryText = "resilient"
                          CreatedAt = createdAt
                          UpdatedAt = None }
                      Definitions =
                        [ { Definition =
                              { Id = definition1.Id
                                EntryId = entry.Id
                                DefinitionText = "Able to recover quickly"
                                Source = Definitions.DefinitionSource.Api
                                DisplayOrder = 0 }
                            Examples = [] }
                          { Definition =
                              { Id = definition2.Id
                                EntryId = entry.Id
                                DefinitionText = "Able to withstand shock"
                                Source = Definitions.DefinitionSource.Manual
                                DisplayOrder = 1 }
                            Examples = [] } ]
                      Translations =
                        [ { Translation =
                              { Id = translation1.Id
                                EntryId = entry.Id
                                TranslationText = "устойчивый"
                                Source = Translations.TranslationSource.Api
                                DisplayOrder = 0 }
                            Examples = [] }
                          { Translation =
                              { Id = translation2.Id
                                EntryId = entry.Id
                                TranslationText = "жизнеспособный"
                                Source = Translations.TranslationSource.Manual
                                DisplayOrder = 1 }
                            Examples = [] } ] }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getEntryByIdWithHierarchyAsync returns entry with complete hierarchy``() =
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
                Entities.makeEntry vocabulary "tenacious" createdAt None

            let definition =
                Entities.makeDefinition entry "Holding fast" Definitions.DefinitionSource.Api 0

            let defExample =
                Entities.makeExampleForDefinition definition "He is tenacious in his pursuit" Examples.ExampleSource.Api

            let translation =
                Entities.makeTranslation entry "упорный" Translations.TranslationSource.Manual 0

            let transExample =
                Entities.makeExampleForTranslation translation "She is very tenacious" Examples.ExampleSource.Custom

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntryByIdWithHierarchyAsync entry.Id
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryWithHierarchy option =
                Some
                    { Entry =
                        { Id = entry.Id
                          VocabularyId = vocabulary.Id
                          EntryText = "tenacious"
                          CreatedAt = createdAt
                          UpdatedAt = None }
                      Definitions =
                        [ { Definition =
                              { Id = definition.Id
                                EntryId = entry.Id
                                DefinitionText = "Holding fast"
                                Source = Definitions.DefinitionSource.Api
                                DisplayOrder = 0 }
                            Examples =
                              [ { Id = defExample.Id
                                  DefinitionId = Some definition.Id
                                  TranslationId = None
                                  ExampleText = "He is tenacious in his pursuit"
                                  Source = Examples.ExampleSource.Api } ] } ]
                      Translations =
                        [ { Translation =
                              { Id = translation.Id
                                EntryId = entry.Id
                                TranslationText = "упорный"
                                Source = Translations.TranslationSource.Manual
                                DisplayOrder = 0 }
                            Examples =
                              [ { Id = transExample.Id
                                  DefinitionId = None
                                  TranslationId = Some translation.Id
                                  ExampleText = "She is very tenacious"
                                  Source = Examples.ExampleSource.Custom } ] } ] }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getEntryByIdWithHierarchyAsync returns entry with updated timestamp``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 407

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "benevolent" createdAt (Some updatedAt)

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntryByIdWithHierarchyAsync entry.Id
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryWithHierarchy option =
                Some
                    { Entry =
                        { Id = entry.Id
                          VocabularyId = vocabulary.Id
                          EntryText = "benevolent"
                          CreatedAt = createdAt
                          UpdatedAt = Some updatedAt }
                      Definitions = []
                      Translations = [] }

            Assert.Equal(expected, actual)
        }

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

            let definition1 =
                Entities.makeDefinition entry "Lasting for a short time" Definitions.DefinitionSource.Api 0

            let definition2 =
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

            let translation1 =
                Entities.makeTranslation entry "счастливый случай" Translations.TranslationSource.Api 0

            let translation2 =
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

            let definition =
                Entities.makeDefinition entry "Present everywhere" Definitions.DefinitionSource.Api 0

            let translation =
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

            let example1 =
                Entities.makeExampleForDefinition definition "He is meticulous" Examples.ExampleSource.Api

            let example2 =
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

            let example1 =
                Entities.makeExampleForTranslation translation "He is tenacious" Examples.ExampleSource.Api

            let example2 =
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

            let definition1 =
                Entities.makeDefinition entry1 "Able to recover" Definitions.DefinitionSource.Api 0

            let translation1 =
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

    [<Fact>]
    member _.``getEntriesHierarchyByVocabularyIdAsync returns empty list when no entries``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Col" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocab" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntriesHierarchyByVocabularyIdAsync vocabulary.Id
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryWithHierarchy list =
                []

            Assert.Equal<EntriesHierarchy.EntryWithHierarchy list>(expected, actual)
        }

    [<Fact>]
    member _.``getEntriesHierarchyByVocabularyIdAsync returns entries with hierarchy``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Col" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocab" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "serendipity" createdAt None

            let definition =
                Entities.makeDefinition entry "happy accident" Definitions.DefinitionSource.Api 1

            let translation =
                Entities.makeTranslation entry "счастливая случайность" Translations.TranslationSource.Manual 1

            let defExample =
                Entities.makeExampleForDefinition definition "Found by serendipity" Examples.ExampleSource.Custom

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addDefinitions [ definition ]
                |> Seeder.addTranslations [ translation ]
                |> Seeder.addExamples [ defExample ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntriesHierarchyByVocabularyIdAsync vocabulary.Id
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryWithHierarchy list =
                [ { Entry =
                      { Id = entry.Id
                        VocabularyId = vocabulary.Id
                        EntryText = "serendipity"
                        CreatedAt = createdAt
                        UpdatedAt = None }
                    Definitions =
                      [ { Definition =
                            { Id = definition.Id
                              EntryId = entry.Id
                              DefinitionText = "happy accident"
                              Source = Definitions.DefinitionSource.Api
                              DisplayOrder = 1 }
                          Examples =
                            [ { Id = defExample.Id
                                DefinitionId = Some definition.Id
                                TranslationId = None
                                ExampleText = "Found by serendipity"
                                Source = Examples.ExampleSource.Custom } ] } ]
                    Translations =
                      [ { Translation =
                            { Id = translation.Id
                              EntryId = entry.Id
                              TranslationText = "счастливая случайность"
                              Source = Translations.TranslationSource.Manual
                              DisplayOrder = 1 }
                          Examples = [] } ] } ]

            Assert.Equal<EntriesHierarchy.EntryWithHierarchy list>(expected, actual)
        }

    [<Fact>]
    member _.``getEntriesHierarchyByVocabularyIdAsync returns only entries for specified vocabulary``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Col" None createdAt None false

            let vocab1 =
                Entities.makeVocabulary collection "Vocab1" None createdAt None false

            let vocab2 =
                Entities.makeVocabulary collection "Vocab2" None createdAt None false

            let entry1 =
                Entities.makeEntry vocab1 "word1" createdAt None

            let entry2 =
                Entities.makeEntry vocab2 "word2" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocab1; vocab2 ]
                |> Seeder.addEntries [ entry1; entry2 ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntriesHierarchyByVocabularyIdAsync vocab1.Id
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryWithHierarchy list =
                [ { Entry =
                      { Id = entry1.Id
                        VocabularyId = vocab1.Id
                        EntryText = "word1"
                        CreatedAt = createdAt
                        UpdatedAt = None }
                    Definitions = []
                    Translations = [] } ]

            Assert.Equal<EntriesHierarchy.EntryWithHierarchy list>(expected, actual)
        }

    [<Fact>]
    member _.``getEntriesHierarchyByVocabularyIdAsync returns empty list for non-existent vocabulary``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                EntriesHierarchy.getEntriesHierarchyByVocabularyIdAsync 999
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryWithHierarchy list =
                []

            Assert.Equal<EntriesHierarchy.EntryWithHierarchy list>(expected, actual)
        }
