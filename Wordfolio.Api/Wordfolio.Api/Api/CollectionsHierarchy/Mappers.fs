module Wordfolio.Api.Api.CollectionsHierarchy.Mappers

open System

open Wordfolio.Api.Api.CollectionsHierarchy
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.CollectionsHierarchy

let toVocabularyWithEntryCountHierarchyResponse
    (vocabulary: VocabularyWithEntryCount)
    : VocabularyWithEntryCountHierarchyResponse =
    { Id = VocabularyId.value vocabulary.Id
      Name = vocabulary.Name
      Description = vocabulary.Description
      CreatedAt = vocabulary.CreatedAt
      UpdatedAt = vocabulary.UpdatedAt
      EntryCount = vocabulary.EntryCount }

let toCollectionWithVocabulariesHierarchyResponse
    (collection: CollectionWithVocabularies)
    : CollectionWithVocabulariesHierarchyResponse =
    { Id = CollectionId.value collection.Id
      Name = collection.Name
      Description = collection.Description
      CreatedAt = collection.CreatedAt
      UpdatedAt = collection.UpdatedAt
      Vocabularies =
        collection.Vocabularies
        |> List.map toVocabularyWithEntryCountHierarchyResponse }

let toCollectionOverviewResponse(collection: CollectionWithVocabularyCount) : CollectionOverviewResponse =
    { Id = CollectionId.value collection.Id
      Name = collection.Name
      Description = collection.Description
      CreatedAt = collection.CreatedAt
      UpdatedAt = collection.UpdatedAt
      VocabularyCount = collection.VocabularyCount }

let toVocabularySortByDomain(sortBy: VocabularySummarySortByRequest) : VocabularySortBy =
    match sortBy with
    | VocabularySummarySortByRequest.Name -> VocabularySortBy.Name
    | VocabularySummarySortByRequest.CreatedAt -> VocabularySortBy.CreatedAt
    | VocabularySummarySortByRequest.UpdatedAt -> VocabularySortBy.UpdatedAt
    | VocabularySummarySortByRequest.EntryCount -> VocabularySortBy.EntryCount
    | x -> failwith $"Unknown {nameof VocabularySummarySortByRequest} value: {x}"

let toCollectionSortByDomain(sortBy: CollectionSortByRequest) : CollectionSortBy =
    match sortBy with
    | CollectionSortByRequest.Name -> CollectionSortBy.Name
    | CollectionSortByRequest.CreatedAt -> CollectionSortBy.CreatedAt
    | CollectionSortByRequest.UpdatedAt -> CollectionSortBy.UpdatedAt
    | CollectionSortByRequest.VocabularyCount -> CollectionSortBy.VocabularyCount
    | x -> failwith $"Unknown {nameof CollectionSortByRequest} value: {x}"

let toSortDirectionDomain(sortDirection: SortDirectionRequest) : SortDirection =
    match sortDirection with
    | SortDirectionRequest.Asc -> SortDirection.Asc
    | SortDirectionRequest.Desc -> SortDirection.Desc
    | x -> failwith $"Unknown {nameof SortDirectionRequest} value: {x}"

let toSearchQuery
    (search: string)
    (sortBy: CollectionSortByRequest)
    (sortDirection: SortDirectionRequest)
    : SearchUserCollectionsQuery =
    { Search =
        if String.IsNullOrWhiteSpace search then
            None
        else
            Some search
      SortBy = sortBy |> toCollectionSortByDomain
      SortDirection = sortDirection |> toSortDirectionDomain }

let toCollectionVocabulariesQuery
    (search: string)
    (sortBy: VocabularySummarySortByRequest)
    (sortDirection: SortDirectionRequest)
    : SearchCollectionVocabulariesQuery =
    { Search =
        if String.IsNullOrWhiteSpace search then
            None
        else
            Some search
      SortBy = sortBy |> toVocabularySortByDomain
      SortDirection = sortDirection |> toSortDirectionDomain }
