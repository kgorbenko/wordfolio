module Wordfolio.Api.Api.Helpers

open System
open System.Security.Claims

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
