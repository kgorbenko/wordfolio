namespace Wordfolio.Api.DataAccess.Tests.EntriesHierarchy

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.DataAccess.Definitions
open Wordfolio.Api.DataAccess.Examples
open Wordfolio.Api.DataAccess.Translations
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetEntriesByIdsWithHierarchyTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getEntriesByIdsWithHierarchyAsync returns empty list when entryIds is empty``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                EntriesHierarchy.getEntriesByIdsWithHierarchyAsync []
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``getEntriesByIdsWithHierarchyAsync returns empty list when none of the ids exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                EntriesHierarchy.getEntriesByIdsWithHierarchyAsync [ 99999; 99998 ]
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``getEntriesByIdsWithHierarchyAsync returns hydrated entries for requested IDs``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 770

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry1 =
                Entities.makeEntry vocabulary "word1" createdAt createdAt

            let entry2 =
                Entities.makeEntry vocabulary "word2" createdAt createdAt

            let definition =
                Entities.makeDefinition entry1 "a good word" DefinitionSource.Manual 0

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntriesByIdsWithHierarchyAsync [ entry1.Id; entry2.Id ]
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryHierarchy list =
                [ { Entry =
                      { Id = entry1.Id
                        VocabularyId = vocabulary.Id
                        EntryText = "word1"
                        CreatedAt = createdAt
                        UpdatedAt = createdAt }
                    Definitions =
                      [ { Definition =
                            { Id = definition.Id
                              EntryId = entry1.Id
                              DefinitionText = "a good word"
                              Source = DefinitionSource.Manual
                              DisplayOrder = 0 }
                          Examples = [] } ]
                    Translations = [] }
                  { Entry =
                      { Id = entry2.Id
                        VocabularyId = vocabulary.Id
                        EntryText = "word2"
                        CreatedAt = createdAt
                        UpdatedAt = createdAt }
                    Definitions = []
                    Translations = [] } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getEntriesByIdsWithHierarchyAsync returns only requested entries``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 771

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry1 =
                Entities.makeEntry vocabulary "word1" createdAt createdAt

            let _ =
                Entities.makeEntry vocabulary "word2" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntriesByIdsWithHierarchyAsync [ entry1.Id ]
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryHierarchy list =
                [ { Entry =
                      { Id = entry1.Id
                        VocabularyId = vocabulary.Id
                        EntryText = "word1"
                        CreatedAt = createdAt
                        UpdatedAt = createdAt }
                    Definitions = []
                    Translations = [] } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getEntriesByIdsWithHierarchyAsync returns hydrated entries with definitions, translations, and examples``
        ()
        =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 772

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry =
                Entities.makeEntry vocabulary "word" createdAt createdAt

            let definition =
                Entities.makeDefinition entry "a definition" DefinitionSource.Manual 0

            let definitionExample =
                Entities.makeExampleForDefinition definition "example for definition" ExampleSource.Custom

            let translation =
                Entities.makeTranslation entry "a translation" TranslationSource.Manual 0

            let translationExample =
                Entities.makeExampleForTranslation translation "example for translation" ExampleSource.Custom

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntriesByIdsWithHierarchyAsync [ entry.Id ]
                |> fixture.WithConnectionAsync

            let expected: EntriesHierarchy.EntryHierarchy list =
                [ { Entry =
                      { Id = entry.Id
                        VocabularyId = vocabulary.Id
                        EntryText = "word"
                        CreatedAt = createdAt
                        UpdatedAt = createdAt }
                    Definitions =
                      [ { Definition =
                            { Id = definition.Id
                              EntryId = entry.Id
                              DefinitionText = "a definition"
                              Source = DefinitionSource.Manual
                              DisplayOrder = 0 }
                          Examples =
                            [ { Id = definitionExample.Id
                                DefinitionId = Some definition.Id
                                TranslationId = None
                                ExampleText = "example for definition"
                                Source = ExampleSource.Custom } ] } ]
                    Translations =
                      [ { Translation =
                            { Id = translation.Id
                              EntryId = entry.Id
                              TranslationText = "a translation"
                              Source = TranslationSource.Manual
                              DisplayOrder = 0 }
                          Examples =
                            [ { Id = translationExample.Id
                                DefinitionId = None
                                TranslationId = Some translation.Id
                                ExampleText = "example for translation"
                                Source = ExampleSource.Custom } ] } ] } ]

            Assert.Equivalent(expected, actual)
        }
