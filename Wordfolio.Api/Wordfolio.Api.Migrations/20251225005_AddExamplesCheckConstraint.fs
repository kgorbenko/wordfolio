namespace Wordfolio.Api.Migrations

open FluentMigrator

open Constants

[<Migration(20251225005L)>]
type AddExamplesCheckConstraint() =
    inherit Migration()

    override this.Up() =
        this.Execute.Sql(
            $"""ALTER TABLE "{WordfolioSchema}"."{ExamplesTable.Name}" ADD CONSTRAINT "CK_Examples_ExclusiveParent" CHECK (("{ExamplesTable.DefinitionIdColumn}" IS NOT NULL AND "{ExamplesTable.TranslationIdColumn}" IS NULL) OR ("{ExamplesTable.DefinitionIdColumn}" IS NULL AND "{ExamplesTable.TranslationIdColumn}" IS NOT NULL))"""
        )

    override this.Down() =
        this.Execute.Sql(
            $"""ALTER TABLE "{WordfolioSchema}"."{ExamplesTable.Name}" DROP CONSTRAINT IF EXISTS "CK_Examples_ExclusiveParent" """
        )
