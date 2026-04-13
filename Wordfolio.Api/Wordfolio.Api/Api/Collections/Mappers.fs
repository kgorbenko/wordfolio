module Wordfolio.Api.Api.Collections.Mappers

open Wordfolio.Api.Api.Collections.Types
open Wordfolio.Api.Domain
open Wordfolio.Api.Infrastructure.ResourceIdEncoder

let toCollectionResponse (encoder: IResourceIdEncoder) (collection: Collection) : CollectionResponse =
    { Id = encoder.Encode(CollectionId.value collection.Id)
      Name = collection.Name
      Description = collection.Description
      CreatedAt = collection.CreatedAt
      UpdatedAt = collection.UpdatedAt }
