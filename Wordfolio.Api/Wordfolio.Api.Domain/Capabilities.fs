namespace Wordfolio.Api.Domain

open System
open System.Threading.Tasks

type ITransactional<'env> =
    abstract RunInTransaction<'a, 'err> : ('env -> Task<Result<'a, 'err>>) -> Task<Result<'a, 'err>>

type CreateDefaultVocabularyParameters =
    { CollectionId: CollectionId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset }

type CreateDefaultCollectionParameters =
    { UserId: UserId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset }

type IGetCollectionById =
    abstract GetCollectionById: CollectionId -> Task<Collection option>

type IGetDefaultVocabulary =
    abstract GetDefaultVocabulary: UserId -> Task<Vocabulary option>

type ICreateDefaultVocabulary =
    abstract CreateDefaultVocabulary: CreateDefaultVocabularyParameters -> Task<VocabularyId>

type IGetDefaultCollection =
    abstract GetDefaultCollection: UserId -> Task<Collection option>

type ICreateDefaultCollection =
    abstract CreateDefaultCollection: CreateDefaultCollectionParameters -> Task<CollectionId>

module Capabilities =
    let runInTransaction (env: #ITransactional<'env>) operation = env.RunInTransaction(operation)

    let getCollectionById (env: #IGetCollectionById) collectionId = env.GetCollectionById(collectionId)

    let getDefaultVocabulary (env: #IGetDefaultVocabulary) userId = env.GetDefaultVocabulary(userId)

    let createDefaultVocabulary (env: #ICreateDefaultVocabulary) parameters = env.CreateDefaultVocabulary(parameters)

    let getDefaultCollection (env: #IGetDefaultCollection) userId = env.GetDefaultCollection(userId)

    let createDefaultCollection (env: #ICreateDefaultCollection) parameters = env.CreateDefaultCollection(parameters)
