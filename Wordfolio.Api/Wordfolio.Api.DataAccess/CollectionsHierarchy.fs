module Wordfolio.Api.DataAccess.CollectionsHierarchy

open System
open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper

type VocabularySummary =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      EntryCount: int }

type CollectionSummary =
    { Id: int
      UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      Vocabularies: VocabularySummary list }

[<CLIMutable>]
type internal CollectionRecord =
    { Id: int
      UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      IsSystem: bool }

[<CLIMutable>]
type internal VocabularyRecord =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      IsDefault: bool
      EntryCount: int }

let getCollectionsByUserIdAsync
    (userId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<CollectionSummary list> =
    task {
        let sql =
            """
            SELECT
                c."Id", c."UserId", c."Name", c."Description", c."CreatedAt", c."UpdatedAt", c."IsSystem",
                v."Id", v."CollectionId", v."Name", v."Description", v."CreatedAt", v."UpdatedAt", v."IsDefault",
                COALESCE(e."EntryCount", 0) as "EntryCount"
            FROM wordfolio."Collections" c
            LEFT JOIN wordfolio."Vocabularies" v ON v."CollectionId" = c."Id" AND v."IsDefault" = false
            LEFT JOIN (
                SELECT "VocabularyId", COUNT(*) as "EntryCount"
                FROM wordfolio."Entries"
                GROUP BY "VocabularyId"
            ) e ON e."VocabularyId" = v."Id"
            WHERE c."UserId" = @UserId AND c."IsSystem" = false
            ORDER BY c."Id", v."Id"
            """

        let commandDefinition =
            CommandDefinition(
                commandText = sql,
                parameters = {| UserId = userId |},
                transaction = transaction,
                cancellationToken = cancellationToken
            )

        let! results =
            connection.QueryAsync<CollectionRecord, VocabularyRecord, CollectionRecord * VocabularyRecord option>(
                commandDefinition,
                (fun c v -> (c, Option.ofObj v)),
                splitOn = "Id"
            )

        let grouped =
            results
            |> Seq.groupBy fst
            |> Seq.map(fun (collection, rows) ->
                let vocabularies =
                    rows
                    |> Seq.choose snd
                    |> Seq.map(fun v ->
                        { Id = v.Id
                          CollectionId = v.CollectionId
                          Name = v.Name
                          Description = v.Description
                          CreatedAt = v.CreatedAt
                          UpdatedAt = v.UpdatedAt |> Option.ofNullable
                          EntryCount = v.EntryCount })
                    |> Seq.toList

                { Id = collection.Id
                  UserId = collection.UserId
                  Name = collection.Name
                  Description = collection.Description
                  CreatedAt = collection.CreatedAt
                  UpdatedAt =
                    collection.UpdatedAt
                    |> Option.ofNullable
                  Vocabularies = vocabularies })
            |> Seq.toList

        return grouped
    }
