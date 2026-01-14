namespace Wordfolio.Api.Domain.Shared

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain

type CreateVocabularyParameters =
    { CollectionId: CollectionId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset }

type CreateCollectionParameters =
    { UserId: UserId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset }

type IGetDefaultVocabulary =
    abstract GetDefaultVocabulary: UserId -> Task<Vocabulary option>

type ICreateDefaultVocabulary =
    abstract CreateDefaultVocabulary: CreateVocabularyParameters -> Task<VocabularyId>

type IGetDefaultCollection =
    abstract GetDefaultCollection: UserId -> Task<Collection option>

type ICreateDefaultCollection =
    abstract CreateDefaultCollection: CreateCollectionParameters -> Task<CollectionId>

module Capabilities =
    let getDefaultVocabulary (env: #IGetDefaultVocabulary) userId = env.GetDefaultVocabulary(userId)

    let createDefaultVocabulary (env: #ICreateDefaultVocabulary) parameters = env.CreateDefaultVocabulary(parameters)

    let getDefaultCollection (env: #IGetDefaultCollection) userId = env.GetDefaultCollection(userId)

    let createDefaultCollection (env: #ICreateDefaultCollection) parameters = env.CreateDefaultCollection(parameters)
