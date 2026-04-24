namespace Wordfolio.Api.Migrations

open System.Data

open FluentMigrator

open Constants

[<Migration(20260418002L)>]
type CreateExerciseSessionEntriesTable() =
    inherit AutoReversingMigration()

    override this.Up() =
        base.Create
            .Table(ExerciseSessionEntriesTable.Name)
            .InSchema(WordfolioSchema)
            .WithColumn(ExerciseSessionEntriesTable.IdColumn)
            .AsInt32()
            .PrimaryKey()
            .Identity()
            .WithColumn(ExerciseSessionEntriesTable.SessionIdColumn)
            .AsInt32()
            .NotNullable()
            .WithColumn(ExerciseSessionEntriesTable.EntryIdColumn)
            .AsInt32()
            .NotNullable()
            .WithColumn(ExerciseSessionEntriesTable.DisplayOrderColumn)
            .AsInt32()
            .NotNullable()
            .WithColumn(ExerciseSessionEntriesTable.PromptDataColumn)
            .AsString()
            .NotNullable()
            .WithColumn(ExerciseSessionEntriesTable.PromptSchemaVersionColumn)
            .AsInt16()
            .NotNullable()
        |> ignore

        base.Create
            .ForeignKey("FK_ExerciseSessionEntries_SessionId_ExerciseSessions_Id")
            .FromTable(ExerciseSessionEntriesTable.Name)
            .InSchema(WordfolioSchema)
            .ForeignColumn(ExerciseSessionEntriesTable.SessionIdColumn)
            .ToTable(ExerciseSessionsTable.Name)
            .InSchema(WordfolioSchema)
            .PrimaryColumn(ExerciseSessionsTable.IdColumn)
            .OnDelete(Rule.Cascade)
        |> ignore

        base.Create
            .ForeignKey("FK_ExerciseSessionEntries_EntryId_Entries_Id")
            .FromTable(ExerciseSessionEntriesTable.Name)
            .InSchema(WordfolioSchema)
            .ForeignColumn(ExerciseSessionEntriesTable.EntryIdColumn)
            .ToTable(EntriesTable.Name)
            .InSchema(WordfolioSchema)
            .PrimaryColumn(EntriesTable.IdColumn)
            .OnDelete(Rule.Cascade)
        |> ignore

        base.Create
            .UniqueConstraint("UQ_ExerciseSessionEntries_SessionId_EntryId")
            .OnTable(ExerciseSessionEntriesTable.Name)
            .WithSchema(WordfolioSchema)
            .Columns(
                [| ExerciseSessionEntriesTable.SessionIdColumn
                   ExerciseSessionEntriesTable.EntryIdColumn |]
            )
        |> ignore

        base.Create
            .Index("IX_ExerciseSessionEntries_SessionId")
            .OnTable(ExerciseSessionEntriesTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn(ExerciseSessionEntriesTable.SessionIdColumn)
        |> ignore

        base.Create
            .Index("IX_ExerciseSessionEntries_EntryId")
            .OnTable(ExerciseSessionEntriesTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn(ExerciseSessionEntriesTable.EntryIdColumn)
        |> ignore
