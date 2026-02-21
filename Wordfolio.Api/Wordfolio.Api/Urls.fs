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
    let Path = "/{vocabularyId:int}/entries"

    let entriesByVocabulary(collectionId: int, vocabularyId: int) =
        $"/collections/{collectionId}/vocabularies/{vocabularyId}/entries"

    let entryById(collectionId: int, vocabularyId: int, id: int) =
        $"{entriesByVocabulary(collectionId, vocabularyId)}/{id}"

    let moveEntryById(collectionId: int, vocabularyId: int, id: int) =
        $"{entryById(collectionId, vocabularyId, id)}/move"

module Drafts =
    [<Literal>]
    let Path = "/drafts"

    [<Literal>]
    let All = "/all"

    [<Literal>]
    let Move = "/move"

    let allDrafts() = $"{Path}{All}"
    let draftById(id: int) = $"{Path}/{id}"
    let moveDraftById(id: int) = $"{draftById id}{Move}"

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
