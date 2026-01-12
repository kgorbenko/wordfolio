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

let insertOutputAsync<'TInput, 'TOutput>
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    (insertFunc: InsertQuery<'TInput>)
    : Task<seq<'TOutput>> =
    task {
        let! results =
            connection.InsertOutputAsync<'TInput, 'TOutput>(
                insertFunc,
                trans = transaction,
                cancellationToken = cancellationToken
            )

        return results
    }

let insertOutputSingleAsync<'TInput, 'TOutput>
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    (insertFunc: InsertQuery<'TInput>)
    : Task<'TOutput> =
    task {
        let! results = insertOutputAsync<'TInput, 'TOutput> connection transaction cancellationToken insertFunc

        return
            results
            |> Seq.tryHead
            |> Option.defaultWith(fun () -> failwith "Insert operation failed: no records were returned")
    }

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

let selectSingleAsync
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    (selectFunc: SelectQuery)
    : Task<'a option> =
    task {
        let! result = connection.SelectAsync<'a>(selectFunc, trans = transaction, cancellationToken = cancellationToken)

        let items =
            result |> Seq.truncate 2 |> Seq.toList

        match items with
        | [] -> return None
        | [ single ] -> return Some single
        | _ -> return failwith "Query returned more than one element when at most one was expected"
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
