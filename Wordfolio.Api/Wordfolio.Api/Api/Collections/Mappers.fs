module Wordfolio.Api.Api.Collections.Mappers

open Wordfolio.Api.Api.Collections
open Wordfolio.Api.Domain

let toCollectionResponse(collection: Collection) : CollectionResponse =
    { Id = CollectionId.value collection.Id
      Name = collection.Name
      Description = collection.Description
      CreatedAt = collection.CreatedAt
      UpdatedAt = collection.UpdatedAt }
