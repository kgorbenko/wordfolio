module Wordfolio.Api.Urls

[<Literal>]
let Root = "/"

[<Literal>]
let ById = "/{id:int}"

module Collections =
    [<Literal>]
    let Path = "/collections"

    let collectionById(id: int) = $"{Path}/{id}"

module Vocabularies =
    [<Literal>]
    let Path =
        "/{collectionId:int}/vocabularies"

    let vocabulariesByCollection(collectionId: int) =
        $"/collections/{collectionId}/vocabularies"

    let vocabularyById(collectionId: int, vocabularyId: int) =
        $"{vocabulariesByCollection collectionId}/{vocabularyId}"

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
