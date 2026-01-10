namespace Wordfolio.Api.Migrations

open FluentMigrator

open Constants

[<Migration(20260110001L)>]
type AddSystemCollectionsAndDefaultVocabularies() =
    inherit AutoReversingMigration()

    override this.Up() =
        base.Alter
            .Table(CollectionsTable.Name)
            .InSchema(WordfolioSchema)
            .AddColumn(CollectionsTable.IsSystemColumn)
            .AsBoolean()
            .NotNullable()
            .WithDefaultValue(false)
        |> ignore

        base.Alter
            .Table(VocabulariesTable.Name)
            .InSchema(WordfolioSchema)
            .AddColumn(VocabulariesTable.IsDefaultColumn)
            .AsBoolean()
            .NotNullable()
            .WithDefaultValue(false)
        |> ignore
