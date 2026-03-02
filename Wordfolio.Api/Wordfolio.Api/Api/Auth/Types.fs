namespace Wordfolio.Api.Api.Auth

type PasswordRequirementsResponse =
    { RequiredLength: int
      RequireDigit: bool
      RequireLowercase: bool
      RequireUppercase: bool
      RequireNonAlphanumeric: bool
      RequiredUniqueChars: int }
