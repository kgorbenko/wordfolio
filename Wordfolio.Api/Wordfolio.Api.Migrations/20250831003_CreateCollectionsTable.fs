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
            .WithColumn(CollectionsTable.IdColumn)
            .AsInt32()
            .PrimaryKey()
            .WithColumn(CollectionsTable.UserIdColumn)
            .AsInt32()
            .NotNullable()
            .WithColumn(CollectionsTable.NameColumn)
            .AsString(255)
            .NotNullable()
            .WithColumn(CollectionsTable.DescriptionColumn)
            .AsString()
            .Nullable()
            .WithColumn(CollectionsTable.CreatedAtColumn)
            .AsDateTimeOffset()
            .NotNullable()
            .WithColumn(CollectionsTable.UpdatedAtColumn)
            .AsDateTimeOffset()
            .NotNullable()
        |> ignore

        base.Create
            .ForeignKey("FK_Collections_UserId_Users_Id")
            .FromTable(CollectionsTable.Name)
            .InSchema(WordfolioSchema)
            .ForeignColumn(CollectionsTable.UserIdColumn)
            .ToTable(UsersTable.Name)
            .InSchema(WordfolioSchema)
            .PrimaryColumn("Id")
        |> ignore

        base.Create
            .Index("IX_Collections_UserId")
            .OnTable(CollectionsTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn(CollectionsTable.UserIdColumn)
        |> ignore
