module Wordfolio.Api.DataAccess.Users

open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.PostgreSQL

open Wordfolio.Api.DataAccess.Dapper
open Wordfolio.Common

type User = { Id: int }

type UserCreationParameters = { Id: int }

let createUserAsync
    (parameters: UserCreationParameters)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<unit> =
    task {
        let usersTable =
            table'<User> Schema.UsersTable.Name
            |> inSchema Schema.Name

        let userToInsert: User =
            { Id = parameters.Id }

        do!
            insert {
                into usersTable
                values [ userToInsert ]
            }
            |> insertAsync connection transaction cancellationToken
            |> Task.ignore
    }
