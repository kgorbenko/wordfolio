namespace Wordfolio.Api.Domain.Collections

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain

type IGetCollectionById =
    abstract GetCollectionById: CollectionId -> Task<Collection option>

type IGetCollectionsByUserId =
    abstract GetCollectionsByUserId: UserId -> Task<Collection list>

type ICreateCollection =
    abstract CreateCollection: UserId * string * string option * DateTimeOffset -> Task<Collection>

type IUpdateCollection =
    abstract UpdateCollection: CollectionId * string * string option * DateTimeOffset -> Task<bool>

type IDeleteCollection =
    abstract DeleteCollection: CollectionId -> Task<bool>

module Capabilities =
    let getCollectionById(env: #IGetCollectionById) collectionId =
        env.GetCollectionById(collectionId)

    let getCollectionsByUserId(env: #IGetCollectionsByUserId) userId =
        env.GetCollectionsByUserId(userId)

    let createCollection(env: #ICreateCollection) userId name description createdAt =
        env.CreateCollection(userId, name, description, createdAt)

    let updateCollection(env: #IUpdateCollection) collectionId name description updatedAt =
        env.UpdateCollection(collectionId, name, description, updatedAt)

    let deleteCollection(env: #IDeleteCollection) collectionId =
        env.DeleteCollection(collectionId)
