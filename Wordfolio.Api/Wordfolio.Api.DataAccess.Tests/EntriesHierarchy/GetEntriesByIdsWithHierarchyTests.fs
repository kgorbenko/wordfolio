namespace Wordfolio.Api.DataAccess.Tests.EntriesHierarchy

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.DataAccess.Definitions
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

            Assert.Equal(2, actual.Length)

            let entry1Result =
                actual
                |> List.find(fun e -> e.Entry.Id = entry1.Id)

            Assert.Equal("word1", entry1Result.Entry.EntryText)
            Assert.Equal(1, entry1Result.Definitions.Length)
            Assert.Equal("a good word", entry1Result.Definitions[0].Definition.DefinitionText)

            let entry2Result =
                actual
                |> List.find(fun e -> e.Entry.Id = entry2.Id)

            Assert.Equal("word2", entry2Result.Entry.EntryText)
            Assert.Empty(entry2Result.Definitions)
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

            let entry2 =
                Entities.makeEntry vocabulary "word2" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                EntriesHierarchy.getEntriesByIdsWithHierarchyAsync [ entry1.Id ]
                |> fixture.WithConnectionAsync

            Assert.Equal(1, actual.Length)
            Assert.Equal(entry1.Id, actual[0].Entry.Id)
            Assert.DoesNotContain(actual, (fun e -> e.Entry.Id = entry2.Id))
        }
