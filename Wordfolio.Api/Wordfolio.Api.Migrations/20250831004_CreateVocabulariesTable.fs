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
            .WithColumn("Id")
            .AsInt32()
            .PrimaryKey()
            .WithColumn("CollectionId")
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
            .ForeignKey("FK_Vocabularies_CollectionId_Collections_Id")
            .FromTable(VocabulariesTable.Name)
            .InSchema(WordfolioSchema)
            .ForeignColumn("CollectionId")
            .ToTable(CollectionsTable.Name)
            .InSchema(WordfolioSchema)
            .PrimaryColumn("Id")
        |> ignore

        base.Create
            .Index("IX_Vocabularies_CollectionId")
            .OnTable(VocabulariesTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn("CollectionId")
        |> ignore
