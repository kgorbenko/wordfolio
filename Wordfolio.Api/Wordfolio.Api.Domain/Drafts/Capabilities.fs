namespace Wordfolio.Api.Domain.Drafts

open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries

type IGetEntriesHierarchy =
    abstract member GetEntriesHierarchy: VocabularyId -> Task<Entry list>

module Capabilities =
    let getEntriesHierarchy (env: #IGetEntriesHierarchy) vocabularyId = env.GetEntriesHierarchy(vocabularyId)
