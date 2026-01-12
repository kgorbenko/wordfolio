module Wordfolio.Api.DataAccess.Schema

[<Literal>]
let Name = "wordfolio"

module UsersTable =
    [<Literal>]
    let Name = "Users"

module CollectionsTable =
    [<Literal>]
    let Name = "Collections"

    [<Literal>]
    let IdColumn = "Id"

    [<Literal>]
    let UserIdColumn = "UserId"

    [<Literal>]
    let NameColumn = "Name"

    [<Literal>]
    let DescriptionColumn = "Description"

    [<Literal>]
    let CreatedAtColumn = "CreatedAt"

    [<Literal>]
    let UpdatedAtColumn = "UpdatedAt"

    [<Literal>]
    let IsSystemColumn = "IsSystem"

module VocabulariesTable =
    [<Literal>]
    let Name = "Vocabularies"

    [<Literal>]
    let IdColumn = "Id"

    [<Literal>]
    let CollectionIdColumn = "CollectionId"

    [<Literal>]
    let NameColumn = "Name"

    [<Literal>]
    let DescriptionColumn = "Description"

    [<Literal>]
    let CreatedAtColumn = "CreatedAt"

    [<Literal>]
    let UpdatedAtColumn = "UpdatedAt"

    [<Literal>]
    let IsDefaultColumn = "IsDefault"

module EntriesTable =
    [<Literal>]
    let Name = "Entries"

    [<Literal>]
    let IdColumn = "Id"

    [<Literal>]
    let VocabularyIdColumn = "VocabularyId"

    [<Literal>]
    let EntryTextColumn = "EntryText"

    [<Literal>]
    let CreatedAtColumn = "CreatedAt"

    [<Literal>]
    let UpdatedAtColumn = "UpdatedAt"

module DefinitionsTable =
    [<Literal>]
    let Name = "Definitions"

    [<Literal>]
    let IdColumn = "Id"

    [<Literal>]
    let EntryIdColumn = "EntryId"

    [<Literal>]
    let DefinitionTextColumn = "DefinitionText"

    [<Literal>]
    let SourceColumn = "Source"

    [<Literal>]
    let DisplayOrderColumn = "DisplayOrder"

module TranslationsTable =
    [<Literal>]
    let Name = "Translations"

    [<Literal>]
    let IdColumn = "Id"

    [<Literal>]
    let EntryIdColumn = "EntryId"

    [<Literal>]
    let TranslationTextColumn =
        "TranslationText"

    [<Literal>]
    let SourceColumn = "Source"

    [<Literal>]
    let DisplayOrderColumn = "DisplayOrder"

module ExamplesTable =
    [<Literal>]
    let Name = "Examples"

    [<Literal>]
    let IdColumn = "Id"

    [<Literal>]
    let DefinitionIdColumn = "DefinitionId"

    [<Literal>]
    let TranslationIdColumn = "TranslationId"

    [<Literal>]
    let ExampleTextColumn = "ExampleText"

    [<Literal>]
    let SourceColumn = "Source"
