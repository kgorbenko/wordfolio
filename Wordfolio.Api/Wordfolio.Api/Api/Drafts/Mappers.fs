module Wordfolio.Api.Api.Drafts.Mappers

open Wordfolio.Api.Api.Drafts
open Wordfolio.Api.Api.Mappers
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries

let private toVocabularyResponse(vocabulary: Vocabulary) : VocabularyResponse =
    { Id = VocabularyId.value vocabulary.Id
      Name = vocabulary.Name
      Description = vocabulary.Description
      CreatedAt = vocabulary.CreatedAt
      UpdatedAt = vocabulary.UpdatedAt }

let toDraftsResponse(drafts: DraftsVocabularyData) : DraftsResponse =
    { Vocabulary = toVocabularyResponse drafts.Vocabulary
      Entries =
        drafts.Entries
        |> List.map toEntryResponse }
