module Wordfolio.Api.Api.Drafts.Types

open System

open Wordfolio.Api.Api.Types

type CreateDraftRequest =
    { EntryText: string
      Definitions: DefinitionRequest list
      Translations: TranslationRequest list
      AllowDuplicate: bool option }

type VocabularyResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type DraftsResponse =
    { Vocabulary: VocabularyResponse
      Entries: EntryResponse list }
