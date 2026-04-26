module Wordfolio.Api.Urls

[<Literal>]
let Root = "/"

[<Literal>]
let ById = "/{id}"

module Collections =
    [<Literal>]
    let Path = "/collections"

    let collectionById(id: string) = $"{Path}/{id}"

module CollectionsHierarchy =
    [<Literal>]
    let Path = "/collections-hierarchy"

    [<Literal>]
    let CollectionsPath = "/collections"

    [<Literal>]
    let VocabulariesByCollectionPath =
        "/collections/{collectionId}/vocabularies"

    let collections() = $"{Path}{CollectionsPath}"

    let vocabulariesByCollection(collectionId: string) =
        $"{collections()}/{collectionId}/vocabularies"

module Vocabularies =
    [<Literal>]
    let Path = "/{collectionId}/vocabularies"

    let vocabulariesByCollection(collectionId: string) =
        $"/collections/{collectionId}/vocabularies"

    let vocabularyById(collectionId: string, vocabularyId: string) =
        $"{vocabulariesByCollection collectionId}/{vocabularyId}"

    let moveVocabularyById(collectionId: string, vocabularyId: string) =
        $"{vocabularyById(collectionId, vocabularyId)}/move"

module Entries =
    [<Literal>]
    let Path = "/{vocabularyId}/entries"

    let entriesByVocabulary(collectionId: string, vocabularyId: string) =
        $"/collections/{collectionId}/vocabularies/{vocabularyId}/entries"

    let entryById(collectionId: string, vocabularyId: string, id: string) =
        $"{entriesByVocabulary(collectionId, vocabularyId)}/{id}"

    let moveEntryById(collectionId: string, vocabularyId: string, id: string) =
        $"{entryById(collectionId, vocabularyId, id)}/move"

module Drafts =
    [<Literal>]
    let Path = "/drafts"

    [<Literal>]
    let All = "/all"

    [<Literal>]
    let Move = "/move"

    let allDrafts() = $"{Path}{All}"
    let draftById(id: string) = $"{Path}/{id}"
    let moveDraftById(id: string) = $"{draftById id}{Move}"

module Auth =
    [<Literal>]
    let Path = "/auth"

    [<Literal>]
    let PasswordRequirements =
        "/password-requirements"

    [<Literal>]
    let ManageInfo = "/manage/info"

    let passwordRequirements() = $"{Path}{PasswordRequirements}"
    let manageInfo() = $"{Path}{ManageInfo}"

module Dictionary =
    [<Literal>]
    let Path = "/dictionary"

    [<Literal>]
    let Lookup = "/lookup"

module Exercises =
    [<Literal>]
    let Path = "/exercises/sessions"

    [<Literal>]
    let SessionById = "/{sessionId}"

    [<Literal>]
    let EntryAttempts =
        "/{sessionId}/entries/{entryId}/attempts"

    let sessionById(sessionId: string) = $"{Path}/{sessionId}"
