module Wordfolio.Api.Urls

[<Literal>]
let Root = "/"

[<Literal>]
let ById = "/{id:int}"

module Collections =
    [<Literal>]
    let Path = "/collections"

    let collectionById(id: int) = $"{Path}/{id}"

module CollectionsHierarchy =
    [<Literal>]
    let Path = "/collections-hierarchy"

    [<Literal>]
    let CollectionsPath = "/collections"

    [<Literal>]
    let VocabulariesByCollectionPath =
        "/collections/{collectionId:int}/vocabularies"

    let collections() = $"{Path}{CollectionsPath}"

    let vocabulariesByCollection(collectionId: int) =
        $"{collections()}/{collectionId}/vocabularies"

module Vocabularies =
    [<Literal>]
    let Path =
        "/{collectionId:int}/vocabularies"

    let vocabulariesByCollection(collectionId: int) =
        $"/collections/{collectionId}/vocabularies"

    let vocabularyById(collectionId: int, vocabularyId: int) =
        $"{vocabulariesByCollection collectionId}/{vocabularyId}"

module Entries =
    [<Literal>]
    let Path = "/entries"

    let entryById(id: int) = $"{Path}/{id}"

    let moveEntryById(id: int) = $"{entryById id}/move"

    let entriesByVocabulary(vocabularyId: int) = $"/vocabularies/{vocabularyId}/entries"

module Drafts =
    [<Literal>]
    let Path = "/drafts"

module Auth =
    [<Literal>]
    let Path = "/auth"

    [<Literal>]
    let PasswordRequirements =
        "/password-requirements"

    let passwordRequirements() = $"{Path}{PasswordRequirements}"

module Dictionary =
    [<Literal>]
    let Path = "/dictionary"

    [<Literal>]
    let Lookup = "/lookup"
