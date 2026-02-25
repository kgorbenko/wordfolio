namespace Wordfolio.Api.Domain.Collections

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain

type CreateCollectionData =
    { UserId: UserId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset }

type UpdateCollectionData =
    { CollectionId: CollectionId
      Name: string
      Description: string option
      UpdatedAt: DateTimeOffset }

type IGetCollectionsByUserId =
    abstract GetCollectionsByUserId: UserId -> Task<Collection list>

type ICreateCollection =
    abstract CreateCollection: CreateCollectionData -> Task<CollectionId>

type IUpdateCollection =
    abstract UpdateCollection: UpdateCollectionData -> Task<int>

type IDeleteCollection =
    abstract DeleteCollection: CollectionId -> Task<int>

module Capabilities =
    let getCollectionsByUserId (env: #IGetCollectionsByUserId) userId = env.GetCollectionsByUserId(userId)

    let createCollection (env: #ICreateCollection) parameters = env.CreateCollection(parameters)

    let updateCollection (env: #IUpdateCollection) parameters = env.UpdateCollection(parameters)

    let deleteCollection (env: #IDeleteCollection) collectionId = env.DeleteCollection(collectionId)
