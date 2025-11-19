namespace Wordfolio.Api.Migrations

open FluentMigrator

open Constants

[<Migration(20250831003L)>]
type CreateCollectionsTable() =
    inherit AutoReversingMigration()

    override this.Up() =
        base.Create
            .Table(CollectionsTable.Name)
            .InSchema(WordfolioSchema)
            .WithColumn("Id")
            .AsInt32()
            .PrimaryKey()
            .WithColumn("UserId")
            .AsInt32()
            .NotNullable()
            .WithColumn("Name")
            .AsString(255)
            .NotNullable()
            .WithColumn("Description")
            .AsString()
            .Nullable()
            .WithColumn("CreatedAtDateTime")
            .AsDateTimeOffset()
            .NotNullable()
            .WithColumn("CreatedAtOffset")
            .AsInt16()
            .NotNullable()
            .WithColumn("UpdatedAtDateTime")
            .AsDateTimeOffset()
            .NotNullable()
            .WithColumn("UpdatedAtOffset")
            .AsInt16()
            .NotNullable()
        |> ignore

        base.Create
            .ForeignKey("FK_Collections_UserId_Users_Id")
            .FromTable(CollectionsTable.Name)
            .InSchema(WordfolioSchema)
            .ForeignColumn("UserId")
            .ToTable(UsersTable.Name)
            .InSchema(WordfolioSchema)
            .PrimaryColumn("Id")
        |> ignore

        base.Create
            .Index("IX_Collections_UserId")
            .OnTable(CollectionsTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn("UserId")
        |> ignore
