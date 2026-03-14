module Wordfolio.Api.Api.CollectionsHierarchy.Mappers

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
