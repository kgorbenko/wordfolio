module Wordfolio.Api.Api.Helpers

open System
open System.Security.Claims

open Wordfolio.Api.Infrastructure.ResourceIdEncoder

let getUserId(user: ClaimsPrincipal) : int option =
    let claim =
        user.FindFirst(ClaimTypes.NameIdentifier)

    match claim with
    | null -> None
    | userClaim ->
        match Int32.TryParse(userClaim.Value) with
        | true, userId -> Some userId
        | false, _ -> None

let okOrFail(result: Result<'Value, unit>) : 'Value =
    match result with
    | Ok value -> value
    | Error() -> failwith "Expected value to be Ok, but got Error"

type ResourceIdsHelper =
    static member Decode(encoder: IResourceIdEncoder, a: string, b: string) =
        match encoder.Decode(a), encoder.Decode(b) with
        | Some a, Some b -> Some(a, b)
        | _ -> None

    static member Decode(encoder: IResourceIdEncoder, a: string, b: string, c: string) =
        match encoder.Decode(a), encoder.Decode(b), encoder.Decode(c) with
        | Some a, Some b, Some c -> Some(a, b, c)
        | _ -> None
