namespace Wordfolio.Api.Domain.CollectionsHierarchy

open System.Threading.Tasks

open Wordfolio.Api.Domain

type IGetCollectionsWithVocabularies =
    abstract GetCollectionsWithVocabularies: UserId -> Task<CollectionSummary list>

type IGetDefaultVocabularySummary =
    abstract GetDefaultVocabularySummary: UserId -> Task<VocabularySummary option>

module Capabilities =
    let getCollectionsWithVocabularies (env: #IGetCollectionsWithVocabularies) userId =
        env.GetCollectionsWithVocabularies(userId)

    let getDefaultVocabularySummary (env: #IGetDefaultVocabularySummary) userId =
        env.GetDefaultVocabularySummary(userId)
