module Wordfolio.Api.Api.Vocabularies.Mappers

open Wordfolio.Api.Api.Vocabularies
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Vocabularies

let toVocabularyResponse(vocabulary: Vocabulary) : VocabularyResponse =
    { Id = VocabularyId.value vocabulary.Id
      CollectionId = CollectionId.value vocabulary.CollectionId
      Name = vocabulary.Name
      Description = vocabulary.Description
      CreatedAt = vocabulary.CreatedAt
      UpdatedAt = vocabulary.UpdatedAt }

let toVocabularyDetailResponse(detail: VocabularyDetail) : VocabularyDetailResponse =
    { Id = VocabularyId.value detail.Id
      CollectionId = CollectionId.value detail.CollectionId
      CollectionName = detail.CollectionName
      Name = detail.Name
      Description = detail.Description
      CreatedAt = detail.CreatedAt
      UpdatedAt = detail.UpdatedAt }
