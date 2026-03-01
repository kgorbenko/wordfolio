module Wordfolio.Api.DataAccess.Definitions

open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.PostgreSQL

open Wordfolio.Api.DataAccess.Dapper

type DefinitionSource =
    | Api = 0s
    | Manual = 1s

[<CLIMutable>]
type internal DefinitionRecord =
    { Id: int
      EntryId: int
      DefinitionText: string
      Source: int16
      DisplayOrder: int }

type Definition =
    { Id: int
      EntryId: int
      DefinitionText: string
      Source: DefinitionSource
      DisplayOrder: int }

type CreateDefinitionParameters =
    { EntryId: int
      DefinitionText: string
      Source: DefinitionSource
      DisplayOrder: int }

let internal definitionsTable =
    table'<DefinitionRecord> Schema.DefinitionsTable.Name
    |> inSchema Schema.Name

[<CLIMutable>]
type internal DefinitionCreationRecord =
    { EntryId: int
      DefinitionText: string
      Source: int16
      DisplayOrder: int }

let createDefinitionsAsync
    (parameters: CreateDefinitionParameters list)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int list> =
    task {
        if parameters.IsEmpty then
            return []
        else
            let definitionsInsertTable =
                table'<DefinitionCreationRecord> Schema.DefinitionsTable.Name
                |> inSchema Schema.Name

            let recordsToInsert =
                parameters
                |> List.map(fun p ->
                    { EntryId = p.EntryId
                      DefinitionText = p.DefinitionText
                      Source = int16 p.Source
                      DisplayOrder = p.DisplayOrder })

            let! records =
                insert {
                    into definitionsInsertTable
                    values recordsToInsert
                }
                |> insertOutputAsync<DefinitionCreationRecord, DefinitionRecord>
                    connection
                    transaction
                    cancellationToken

            return
                records
                |> Seq.map(fun r -> r.Id)
                |> Seq.toList
    }
