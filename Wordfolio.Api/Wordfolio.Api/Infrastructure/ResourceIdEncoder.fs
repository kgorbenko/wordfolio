module Wordfolio.Api.Infrastructure.ResourceIdEncoder

open Microsoft.Extensions.Options
open Sqids

open Wordfolio.Api.Configuration.SqidsEncoder

type IResourceIdEncoder =
    abstract member Encode: int -> string
    abstract member Decode: string -> int option

type ResourceIdEncoder(options: IOptions<SqidsEncoderConfiguration>) =
    let sqids =
        SqidsEncoder(SqidsOptions(Alphabet = options.Value.Alphabet))

    interface IResourceIdEncoder with
        member _.Encode(id: int) = sqids.Encode(id)

        member _.Decode(encoded: string) =
            let ids = sqids.Decode(encoded)

            if
                ids.Count = 1
                && ids.[0] >= 0
                && sqids.Encode(ids.[0]) = encoded
            then
                Some ids.[0]
            else
                None
