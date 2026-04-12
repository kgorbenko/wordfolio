module Wordfolio.Api.Api.Drafts.Mappers

open Wordfolio.Api.Api.Drafts.Types
open Wordfolio.Api.Api.Mappers
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Infrastructure.ResourceIdEncoder

let private toVocabularyResponse (encoder: IResourceIdEncoder) (vocabulary: Vocabulary) : VocabularyResponse =
    { Id = encoder.Encode(VocabularyId.value vocabulary.Id)
      Name = vocabulary.Name
      Description = vocabulary.Description
      CreatedAt = vocabulary.CreatedAt
      UpdatedAt = vocabulary.UpdatedAt }

let toDraftsVocabularyDataResponse
    (encoder: IResourceIdEncoder)
    (drafts: DraftsVocabularyData)
    : DraftsVocabularyDataResponse =
    { Vocabulary = toVocabularyResponse encoder drafts.Vocabulary
      Entries =
        drafts.Entries
        |> List.map(toEntryResponse encoder) }
