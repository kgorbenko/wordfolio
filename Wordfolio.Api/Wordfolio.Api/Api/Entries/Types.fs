module Wordfolio.Api.Api.Entries.Types

open Wordfolio.Api.Api.Types

type CreateEntryRequest =
    { EntryText: string
      Definitions: DefinitionRequest list
      Translations: TranslationRequest list
      AllowDuplicate: bool option }
