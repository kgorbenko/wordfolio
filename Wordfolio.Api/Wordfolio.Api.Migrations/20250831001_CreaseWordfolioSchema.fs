namespace Wordfolio.Api.Migrations

open FluentMigrator

open Constants

[<Migration(20250831001L)>]
type CreateWordfolioSchema() =
    inherit AutoReversingMigration()

    override this.Up() =
        base.Create.Schema(WordfolioSchema)
        |> ignore