namespace Wordfolio.Api.Migrations

open System.Data

open FluentMigrator

open Constants

[<Migration(20260418003L)>]
type CreateExerciseAttemptsTable() =
    inherit AutoReversingMigration()

    override this.Up() =
        base.Create
            .Table(ExerciseAttemptsTable.Name)
            .InSchema(WordfolioSchema)
            .WithColumn(ExerciseAttemptsTable.IdColumn)
            .AsInt32()
            .PrimaryKey()
            .Identity()
            .WithColumn(ExerciseAttemptsTable.UserIdColumn)
            .AsInt32()
            .NotNullable()
            .WithColumn(ExerciseAttemptsTable.SessionIdColumn)
            .AsInt32()
            .Nullable()
            .WithColumn(ExerciseAttemptsTable.EntryIdColumn)
            .AsInt32()
            .NotNullable()
            .WithColumn(ExerciseAttemptsTable.ExerciseTypeColumn)
            .AsInt16()
            .NotNullable()
            .WithColumn(ExerciseAttemptsTable.PromptDataColumn)
            .AsString()
            .NotNullable()
            .WithColumn(ExerciseAttemptsTable.PromptSchemaVersionColumn)
            .AsInt16()
            .NotNullable()
            .WithColumn(ExerciseAttemptsTable.RawAnswerColumn)
            .AsString()
            .NotNullable()
            .WithColumn(ExerciseAttemptsTable.IsCorrectColumn)
            .AsBoolean()
            .NotNullable()
            .WithColumn(ExerciseAttemptsTable.AttemptedAtColumn)
            .AsDateTimeOffset()
            .NotNullable()
        |> ignore

        base.Create
            .ForeignKey("FK_ExerciseAttempts_UserId_Users_Id")
            .FromTable(ExerciseAttemptsTable.Name)
            .InSchema(WordfolioSchema)
            .ForeignColumn(ExerciseAttemptsTable.UserIdColumn)
            .ToTable(UsersTable.Name)
            .InSchema(WordfolioSchema)
            .PrimaryColumn("Id")
        |> ignore

        base.Create
            .ForeignKey("FK_ExerciseAttempts_EntryId_Entries_Id")
            .FromTable(ExerciseAttemptsTable.Name)
            .InSchema(WordfolioSchema)
            .ForeignColumn(ExerciseAttemptsTable.EntryIdColumn)
            .ToTable(EntriesTable.Name)
            .InSchema(WordfolioSchema)
            .PrimaryColumn(EntriesTable.IdColumn)
            .OnDelete(Rule.Cascade)
        |> ignore

        base.Create
            .UniqueConstraint("UQ_ExerciseAttempts_SessionId_EntryId")
            .OnTable(ExerciseAttemptsTable.Name)
            .WithSchema(WordfolioSchema)
            .Columns([| ExerciseAttemptsTable.SessionIdColumn; ExerciseAttemptsTable.EntryIdColumn |])
        |> ignore

        base.Create
            .Index("IX_ExerciseAttempts_UserId_EntryId_AttemptedAt")
            .OnTable(ExerciseAttemptsTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn(ExerciseAttemptsTable.UserIdColumn)
            .Ascending()
            .OnColumn(ExerciseAttemptsTable.EntryIdColumn)
            .Ascending()
            .OnColumn(ExerciseAttemptsTable.AttemptedAtColumn)
            .Descending()
        |> ignore

        base.Create
            .Index("IX_ExerciseAttempts_SessionId")
            .OnTable(ExerciseAttemptsTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn(ExerciseAttemptsTable.SessionIdColumn)
        |> ignore

        base.Create
            .Index("IX_ExerciseAttempts_EntryId")
            .OnTable(ExerciseAttemptsTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn(ExerciseAttemptsTable.EntryIdColumn)
        |> ignore
