namespace Wordfolio.Api.Migrations

open FluentMigrator

open Constants

[<Migration(20251225004L)>]
type CreateExamplesTable() =
    inherit AutoReversingMigration()

    override this.Up() =
        base.Create
            .Table(ExamplesTable.Name)
            .InSchema(WordfolioSchema)
            .WithColumn(ExamplesTable.IdColumn)
            .AsInt32()
            .PrimaryKey()
            .Identity()
            .WithColumn(ExamplesTable.DefinitionIdColumn)
            .AsInt32()
            .Nullable()
            .WithColumn(ExamplesTable.TranslationIdColumn)
            .AsInt32()
            .Nullable()
            .WithColumn(ExamplesTable.ExampleTextColumn)
            .AsString(500)
            .NotNullable()
            .WithColumn(ExamplesTable.SourceColumn)
            .AsInt16()
            .NotNullable()
        |> ignore

        base.Create
            .ForeignKey("FK_Examples_Definitions_DefinitionId")
            .FromTable(ExamplesTable.Name)
            .InSchema(WordfolioSchema)
            .ForeignColumn(ExamplesTable.DefinitionIdColumn)
            .ToTable(DefinitionsTable.Name)
            .InSchema(WordfolioSchema)
            .PrimaryColumn(DefinitionsTable.IdColumn)
            .OnDelete(System.Data.Rule.Cascade)
        |> ignore

        base.Create
            .ForeignKey("FK_Examples_Translations_TranslationId")
            .FromTable(ExamplesTable.Name)
            .InSchema(WordfolioSchema)
            .ForeignColumn(ExamplesTable.TranslationIdColumn)
            .ToTable(TranslationsTable.Name)
            .InSchema(WordfolioSchema)
            .PrimaryColumn(TranslationsTable.IdColumn)
            .OnDelete(System.Data.Rule.Cascade)
        |> ignore

        base.Create
            .Index("IX_Examples_DefinitionId")
            .OnTable(ExamplesTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn(ExamplesTable.DefinitionIdColumn)
        |> ignore

        base.Create
            .Index("IX_Examples_TranslationId")
            .OnTable(ExamplesTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn(ExamplesTable.TranslationIdColumn)
        |> ignore
