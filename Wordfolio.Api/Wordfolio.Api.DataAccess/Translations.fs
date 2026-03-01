module Wordfolio.Api.DataAccess.Translations

open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.PostgreSQL

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

type CreateTranslationParameters =
    { EntryId: int
      TranslationText: string
      Source: TranslationSource
      DisplayOrder: int }

let internal translationsTable =
    table'<TranslationRecord> Schema.TranslationsTable.Name
    |> inSchema Schema.Name

[<CLIMutable>]
type internal TranslationCreationRecord =
    { EntryId: int
      TranslationText: string
      Source: int16
      DisplayOrder: int }

let createTranslationsAsync
    (parameters: CreateTranslationParameters list)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int list> =
    task {
        if parameters.IsEmpty then
            return []
        else
            let translationsInsertTable =
                table'<TranslationCreationRecord> Schema.TranslationsTable.Name
                |> inSchema Schema.Name

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
