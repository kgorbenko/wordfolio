namespace Wordfolio.Api.Migrations

open FluentMigrator

open Constants

[<Migration(20260418001L)>]
type CreateExerciseSessionsTable() =
    inherit AutoReversingMigration()

    override this.Up() =
        base.Create
            .Table(ExerciseSessionsTable.Name)
            .InSchema(WordfolioSchema)
            .WithColumn(ExerciseSessionsTable.IdColumn)
            .AsInt32()
            .PrimaryKey()
            .Identity()
            .WithColumn(ExerciseSessionsTable.UserIdColumn)
            .AsInt32()
            .NotNullable()
            .WithColumn(ExerciseSessionsTable.ExerciseTypeColumn)
            .AsInt16()
            .NotNullable()
            .WithColumn(ExerciseSessionsTable.CreatedAtColumn)
            .AsDateTimeOffset()
            .NotNullable()
        |> ignore

        base.Create
            .ForeignKey("FK_ExerciseSessions_UserId_Users_Id")
            .FromTable(ExerciseSessionsTable.Name)
            .InSchema(WordfolioSchema)
            .ForeignColumn(ExerciseSessionsTable.UserIdColumn)
            .ToTable(UsersTable.Name)
            .InSchema(WordfolioSchema)
            .PrimaryColumn("Id")
        |> ignore

        base.Create
            .Index("IX_ExerciseSessions_UserId")
            .OnTable(ExerciseSessionsTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn(ExerciseSessionsTable.UserIdColumn)
        |> ignore

        base.Create
            .Index("IX_ExerciseSessions_CreatedAt")
            .OnTable(ExerciseSessionsTable.Name)
            .InSchema(WordfolioSchema)
            .OnColumn(ExerciseSessionsTable.CreatedAtColumn)
        |> ignore
