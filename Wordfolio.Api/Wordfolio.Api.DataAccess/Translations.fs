module Wordfolio.Api.DataAccess.Translations

open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.PostgreSQL
open Microsoft.FSharp.Core.LanguagePrimitives

open Wordfolio.Api.DataAccess.Dapper

type TranslationSource =
    | Api = 0s
    | Manual = 1s

[<CLIMutable>]
type internal TranslationRecord =
    { Id: int
      EntryId: int
      TranslationText: string
      Source: int16
      DisplayOrder: int }

type Translation =
    { Id: int
      EntryId: int
      TranslationText: string
      Source: TranslationSource
      DisplayOrder: int }

type TranslationCreationParameters =
    { EntryId: int
      TranslationText: string
      Source: TranslationSource
      DisplayOrder: int }

type TranslationUpdateParameters =
    { Id: int
      TranslationText: string
      Source: TranslationSource
      DisplayOrder: int }

let private fromRecord(record: TranslationRecord) : Translation =
    { Id = record.Id
      EntryId = record.EntryId
      TranslationText = record.TranslationText
      Source = EnumOfValue<int16, TranslationSource>(record.Source)
      DisplayOrder = record.DisplayOrder }

let internal translationsTable =
    table'<TranslationRecord> Schema.TranslationsTable.Name
    |> inSchema Schema.Name

[<CLIMutable>]
type internal TranslationCreationRecord =
    { EntryId: int
      TranslationText: string
      Source: int16
      DisplayOrder: int }

let internal translationsInsertTable =
    table'<TranslationCreationRecord> Schema.TranslationsTable.Name
    |> inSchema Schema.Name

let createTranslationsAsync
    (parameters: TranslationCreationParameters list)
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
                      TranslationText = p.TranslationText
                      Source = int16 p.Source
                      DisplayOrder = p.DisplayOrder })

            let! records =
                insert {
                    into translationsInsertTable
                    values recordsToInsert
                }
                |> insertOutputAsync<TranslationCreationRecord, TranslationRecord>
                    connection
                    transaction
                    cancellationToken

            return
                records
                |> Seq.map(fun r -> r.Id)
                |> Seq.toList
    }

let getTranslationsByEntryIdAsync
    (entryId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Translation list> =
    task {
        let! results =
            select {
                for t in translationsTable do
                    where(t.EntryId = entryId)
                    orderBy t.DisplayOrder
            }
            |> selectAsync connection transaction cancellationToken

        return results |> List.map fromRecord
    }

let updateTranslationsAsync
    (parameters: TranslationUpdateParameters list)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        if parameters.IsEmpty then
            return 0
        else
            // TODO: Optimize to do batch update
            let updateSingle(param: TranslationUpdateParameters) =
                task {
                    let! affectedRows =
                        update {
                            for t in translationsTable do
                                setColumn t.TranslationText param.TranslationText
                                setColumn t.Source (int16 param.Source)
                                setColumn t.DisplayOrder param.DisplayOrder
                                where(t.Id = param.Id)
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

let deleteTranslationsAsync
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
                    for t in translationsTable do
                        where(isIn t.Id ids)
                }
                |> deleteAsync connection transaction cancellationToken

            return affectedRows
    }
