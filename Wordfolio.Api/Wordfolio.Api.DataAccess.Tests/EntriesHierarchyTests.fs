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
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None

            let entry =
                Entities.makeEntry vocabulary "solitary" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntryByIdWithHierarchyAsync entry.Id
                |> fixture.WithConnectionAsync

            match actual with
            | None -> Assert.Fail("Expected entry but got None")
            | Some hierarchy ->
                Assert.Equal(entry.Id, hierarchy.Entry.Id)
                Assert.Equal(vocabulary.Id, hierarchy.Entry.VocabularyId)
                Assert.Equal("solitary", hierarchy.Entry.EntryText)
                Assert.Equal(createdAt, hierarchy.Entry.CreatedAt)
                Assert.Equal(None, hierarchy.Entry.UpdatedAt)
                Assert.Empty(hierarchy.Definitions)
                Assert.Empty(hierarchy.Translations)
        }

    [<Fact>]
    member _.``getEntryByIdWithHierarchyAsync returns entry with single definition and no examples``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 401

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None

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

            match actual with
            | None -> Assert.Fail("Expected entry but got None")
            | Some hierarchy ->
                Assert.Equal(entry.Id, hierarchy.Entry.Id)
                Assert.Equal("ephemeral", hierarchy.Entry.EntryText)
                Assert.Equal(1, hierarchy.Definitions.Length)
                Assert.Equal(definition.Id, hierarchy.Definitions.[0].Definition.Id)
                Assert.Equal("Lasting for a very short time", hierarchy.Definitions.[0].Definition.DefinitionText)
                Assert.Equal(Definitions.DefinitionSource.Manual, hierarchy.Definitions.[0].Definition.Source)
                Assert.Equal(0, hierarchy.Definitions.[0].Definition.DisplayOrder)
                Assert.Empty(hierarchy.Definitions.[0].Examples)
                Assert.Empty(hierarchy.Translations)
        }

    [<Fact>]
    member _.``getEntryByIdWithHierarchyAsync returns entry with single translation and no examples``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 402

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None

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

            match actual with
            | None -> Assert.Fail("Expected entry but got None")
            | Some hierarchy ->
                Assert.Equal(entry.Id, hierarchy.Entry.Id)
                Assert.Equal("serendipity", hierarchy.Entry.EntryText)
                Assert.Empty(hierarchy.Definitions)
                Assert.Equal(1, hierarchy.Translations.Length)
                Assert.Equal(translation.Id, hierarchy.Translations.[0].Translation.Id)
                Assert.Equal("счастливая случайность", hierarchy.Translations.[0].Translation.TranslationText)
                Assert.Equal(Translations.TranslationSource.Api, hierarchy.Translations.[0].Translation.Source)
                Assert.Equal(0, hierarchy.Translations.[0].Translation.DisplayOrder)
                Assert.Empty(hierarchy.Translations.[0].Examples)
        }

    [<Fact>]
    member _.``getEntryByIdWithHierarchyAsync returns entry with definition and examples``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 403

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None

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

            match actual with
            | None -> Assert.Fail("Expected entry but got None")
            | Some hierarchy ->
                Assert.Equal(entry.Id, hierarchy.Entry.Id)
                Assert.Equal("ubiquitous", hierarchy.Entry.EntryText)
                Assert.Equal(1, hierarchy.Definitions.Length)
                Assert.Equal(definition.Id, hierarchy.Definitions.[0].Definition.Id)
                Assert.Equal(2, hierarchy.Definitions.[0].Examples.Length)

                let examples =
                    hierarchy.Definitions.[0].Examples
                    |> List.sortBy(fun e -> e.Id)

                Assert.Equal(example1.Id, examples.[0].Id)
                Assert.Equal("The mobile phone is ubiquitous", examples.[0].ExampleText)
                Assert.Equal(Examples.ExampleSource.Api, examples.[0].Source)
                Assert.Equal(Some definition.Id, examples.[0].DefinitionId)
                Assert.Equal(None, examples.[0].TranslationId)

                Assert.Equal(example2.Id, examples.[1].Id)
                Assert.Equal("Coffee shops are ubiquitous in the city", examples.[1].ExampleText)
                Assert.Equal(Examples.ExampleSource.Custom, examples.[1].Source)
        }

    [<Fact>]
    member _.``getEntryByIdWithHierarchyAsync returns entry with translation and examples``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 404

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None

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

            match actual with
            | None -> Assert.Fail("Expected entry but got None")
            | Some hierarchy ->
                Assert.Equal(entry.Id, hierarchy.Entry.Id)
                Assert.Equal("meticulous", hierarchy.Entry.EntryText)
                Assert.Empty(hierarchy.Definitions)
                Assert.Equal(1, hierarchy.Translations.Length)
                Assert.Equal(translation.Id, hierarchy.Translations.[0].Translation.Id)
                Assert.Equal(2, hierarchy.Translations.[0].Examples.Length)

                let examples =
                    hierarchy.Translations.[0].Examples
                    |> List.sortBy(fun e -> e.Id)

                Assert.Equal(example1.Id, examples.[0].Id)
                Assert.Equal("He is meticulous about details", examples.[0].ExampleText)
                Assert.Equal(Examples.ExampleSource.Api, examples.[0].Source)
                Assert.Equal(None, examples.[0].DefinitionId)
                Assert.Equal(Some translation.Id, examples.[0].TranslationId)

                Assert.Equal(example2.Id, examples.[1].Id)
                Assert.Equal("Her work is meticulous", examples.[1].ExampleText)
                Assert.Equal(Examples.ExampleSource.Custom, examples.[1].Source)
        }

    [<Fact>]
    member _.``getEntryByIdWithHierarchyAsync returns entry with multiple definitions and translations``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 405

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None

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

            match actual with
            | None -> Assert.Fail("Expected entry but got None")
            | Some hierarchy ->
                Assert.Equal(entry.Id, hierarchy.Entry.Id)
                Assert.Equal("resilient", hierarchy.Entry.EntryText)
                Assert.Equal(2, hierarchy.Definitions.Length)
                Assert.Equal(2, hierarchy.Translations.Length)

                // Check definitions are ordered by DisplayOrder
                Assert.Equal(definition1.Id, hierarchy.Definitions.[0].Definition.Id)
                Assert.Equal("Able to recover quickly", hierarchy.Definitions.[0].Definition.DefinitionText)
                Assert.Equal(0, hierarchy.Definitions.[0].Definition.DisplayOrder)

                Assert.Equal(definition2.Id, hierarchy.Definitions.[1].Definition.Id)
                Assert.Equal("Able to withstand shock", hierarchy.Definitions.[1].Definition.DefinitionText)
                Assert.Equal(1, hierarchy.Definitions.[1].Definition.DisplayOrder)

                // Check translations are ordered by DisplayOrder
                Assert.Equal(translation1.Id, hierarchy.Translations.[0].Translation.Id)
                Assert.Equal("устойчивый", hierarchy.Translations.[0].Translation.TranslationText)
                Assert.Equal(0, hierarchy.Translations.[0].Translation.DisplayOrder)

                Assert.Equal(translation2.Id, hierarchy.Translations.[1].Translation.Id)
                Assert.Equal("жизнеспособный", hierarchy.Translations.[1].Translation.TranslationText)
                Assert.Equal(1, hierarchy.Translations.[1].Translation.DisplayOrder)
        }

    [<Fact>]
    member _.``getEntryByIdWithHierarchyAsync returns entry with complete hierarchy``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 406

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None

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

            match actual with
            | None -> Assert.Fail("Expected entry but got None")
            | Some hierarchy ->
                Assert.Equal(entry.Id, hierarchy.Entry.Id)
                Assert.Equal("tenacious", hierarchy.Entry.EntryText)

                // Check definition with example
                Assert.Equal(1, hierarchy.Definitions.Length)
                Assert.Equal(definition.Id, hierarchy.Definitions.[0].Definition.Id)
                Assert.Equal("Holding fast", hierarchy.Definitions.[0].Definition.DefinitionText)
                Assert.Equal(1, hierarchy.Definitions.[0].Examples.Length)
                Assert.Equal(defExample.Id, hierarchy.Definitions.[0].Examples.[0].Id)
                Assert.Equal("He is tenacious in his pursuit", hierarchy.Definitions.[0].Examples.[0].ExampleText)

                // Check translation with example
                Assert.Equal(1, hierarchy.Translations.Length)
                Assert.Equal(translation.Id, hierarchy.Translations.[0].Translation.Id)
                Assert.Equal("упорный", hierarchy.Translations.[0].Translation.TranslationText)
                Assert.Equal(1, hierarchy.Translations.[0].Examples.Length)
                Assert.Equal(transExample.Id, hierarchy.Translations.[0].Examples.[0].Id)
                Assert.Equal("She is very tenacious", hierarchy.Translations.[0].Examples.[0].ExampleText)
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
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None

            let entry =
                Entities.makeEntry vocabulary "benevolent" createdAt (Some updatedAt)

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntryByIdWithHierarchyAsync entry.Id
                |> fixture.WithConnectionAsync

            match actual with
            | None -> Assert.Fail("Expected entry but got None")
            | Some hierarchy ->
                Assert.Equal(entry.Id, hierarchy.Entry.Id)
                Assert.Equal("benevolent", hierarchy.Entry.EntryText)
                Assert.Equal(createdAt, hierarchy.Entry.CreatedAt)
                Assert.Equal(Some updatedAt, hierarchy.Entry.UpdatedAt)
        }
