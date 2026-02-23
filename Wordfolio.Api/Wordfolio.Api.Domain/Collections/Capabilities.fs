namespace Wordfolio.Api.Domain.Collections

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain

type IGetCollectionsByUserId =
    abstract GetCollectionsByUserId: UserId -> Task<Collection list>

type ICreateCollection =
    abstract CreateCollection: UserId * string * string option * DateTimeOffset -> Task<CollectionId>

type IUpdateCollection =
    abstract UpdateCollection: CollectionId * string * string option * DateTimeOffset -> Task<int>

type IDeleteCollection =
    abstract DeleteCollection: CollectionId -> Task<int>

module Capabilities =
    let getCollectionsByUserId (env: #IGetCollectionsByUserId) userId = env.GetCollectionsByUserId(userId)

    let createCollection (env: #ICreateCollection) userId name description createdAt =
        env.CreateCollection(userId, name, description, createdAt)

    let updateCollection (env: #IUpdateCollection) collectionId name description updatedAt =
        env.UpdateCollection(collectionId, name, description, updatedAt)

    let deleteCollection (env: #IDeleteCollection) collectionId = env.DeleteCollection(collectionId)
