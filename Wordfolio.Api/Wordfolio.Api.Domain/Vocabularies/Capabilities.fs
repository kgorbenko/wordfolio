namespace Wordfolio.Api.Domain.Vocabularies

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain

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
