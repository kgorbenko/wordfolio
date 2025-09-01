module Wordfolio.Api.DataAccess.Dapper

open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.PostgreSQL

let registerTypes () =
    OptionTypes.register()
    ()

let insertAsync
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    (insertFunc: InsertQuery<'a>)
    : Task<int> =
    task {
        return! connection.InsertAsync(insertFunc, trans = transaction, cancellationToken = cancellationToken)
    }