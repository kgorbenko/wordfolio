namespace Wordfolio.Api.Migrations

open FluentMigrator

open Constants

[<Migration(20251225003L)>]
type CreateTranslationsTable() =
    inherit AutoReversingMigration()

    override this.Up() =
        base.Create
            .Table(TranslationsTable.Name)
            .InSchema(WordfolioSchema)
            .WithColumn(TranslationsTable.IdColumn)
            .AsInt32()
            .PrimaryKey()
            .Identity()
            .WithColumn(TranslationsTable.EntryIdColumn)
            .AsInt32()
            .NotNullable()
            .WithColumn(TranslationsTable.TranslationTextColumn)
            .AsString(255)
            .NotNullable()
            .WithColumn(TranslationsTable.SourceColumn)
            .AsInt16()
            .NotNullable()
            .WithColumn(TranslationsTable.DisplayOrderColumn)
            .AsInt32()
            .NotNullable()
        |> ignore

        base.Create
            .ForeignKey("FK_Translations_Entries_EntryId")
            .FromTable(TranslationsTable.Name)
            .InSchema(WordfolioSchema)
            .ForeignColumn(TranslationsTable.EntryIdColumn)
            .ToTable(EntriesTable.Name)
            .InSchema(WordfolioSchema)
            .PrimaryColumn(EntriesTable.IdColumn)
            .OnDelete(System.Data.Rule.Cascade)
        |> ignore

        base.Create
            .Index("IX_Translations_EntryId")
            .OnTable(TranslationsTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn(TranslationsTable.EntryIdColumn)
        |> ignore

        base.Create
            .UniqueConstraint("UQ_Translations_EntryId_DisplayOrder")
            .OnTable(TranslationsTable.Name)
            .WithSchema(WordfolioSchema)
            .Columns(TranslationsTable.EntryIdColumn, TranslationsTable.DisplayOrderColumn)
        |> ignore
