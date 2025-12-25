module Wordfolio.Api.Migrations.Constants

[<Literal>]
let WordfolioSchema = "wordfolio"

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
