namespace Wordfolio.Api.Domain.Drafts

open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Shared

type DraftsData =
    { Vocabulary: Vocabulary
      Entries: Entry list }
