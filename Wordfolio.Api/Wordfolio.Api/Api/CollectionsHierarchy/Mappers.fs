module Wordfolio.Api.Api.CollectionsHierarchy.Mappers

open System

open Wordfolio.Api.Api.CollectionsHierarchy.Types
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.CollectionsHierarchy

let toVocabularyWithEntryCountResponse(vocabulary: VocabularyWithEntryCount) : VocabularyWithEntryCountResponse =
    { Id = VocabularyId.value vocabulary.Id
      Name = vocabulary.Name
      Description = vocabulary.Description
      CreatedAt = vocabulary.CreatedAt
      UpdatedAt = vocabulary.UpdatedAt
      EntryCount = vocabulary.EntryCount }

let toCollectionWithVocabulariesResponse(collection: CollectionWithVocabularies) : CollectionWithVocabulariesResponse =
    { Id = CollectionId.value collection.Id
      Name = collection.Name
      Description = collection.Description
      CreatedAt = collection.CreatedAt
      UpdatedAt = collection.UpdatedAt
      Vocabularies =
        collection.Vocabularies
        |> List.map toVocabularyWithEntryCountResponse }

let toCollectionWithVocabularyCountResponse
    (collection: CollectionWithVocabularyCount)
    : CollectionWithVocabularyCountResponse =
    { Id = CollectionId.value collection.Id
      Name = collection.Name
      Description = collection.Description
      CreatedAt = collection.CreatedAt
      UpdatedAt = collection.UpdatedAt
      VocabularyCount = collection.VocabularyCount }

let toVocabularySortByDomain(sortBy: VocabularySortByRequest) : VocabularySortBy =
    match sortBy with
    | VocabularySortByRequest.Name -> VocabularySortBy.Name
    | VocabularySortByRequest.CreatedAt -> VocabularySortBy.CreatedAt
    | VocabularySortByRequest.UpdatedAt -> VocabularySortBy.UpdatedAt
    | VocabularySortByRequest.EntryCount -> VocabularySortBy.EntryCount
    | x -> failwith $"Unknown {nameof VocabularySortByRequest} value: {x}"

let toSortDirectionDomain(sortDirection: SortDirectionRequest) : SortDirection =
    match sortDirection with
    | SortDirectionRequest.Asc -> SortDirection.Asc
    | SortDirectionRequest.Desc -> SortDirection.Desc
    | x -> failwith $"Unknown {nameof SortDirectionRequest} value: {x}"

let toCollectionVocabulariesQuery
    (search: string)
    (sortBy: VocabularySortByRequest)
    (sortDirection: SortDirectionRequest)
    : SearchCollectionVocabulariesQuery =
    { Search =
        if String.IsNullOrWhiteSpace search then
            None
        else
            Some search
      SortBy = sortBy |> toVocabularySortByDomain
      SortDirection = sortDirection |> toSortDirectionDomain }
