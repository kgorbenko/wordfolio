module Wordfolio.Api.DataAccess.Examples

open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.PostgreSQL
open Microsoft.FSharp.Core.LanguagePrimitives

open Wordfolio.Api.DataAccess.Dapper

type ExampleSource =
    | Api = 0s
    | Custom = 1s

[<CLIMutable>]
type internal ExampleRecord =
    { Id: int
      DefinitionId: int option
      TranslationId: int option
      ExampleText: string
      Source: int16 }

type Example =
    { Id: int
      DefinitionId: int option
      TranslationId: int option
      ExampleText: string
      Source: ExampleSource }

type ExampleCreationParameters =
    { DefinitionId: int option
      TranslationId: int option
      ExampleText: string
      Source: ExampleSource }

type ExampleUpdateParameters =
    { Id: int
      ExampleText: string
      Source: ExampleSource }

let private fromRecord(record: ExampleRecord) : Example =
    { Id = record.Id
      DefinitionId = record.DefinitionId
      TranslationId = record.TranslationId
      ExampleText = record.ExampleText
      Source = EnumOfValue<int16, ExampleSource>(record.Source) }

let internal examplesTable =
    table'<ExampleRecord> Schema.ExamplesTable.Name
    |> inSchema Schema.Name

[<CLIMutable>]
type internal ExampleCreationRecord =
    { DefinitionId: int option
      TranslationId: int option
      ExampleText: string
      Source: int16 }

let internal examplesInsertTable =
    table'<ExampleCreationRecord> Schema.ExamplesTable.Name
    |> inSchema Schema.Name

let createExamplesAsync
    (parameters: ExampleCreationParameters list)
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
                    { DefinitionId = p.DefinitionId
                      TranslationId = p.TranslationId
                      ExampleText = p.ExampleText
                      Source = int16 p.Source })

            let! records =
                insert {
                    into examplesInsertTable
                    values recordsToInsert
                }
                |> insertOutputAsync<ExampleCreationRecord, ExampleRecord> connection transaction cancellationToken

            return
                records
                |> Seq.map(fun r -> r.Id)
                |> Seq.toList
    }

let getExamplesByDefinitionIdAsync
    (definitionId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Example list> =
    task {
        let! results =
            select {
                for e in examplesTable do
                    where(e.DefinitionId = Some definitionId)
            }
            |> selectAsync connection transaction cancellationToken

        return results |> List.map fromRecord
    }

let getExamplesByTranslationIdAsync
    (translationId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Example list> =
    task {
        let! results =
            select {
                for e in examplesTable do
                    where(e.TranslationId = Some translationId)
            }
            |> selectAsync connection transaction cancellationToken

        return results |> List.map fromRecord
    }

let updateExamplesAsync
    (parameters: ExampleUpdateParameters list)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int> =
    task {
        if parameters.IsEmpty then
            return 0
        else
            let updateSingle(param: ExampleUpdateParameters) =
                task {
                    let! affectedRows =
                        update {
                            for e in examplesTable do
                                setColumn e.ExampleText param.ExampleText
                                setColumn e.Source (int16 param.Source)
                                where(e.Id = param.Id)
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

let deleteExamplesAsync
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
                    for e in examplesTable do
                        where(isIn e.Id ids)
                }
                |> deleteAsync connection transaction cancellationToken

            return affectedRows
    }
