namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type EntriesHierarchyGetEntryByIdWithHierarchyTests(fixture: WordfolioTestFixture) =
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

            let expected: EntriesHierarchy.EntryHierarchy option =
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

            let expected: EntriesHierarchy.EntryHierarchy option =
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

            let expected: EntriesHierarchy.EntryHierarchy option =
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

            let expected: EntriesHierarchy.EntryHierarchy option =
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

            let expected: EntriesHierarchy.EntryHierarchy option =
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

            let expected: EntriesHierarchy.EntryHierarchy option =
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

            let expected: EntriesHierarchy.EntryHierarchy option =
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

            let expected: EntriesHierarchy.EntryHierarchy option =
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
