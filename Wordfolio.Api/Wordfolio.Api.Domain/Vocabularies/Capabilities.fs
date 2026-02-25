namespace Wordfolio.Api.Domain.Vocabularies

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain

type CreateVocabularyData =
    { CollectionId: CollectionId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset }

type UpdateVocabularyData =
    { VocabularyId: VocabularyId
      Name: string
      Description: string option
      UpdatedAt: DateTimeOffset }

type IGetVocabularyById =
    abstract GetVocabularyById: VocabularyId -> Task<Vocabulary option>

type IGetVocabulariesByCollectionId =
    abstract GetVocabulariesByCollectionId: CollectionId -> Task<Vocabulary list>

type ICreateVocabulary =
    abstract CreateVocabulary: CreateVocabularyData -> Task<VocabularyId>

type IUpdateVocabulary =
    abstract UpdateVocabulary: UpdateVocabularyData -> Task<int>

type IDeleteVocabulary =
    abstract DeleteVocabulary: VocabularyId -> Task<int>

module Capabilities =
    let getVocabularyById (env: #IGetVocabularyById) vocabularyId = env.GetVocabularyById(vocabularyId)

    let getVocabulariesByCollectionId (env: #IGetVocabulariesByCollectionId) collectionId =
        env.GetVocabulariesByCollectionId(collectionId)

    let createVocabulary (env: #ICreateVocabulary) data = env.CreateVocabulary(data)

    let updateVocabulary (env: #IUpdateVocabulary) data = env.UpdateVocabulary(data)

    let deleteVocabulary (env: #IDeleteVocabulary) vocabularyId = env.DeleteVocabulary(vocabularyId)
