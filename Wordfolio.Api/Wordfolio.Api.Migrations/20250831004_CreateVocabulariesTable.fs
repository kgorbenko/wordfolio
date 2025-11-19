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
            .Identity()
            .WithColumn("CollectionId")
            .AsInt32()
            .NotNullable()
            .ForeignKey(CollectionsTable.Name, "Id")
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
            .Index("IX_Vocabularies_CollectionId")
            .OnTable(VocabulariesTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn("CollectionId")
        |> ignore
