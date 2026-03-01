namespace Wordfolio.Api.DataAccess.Tests.EntriesHierarchy

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type EntriesHierarchyGetEntriesHierarchyByVocabularyIdTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

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

            let expected: EntriesHierarchy.EntryHierarchy list =
                []

            Assert.Equal<EntriesHierarchy.EntryHierarchy list>(expected, actual)
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

            let expected: EntriesHierarchy.EntryHierarchy list =
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

            Assert.Equal<EntriesHierarchy.EntryHierarchy list>(expected, actual)
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

            let expected: EntriesHierarchy.EntryHierarchy list =
                [ { Entry =
                      { Id = entry1.Id
                        VocabularyId = vocab1.Id
                        EntryText = "word1"
                        CreatedAt = createdAt
                        UpdatedAt = None }
                    Definitions = []
                    Translations = [] } ]

            Assert.Equal<EntriesHierarchy.EntryHierarchy list>(expected, actual)
        }

    [<Fact>]
    member _.``getEntriesHierarchyByVocabularyIdAsync returns partial hierarchies without cross-associating children``
        ()
        =
        task {
            do! fixture.ResetDatabaseAsync()

            let earlierCreatedAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let laterCreatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 101

            let collection =
                Entities.makeCollection user "Col" None earlierCreatedAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocab" None earlierCreatedAt None false

            let olderEntry =
                Entities.makeEntry vocabulary "older" earlierCreatedAt None

            let newerEntry =
                Entities.makeEntry vocabulary "newer" laterCreatedAt None

            let olderDefinition =
                Entities.makeDefinition olderEntry "older meaning" Definitions.DefinitionSource.Manual 0

            let olderDefinitionExample =
                Entities.makeExampleForDefinition olderDefinition "older example" Examples.ExampleSource.Api

            let newerTranslation =
                Entities.makeTranslation newerEntry "новее" Translations.TranslationSource.Api 0

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ olderEntry; newerEntry ]
                |> Seeder.addDefinitions [ olderDefinition ]
                |> Seeder.addTranslations [ newerTranslation ]
                |> Seeder.addExamples [ olderDefinitionExample ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntriesHierarchyByVocabularyIdAsync vocabulary.Id
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryHierarchy list =
                [ { Entry =
                      { Id = newerEntry.Id
                        VocabularyId = vocabulary.Id
                        EntryText = "newer"
                        CreatedAt = laterCreatedAt
                        UpdatedAt = None }
                    Definitions = []
                    Translations =
                      [ { Translation =
                            { Id = newerTranslation.Id
                              EntryId = newerEntry.Id
                              TranslationText = "новее"
                              Source = Translations.TranslationSource.Api
                              DisplayOrder = 0 }
                          Examples = [] } ] }
                  { Entry =
                      { Id = olderEntry.Id
                        VocabularyId = vocabulary.Id
                        EntryText = "older"
                        CreatedAt = earlierCreatedAt
                        UpdatedAt = None }
                    Definitions =
                      [ { Definition =
                            { Id = olderDefinition.Id
                              EntryId = olderEntry.Id
                              DefinitionText = "older meaning"
                              Source = Definitions.DefinitionSource.Manual
                              DisplayOrder = 0 }
                          Examples =
                            [ { Id = olderDefinitionExample.Id
                                DefinitionId = Some olderDefinition.Id
                                TranslationId = None
                                ExampleText = "older example"
                                Source = Examples.ExampleSource.Api } ] } ]
                    Translations = [] } ]

            Assert.Equal<EntriesHierarchy.EntryHierarchy list>(expected, actual)
        }

    [<Fact>]
    member _.``getEntriesHierarchyByVocabularyIdAsync returns empty list for non-existent vocabulary``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                EntriesHierarchy.getEntriesHierarchyByVocabularyIdAsync 999
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryHierarchy list =
                []

            Assert.Equal<EntriesHierarchy.EntryHierarchy list>(expected, actual)
        }
