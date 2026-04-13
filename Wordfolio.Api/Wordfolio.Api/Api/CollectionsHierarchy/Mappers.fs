module Wordfolio.Api.Api.CollectionsHierarchy.Mappers

open Wordfolio.Api.Api.CollectionsHierarchy.Types
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.CollectionsHierarchy
open Wordfolio.Api.Infrastructure.ResourceIdEncoder

let toVocabularyWithEntryCountResponse
    (encoder: IResourceIdEncoder)
    (vocabulary: VocabularyWithEntryCount)
    : VocabularyWithEntryCountResponse =
    { Id = encoder.Encode(VocabularyId.value vocabulary.Id)
      Name = vocabulary.Name
      Description = vocabulary.Description
      CreatedAt = vocabulary.CreatedAt
      UpdatedAt = vocabulary.UpdatedAt
      EntryCount = vocabulary.EntryCount }

let toCollectionWithVocabulariesResponse
    (encoder: IResourceIdEncoder)
    (collection: CollectionWithVocabularies)
    : CollectionWithVocabulariesResponse =
    { Id = encoder.Encode(CollectionId.value collection.Id)
      Name = collection.Name
      Description = collection.Description
      CreatedAt = collection.CreatedAt
      UpdatedAt = collection.UpdatedAt
      Vocabularies =
        collection.Vocabularies
        |> List.map(toVocabularyWithEntryCountResponse encoder) }

let toCollectionWithVocabularyCountResponse
    (encoder: IResourceIdEncoder)
    (collection: CollectionWithVocabularyCount)
    : CollectionWithVocabularyCountResponse =
    { Id = encoder.Encode(CollectionId.value collection.Id)
      Name = collection.Name
      Description = collection.Description
      CreatedAt = collection.CreatedAt
      UpdatedAt = collection.UpdatedAt
      VocabularyCount = collection.VocabularyCount }
