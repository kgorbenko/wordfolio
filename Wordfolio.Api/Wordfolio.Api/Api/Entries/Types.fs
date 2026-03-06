namespace Wordfolio.Api.Api.Entries

open Wordfolio.Api.Api

type CreateEntryRequest =
    { EntryText: string
      Definitions: DefinitionRequest list
      Translations: TranslationRequest list
      AllowDuplicate: bool option }
