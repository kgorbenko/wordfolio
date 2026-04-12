module Wordfolio.Api.Urls

[<Literal>]
let Root = "/"

[<Literal>]
let ById = "/{id}"

module Collections =
    [<Literal>]
    let Path = "/collections"

    let collectionById(encodedId: string) = $"{Path}/{encodedId}"

module CollectionsHierarchy =
    [<Literal>]
    let Path = "/collections-hierarchy"

    [<Literal>]
    let CollectionsPath = "/collections"

    [<Literal>]
    let VocabulariesByCollectionPath =
        "/collections/{collectionId}/vocabularies"

    let collections() = $"{Path}{CollectionsPath}"

    let vocabulariesByCollection(encodedCollectionId: string) =
        $"{collections()}/{encodedCollectionId}/vocabularies"

module Vocabularies =
    [<Literal>]
    let Path = "/{collectionId}/vocabularies"

    let vocabulariesByCollection(encodedCollectionId: string) =
        $"/collections/{encodedCollectionId}/vocabularies"

    let vocabularyById(encodedCollectionId: string, encodedVocabularyId: string) =
        $"{vocabulariesByCollection encodedCollectionId}/{encodedVocabularyId}"

    let moveVocabularyById(encodedCollectionId: string, encodedVocabularyId: string) =
        $"{vocabularyById(encodedCollectionId, encodedVocabularyId)}/move"

module Entries =
    [<Literal>]
    let Path = "/{vocabularyId}/entries"

    let entriesByVocabulary(encodedCollectionId: string, encodedVocabularyId: string) =
        $"/collections/{encodedCollectionId}/vocabularies/{encodedVocabularyId}/entries"

    let entryById(encodedCollectionId: string, encodedVocabularyId: string, encodedId: string) =
        $"{entriesByVocabulary(encodedCollectionId, encodedVocabularyId)}/{encodedId}"

    let moveEntryById(encodedCollectionId: string, encodedVocabularyId: string, encodedId: string) =
        $"{entryById(encodedCollectionId, encodedVocabularyId, encodedId)}/move"

module Drafts =
    [<Literal>]
    let Path = "/drafts"

    [<Literal>]
    let All = "/all"

    [<Literal>]
    let Move = "/move"

    let allDrafts() = $"{Path}{All}"
    let draftById(encodedId: string) = $"{Path}/{encodedId}"
    let moveDraftById(encodedId: string) = $"{draftById encodedId}{Move}"

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
