namespace Wordfolio.Api.Domain.Vocabularies

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections

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

type IGetVocabularyById =
    abstract GetVocabularyById: VocabularyId -> Task<Vocabulary option>

type IGetVocabulariesByCollectionId =
    abstract GetVocabulariesByCollectionId: CollectionId -> Task<Vocabulary list>

type ICreateVocabulary =
    abstract CreateVocabulary: CollectionId * string * string option * DateTimeOffset -> Task<VocabularyId>

type IUpdateVocabulary =
    abstract UpdateVocabulary: VocabularyId * string * string option * DateTimeOffset -> Task<int>

type IDeleteVocabulary =
    abstract DeleteVocabulary: VocabularyId -> Task<int>

type IGetDefaultVocabulary =
    abstract member GetDefaultVocabulary: UserId -> Task<Vocabulary option>

type ICreateDefaultVocabulary =
    abstract member CreateDefaultVocabulary: CreateVocabularyParameters -> Task<VocabularyId>

type IGetDefaultCollection =
    abstract member GetDefaultCollection: UserId -> Task<Collection option>

type ICreateDefaultCollection =
    abstract member CreateDefaultCollection: CreateCollectionParameters -> Task<CollectionId>

module Capabilities =
    let getVocabularyById (env: #IGetVocabularyById) vocabularyId = env.GetVocabularyById(vocabularyId)

    let getVocabulariesByCollectionId (env: #IGetVocabulariesByCollectionId) collectionId =
        env.GetVocabulariesByCollectionId(collectionId)

    let createVocabulary (env: #ICreateVocabulary) collectionId name description createdAt =
        env.CreateVocabulary(collectionId, name, description, createdAt)

    let updateVocabulary (env: #IUpdateVocabulary) vocabularyId name description updatedAt =
        env.UpdateVocabulary(vocabularyId, name, description, updatedAt)

    let deleteVocabulary (env: #IDeleteVocabulary) vocabularyId = env.DeleteVocabulary(vocabularyId)

    let getCollectionById (env: #IGetCollectionById) collectionId = env.GetCollectionById(collectionId)

    let getDefaultVocabulary (env: #IGetDefaultVocabulary) userId = env.GetDefaultVocabulary(userId)

    let createDefaultVocabulary (env: #ICreateDefaultVocabulary) parameters = env.CreateDefaultVocabulary(parameters)

    let getDefaultCollection (env: #IGetDefaultCollection) userId = env.GetDefaultCollection(userId)

    let createDefaultCollection (env: #ICreateDefaultCollection) parameters = env.CreateDefaultCollection(parameters)
