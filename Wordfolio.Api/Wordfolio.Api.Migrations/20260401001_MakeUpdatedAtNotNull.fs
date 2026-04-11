namespace Wordfolio.Api.Migrations

open FluentMigrator

open Constants

[<Migration(20260401001L)>]
type MakeUpdatedAtNotNull() =
    inherit Migration()

    override this.Up() =
        base.Alter
            .Table(CollectionsTable.Name)
            .InSchema(WordfolioSchema)
            .AlterColumn(CollectionsTable.UpdatedAtColumn)
            .AsDateTimeOffset()
            .NotNullable()
        |> ignore

        base.Alter
            .Table(VocabulariesTable.Name)
            .InSchema(WordfolioSchema)
            .AlterColumn(VocabulariesTable.UpdatedAtColumn)
            .AsDateTimeOffset()
            .NotNullable()
        |> ignore

        base.Alter
            .Table(EntriesTable.Name)
            .InSchema(WordfolioSchema)
            .AlterColumn(EntriesTable.UpdatedAtColumn)
            .AsDateTimeOffset()
            .NotNullable()
        |> ignore

    override this.Down() =
        base.Alter
            .Table(CollectionsTable.Name)
            .InSchema(WordfolioSchema)
            .AlterColumn(CollectionsTable.UpdatedAtColumn)
            .AsDateTimeOffset()
            .Nullable()
        |> ignore

        base.Alter
            .Table(VocabulariesTable.Name)
            .InSchema(WordfolioSchema)
            .AlterColumn(VocabulariesTable.UpdatedAtColumn)
            .AsDateTimeOffset()
            .Nullable()
        |> ignore

        base.Alter
            .Table(EntriesTable.Name)
            .InSchema(WordfolioSchema)
            .AlterColumn(EntriesTable.UpdatedAtColumn)
            .AsDateTimeOffset()
            .Nullable()
        |> ignore
