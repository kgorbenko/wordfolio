namespace Wordfolio.Api.Migrations

open FluentMigrator

open Constants

[<Migration(20251225002L)>]
type CreateDefinitionsTable() =
    inherit AutoReversingMigration()

    override this.Up() =
        base.Create
            .Table(DefinitionsTable.Name)
            .InSchema(WordfolioSchema)
            .WithColumn(DefinitionsTable.IdColumn)
            .AsInt32()
            .PrimaryKey()
            .Identity()
            .WithColumn(DefinitionsTable.EntryIdColumn)
            .AsInt32()
            .NotNullable()
            .WithColumn(DefinitionsTable.DefinitionTextColumn)
            .AsString(255)
            .NotNullable()
            .WithColumn(DefinitionsTable.SourceColumn)
            .AsInt16()
            .NotNullable()
            .WithColumn(DefinitionsTable.DisplayOrderColumn)
            .AsInt32()
            .NotNullable()
        |> ignore

        base.Create
            .ForeignKey("FK_Definitions_Entries_EntryId")
            .FromTable(DefinitionsTable.Name)
            .InSchema(WordfolioSchema)
            .ForeignColumn(DefinitionsTable.EntryIdColumn)
            .ToTable(EntriesTable.Name)
            .InSchema(WordfolioSchema)
            .PrimaryColumn(EntriesTable.IdColumn)
            .OnDelete(System.Data.Rule.Cascade)
        |> ignore

        base.Create
            .Index("IX_Definitions_EntryId")
            .OnTable(DefinitionsTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn(DefinitionsTable.EntryIdColumn)
        |> ignore

        base.Create
            .UniqueConstraint("UQ_Definitions_EntryId_DisplayOrder")
            .OnTable(DefinitionsTable.Name)
            .WithSchema(WordfolioSchema)
            .Columns(DefinitionsTable.EntryIdColumn, DefinitionsTable.DisplayOrderColumn)
        |> ignore
