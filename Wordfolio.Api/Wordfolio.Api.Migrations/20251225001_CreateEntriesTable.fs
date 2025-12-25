namespace Wordfolio.Api.Migrations

open FluentMigrator

open Constants

[<Migration(20251225001L)>]
type CreateEntriesTable() =
    inherit AutoReversingMigration()

    override this.Up() =
        base.Create
            .Table(EntriesTable.Name)
            .InSchema(WordfolioSchema)
            .WithColumn(EntriesTable.IdColumn)
            .AsInt32()
            .PrimaryKey()
            .Identity()
            .WithColumn(EntriesTable.VocabularyIdColumn)
            .AsInt32()
            .NotNullable()
            .WithColumn(EntriesTable.EntryTextColumn)
            .AsString(255)
            .NotNullable()
            .WithColumn(EntriesTable.CreatedAtColumn)
            .AsDateTimeOffset()
            .NotNullable()
            .WithColumn(EntriesTable.UpdatedAtColumn)
            .AsDateTimeOffset()
            .Nullable()
        |> ignore

        base.Create
            .ForeignKey("FK_Entries_Vocabularies_VocabularyId")
            .FromTable(EntriesTable.Name)
            .InSchema(WordfolioSchema)
            .ForeignColumn(EntriesTable.VocabularyIdColumn)
            .ToTable(VocabulariesTable.Name)
            .InSchema(WordfolioSchema)
            .PrimaryColumn(VocabulariesTable.IdColumn)
            .OnDelete(System.Data.Rule.Cascade)
        |> ignore

        base.Create
            .Index("IX_Entries_VocabularyId")
            .OnTable(EntriesTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn(EntriesTable.VocabularyIdColumn)
        |> ignore

        base.Create
            .Index("IX_Entries_VocabularyId_EntryText")
            .OnTable(EntriesTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn(EntriesTable.VocabularyIdColumn)
            .Ascending()
            .OnColumn(EntriesTable.EntryTextColumn)
            .Ascending()
        |> ignore
