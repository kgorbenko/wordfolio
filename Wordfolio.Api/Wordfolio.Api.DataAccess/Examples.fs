module Wordfolio.Api.DataAccess.Examples

open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.PostgreSQL
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

type CreateExampleParameters =
    { DefinitionId: int option
      TranslationId: int option
      ExampleText: string
      Source: ExampleSource }

[<CLIMutable>]
type internal ExampleCreationRecord =
    { DefinitionId: int option
      TranslationId: int option
      ExampleText: string
      Source: int16 }

let createExamplesAsync
    (parameters: CreateExampleParameters list)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int list> =
    task {
        if parameters.IsEmpty then
            return []
        else
            let examplesInsertTable =
                table'<ExampleCreationRecord> Schema.ExamplesTable.Name
                |> inSchema Schema.Name

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
