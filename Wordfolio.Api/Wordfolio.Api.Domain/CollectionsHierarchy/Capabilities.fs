namespace Wordfolio.Api.Domain.CollectionsHierarchy

open System.Threading.Tasks

open Wordfolio.Api.Domain

type IGetCollectionsWithVocabularies =
    abstract GetCollectionsWithVocabularies: UserId -> Task<CollectionSummary list>

module Capabilities =
    let getCollectionsWithVocabularies (env: #IGetCollectionsWithVocabularies) userId =
        env.GetCollectionsWithVocabularies(userId)
