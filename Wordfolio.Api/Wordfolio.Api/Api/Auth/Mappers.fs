module Wordfolio.Api.Api.Auth.Mappers

open Microsoft.AspNetCore.Identity

open Wordfolio.Api.Api.Auth

let toPasswordRequirementsResponse(passwordOptions: PasswordOptions) : PasswordRequirementsResponse =
    { RequiredLength = passwordOptions.RequiredLength
      RequireDigit = passwordOptions.RequireDigit
      RequireLowercase = passwordOptions.RequireLowercase
      RequireUppercase = passwordOptions.RequireUppercase
      RequireNonAlphanumeric = passwordOptions.RequireNonAlphanumeric
      RequiredUniqueChars = passwordOptions.RequiredUniqueChars }
