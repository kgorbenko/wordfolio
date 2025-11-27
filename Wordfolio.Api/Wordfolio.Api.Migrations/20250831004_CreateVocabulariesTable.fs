namespace Wordfolio.Api.Migrations

open FluentMigrator

open Constants

[<Migration(20250831004L)>]
type CreateVocabulariesTable() =
    inherit AutoReversingMigration()

    override this.Up() =
        base.Create
            .Table(VocabulariesTable.Name)
            .InSchema(WordfolioSchema)
            .WithColumn(VocabulariesTable.IdColumn)
            .AsInt32()
            .PrimaryKey()
            .WithColumn(VocabulariesTable.CollectionIdColumn)
            .AsInt32()
            .NotNullable()
            .WithColumn(VocabulariesTable.NameColumn)
            .AsString(255)
            .NotNullable()
            .WithColumn(VocabulariesTable.DescriptionColumn)
            .AsString()
            .Nullable()
            .WithColumn(VocabulariesTable.CreatedAtColumn)
            .AsDateTimeOffset()
            .NotNullable()
            .WithColumn(VocabulariesTable.UpdatedAtColumn)
            .AsDateTimeOffset()
            .NotNullable()
        |> ignore

        base.Create
            .ForeignKey("FK_Vocabularies_CollectionId_Collections_Id")
            .FromTable(VocabulariesTable.Name)
            .InSchema(WordfolioSchema)
            .ForeignColumn(VocabulariesTable.CollectionIdColumn)
            .ToTable(CollectionsTable.Name)
            .InSchema(WordfolioSchema)
            .PrimaryColumn(CollectionsTable.IdColumn)
        |> ignore

        base.Create
            .Index("IX_Vocabularies_CollectionId")
            .OnTable(VocabulariesTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn(VocabulariesTable.CollectionIdColumn)
        |> ignore
