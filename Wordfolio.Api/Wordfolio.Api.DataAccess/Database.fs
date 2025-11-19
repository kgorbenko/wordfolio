module Wordfolio.Api.DataAccess.Database

open System.Data
open System.Threading
open System.Threading.Tasks

open Npgsql

let private withConnectionAsync
    (dataSource: NpgsqlDataSource)
    (cancellationToken: CancellationToken)
    (doAsync: IDbConnection -> CancellationToken -> Task<'a>)
    : Task<'a> =
    task {
        use! connection = dataSource.OpenConnectionAsync(cancellationToken)

        let! result = doAsync connection cancellationToken

        return result
    }

let private inTransactionAsync
    (connection: IDbConnection)
    (cancellationToken: CancellationToken)
    (doAsync: IDbConnection -> IDbTransaction -> CancellationToken -> Task<'a>)
    : Task<'a> =
    task {
        use transaction =
            connection.BeginTransaction()

        let! result = doAsync connection transaction cancellationToken
        transaction.Commit()

        return result
    }
