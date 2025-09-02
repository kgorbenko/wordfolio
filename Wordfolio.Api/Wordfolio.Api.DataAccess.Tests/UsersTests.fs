namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading
open System.Threading.Tasks
open Xunit
open Npgsql
open Wordfolio.Api.DataAccess

[<Collection("postgres")>]
type UsersTests(fixture: PostgresFixture) =

    [<Fact>]
    member _.``createUserAsync inserts a row`` () = task {
        // Arrange
        let ds : NpgsqlDataSource = fixture.DataSource
        use! conn = ds.OpenConnectionAsync()
        use tx = conn.BeginTransaction()
        let ct = CancellationToken.None

        // Act
        do! Users.createUserAsync { Users.UserCreationParameters.Id = 123 } conn tx ct

        // Assert - query back using plain SQL
        use cmd = new NpgsqlCommand("SELECT COUNT(*) FROM wordfolio.\"Users\" WHERE \"Id\" = 123", conn, tx)
        let! countObj = cmd.ExecuteScalarAsync(ct)
        let count = Convert.ToInt32(countObj)
        Assert.Equal(1, count)
    }
