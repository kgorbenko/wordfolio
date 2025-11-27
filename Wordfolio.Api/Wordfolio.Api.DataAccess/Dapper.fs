module Wordfolio.Api.DataAccess.Dapper

open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.PostgreSQL

let registerTypes() =
    OptionTypes.register()
    ()

let insertAsync
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    (insertFunc: InsertQuery<'a>)
    : Task<int> =
    task { return! connection.InsertAsync(insertFunc, trans = transaction, cancellationToken = cancellationToken) }

let selectAsync
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    (selectFunc: SelectQuery)
    : Task<'a list> =
    task {
        let! result = connection.SelectAsync<'a>(selectFunc, trans = transaction, cancellationToken = cancellationToken)
        return result |> List.ofSeq
    }

let trySelectFirstAsync
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    (selectFunc: SelectQuery)
    : Task<'a option> =
    task {
        let! result = connection.SelectAsync<'a>(selectFunc, trans = transaction, cancellationToken = cancellationToken)
        return result |> Seq.tryHead
    }

let updateAsync
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    (updateFunc: UpdateQuery<'a>)
    : Task<int> =
    task { return! connection.UpdateAsync(updateFunc, trans = transaction, cancellationToken = cancellationToken) }

let deleteAsync
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    (deleteFunc: DeleteQuery)
    : Task<int> =
    task { return! connection.DeleteAsync(deleteFunc, trans = transaction, cancellationToken = cancellationToken) }
