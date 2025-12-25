module Wordfolio.Api.DataAccess.Definitions

open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.PostgreSQL
open Microsoft.FSharp.Core.LanguagePrimitives

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

type DefinitionCreationParameters =
    { EntryId: int
      DefinitionText: string
      Source: DefinitionSource
      DisplayOrder: int }

type DefinitionUpdateParameters =
    { Id: int
      DefinitionText: string
      Source: DefinitionSource
      DisplayOrder: int }

let private fromRecord(record: DefinitionRecord) : Definition =
    { Id = record.Id
      EntryId = record.EntryId
      DefinitionText = record.DefinitionText
      Source = EnumOfValue<int16, DefinitionSource>(record.Source)
      DisplayOrder = record.DisplayOrder }

let internal definitionsTable =
    table'<DefinitionRecord> Schema.DefinitionsTable.Name
    |> inSchema Schema.Name

[<CLIMutable>]
type internal DefinitionCreationRecord =
    { EntryId: int
      DefinitionText: string
      Source: int16
      DisplayOrder: int }

let internal definitionsInsertTable =
    table'<DefinitionCreationRecord> Schema.DefinitionsTable.Name
    |> inSchema Schema.Name

let createDefinitionsAsync
    (parameters: DefinitionCreationParameters list)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int list> =
    task {
        if parameters.IsEmpty then
            return []
        else
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

let getDefinitionsByEntryIdAsync
    (entryId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Definition list> =
    task {
        let! results =
            select {
                for d in definitionsTable do
                    where(d.EntryId = entryId)
                    orderBy d.DisplayOrder
            }
            |> selectAsync connection transaction cancellationToken

        return results |> List.map fromRecord
    }

let updateDefinitionsAsync
    (parameters: DefinitionUpdateParameters list)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        if parameters.IsEmpty then
            return 0
        else
            // TODO: Optimize to do batch update
            let updateSingle(param: DefinitionUpdateParameters) =
                task {
                    let! affectedRows =
                        update {
                            for d in definitionsTable do
                                setColumn d.DefinitionText param.DefinitionText
                                setColumn d.Source (int16 param.Source)
                                setColumn d.DisplayOrder param.DisplayOrder
                                where(d.Id = param.Id)
                        }
                        |> updateAsync connection transaction cancellationToken

                    return affectedRows
                }

            let mutable totalAffected = 0

            for param in parameters do
                let! affectedRows = updateSingle param
                totalAffected <- totalAffected + affectedRows

            return totalAffected
    }

let deleteDefinitionsAsync
    (ids: int list)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        if ids.IsEmpty then
            return 0
        else
            let! affectedRows =
                delete {
                    for d in definitionsTable do
                        where(isIn d.Id ids)
                }
                |> deleteAsync connection transaction cancellationToken

            return affectedRows
    }
