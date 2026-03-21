namespace Wordfolio.Api.Migrations

open FluentMigrator

open Constants

[<Migration(20260321001L)>]
type AddCascadeDeleteToVocabulariesCollectionForeignKey() =
    inherit Migration()

    override this.Up() =
        base.Delete
            .ForeignKey("FK_Vocabularies_CollectionId_Collections_Id")
            .OnTable(VocabulariesTable.Name)
            .InSchema(WordfolioSchema)
        |> ignore

        base.Create
            .ForeignKey("FK_Vocabularies_CollectionId_Collections_Id")
            .FromTable(VocabulariesTable.Name)
            .InSchema(WordfolioSchema)
            .ForeignColumn(VocabulariesTable.CollectionIdColumn)
            .ToTable(CollectionsTable.Name)
            .InSchema(WordfolioSchema)
            .PrimaryColumn(CollectionsTable.IdColumn)
            .OnDelete(System.Data.Rule.Cascade)
        |> ignore

    override this.Down() =
        base.Delete
            .ForeignKey("FK_Vocabularies_CollectionId_Collections_Id")
            .OnTable(VocabulariesTable.Name)
            .InSchema(WordfolioSchema)
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
