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
    let CreatedAtDateTimeColumn =
        "CreatedAtDateTime"

    [<Literal>]
    let CreatedAtOffsetColumn =
        "CreatedAtOffset"

    [<Literal>]
    let UpdatedAtDateTimeColumn =
        "UpdatedAtDateTime"

    [<Literal>]
    let UpdatedAtOffsetColumn =
        "UpdatedAtOffset"

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
    let CreatedAtDateTimeColumn =
        "CreatedAtDateTime"

    [<Literal>]
    let CreatedAtOffsetColumn =
        "CreatedAtOffset"

    [<Literal>]
    let UpdatedAtDateTimeColumn =
        "UpdatedAtDateTime"

    [<Literal>]
    let UpdatedAtOffsetColumn =
        "UpdatedAtOffset"
