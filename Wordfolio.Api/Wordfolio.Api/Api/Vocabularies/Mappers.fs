module Wordfolio.Api.Api.Vocabularies.Mappers

open Wordfolio.Api.Api.Vocabularies.Types
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Vocabularies
open Wordfolio.Api.Infrastructure.ResourceIdEncoder

let toVocabularyResponse (encoder: IResourceIdEncoder) (vocabulary: Vocabulary) : VocabularyResponse =
    { Id = encoder.Encode(VocabularyId.value vocabulary.Id)
      CollectionId = encoder.Encode(CollectionId.value vocabulary.CollectionId)
      Name = vocabulary.Name
      Description = vocabulary.Description
      CreatedAt = vocabulary.CreatedAt
      UpdatedAt = vocabulary.UpdatedAt }

let toVocabularyDetailResponse (encoder: IResourceIdEncoder) (detail: VocabularyDetail) : VocabularyDetailResponse =
    { Id = encoder.Encode(VocabularyId.value detail.Id)
      CollectionId = encoder.Encode(CollectionId.value detail.CollectionId)
      CollectionName = detail.CollectionName
      Name = detail.Name
      Description = detail.Description
      CreatedAt = detail.CreatedAt
      UpdatedAt = detail.UpdatedAt }
