namespace Wordfolio.Api.DataAccess.Tests

open System.Threading
open System.Data.Common
open Microsoft.EntityFrameworkCore

open Wordfolio.Api.DataAccess

[<CLIMutable>]
type UserEntity = {
    Id: int
}

type TestDbContext(options: DbContextOptions<TestDbContext>) =
    inherit DbContext(options)

    member this.Users with get(): DbSet<UserEntity> = base.Set<UserEntity>()

    override _.OnModelCreating(modelBuilder: ModelBuilder) =
        let users = modelBuilder.Entity<UserEntity>()

        users
            .ToTable(Schema.UsersTable.Name, Schema.Name)
            .HasKey(fun x -> x.Id :> obj)
        |> ignore

        users.Property(_.Id).ValueGeneratedNever() |> ignore

        base.OnModelCreating(modelBuilder)

[<AutoOpen>]
module EfTestSupport =
    let createContext (connection: DbConnection) =
        let builder = DbContextOptionsBuilder<TestDbContext>()
        builder.UseNpgsql(connection) |> ignore
        new TestDbContext(builder.Options)

    let seedUsersAsync (connection: DbConnection) (transaction: DbTransaction) (cancellationToken: CancellationToken) (users: UserEntity list) = task {
        use context = createContext connection
        context.Database.UseTransaction(transaction) |> ignore
        do! context.Users.AddRangeAsync(users, cancellationToken)
        let! _ = context.SaveChangesAsync(cancellationToken)
        return ()
    }
