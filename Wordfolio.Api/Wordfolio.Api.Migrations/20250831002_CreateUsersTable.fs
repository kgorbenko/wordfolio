namespace Wordfolio.Api.Migrations

open FluentMigrator

open Constants

[<Migration(20250831002L)>]
type CreateUsersTable() =
    inherit AutoReversingMigration()

    override this.Up() =
        base.Create
            .Table(UsersTable.Name)
            .InSchema(WordfolioSchema)
            .WithColumn("Id")
            .AsInt32()
            .PrimaryKey()
        |> ignore
