module Wordfolio.Api.DataAccess.CollectionsHierarchy

open System
open System.Data
open System.Threading
open System.Threading.Tasks

open Dapper

type VocabularySummary =
    { Id: int
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

type CollectionOverview =
    { Id: int
      UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      VocabularyCount: int }

type CollectionSortBy =
    | Name
    | CreatedAt
    | UpdatedAt
    | VocabularyCount

type SortDirection =
    | Asc
    | Desc

type SearchUserCollectionsQuery =
    { Search: string option
      SortBy: CollectionSortBy
      SortDirection: SortDirection }

[<CLIMutable>]
type internal CollectionOverviewRecord =
    { Id: int
      UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      VocabularyCount: int }

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
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      IsDefault: bool
      EntryCount: int }

let private toCollectionSummaries(results: seq<CollectionRecord * VocabularyRecord option>) : CollectionSummary list =
    results
    |> Seq.groupBy fst
    |> Seq.map(fun (collection, rows) ->
        let vocabularies =
            rows
            |> Seq.choose snd
            |> Seq.map(fun v ->
                { Id = v.Id
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
                v."Id", v."Name", v."Description", v."CreatedAt", v."UpdatedAt", v."IsDefault",
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

        return toCollectionSummaries results
    }

let private escapeLikeWildcards(input: string) : string =
    input.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_")

let searchUserCollectionsAsync
    (userId: int)
    (query: SearchUserCollectionsQuery)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<CollectionOverview list> =
    task {
        let sortDirection =
            match query.SortDirection with
            | Asc -> "ASC"
            | Desc -> "DESC"

        let orderByColumn =
            match query.SortBy with
            | CollectionSortBy.Name -> "LOWER(c.\"Name\")"
            | CollectionSortBy.CreatedAt -> "c.\"CreatedAt\""
            | CollectionSortBy.UpdatedAt -> "c.\"UpdatedAt\""
            | CollectionSortBy.VocabularyCount -> "\"VocabularyCount\""

        let searchText = "CAST(@Search AS text)"

        let sql =
            $"""
            SELECT
                c."Id", c."UserId", c."Name", c."Description", c."CreatedAt", c."UpdatedAt",
                COALESCE(v_counts."VocabularyCount", 0) AS "VocabularyCount"
            FROM wordfolio."Collections" c
            LEFT JOIN (
                SELECT
                    v."CollectionId",
                    COUNT(*) AS "VocabularyCount"
                FROM wordfolio."Vocabularies" v
                WHERE v."IsDefault" = false
                GROUP BY v."CollectionId"
            ) v_counts ON v_counts."CollectionId" = c."Id"
            WHERE c."UserId" = @UserId
              AND c."IsSystem" = false
              AND (
                    {searchText} IS NULL
                    OR c."Name" ILIKE '%%' || {searchText} || '%%'
                    OR COALESCE(c."Description", '') ILIKE '%%' || {searchText} || '%%'
              )
            ORDER BY {orderByColumn} {sortDirection}
            """

        let commandDefinition =
            CommandDefinition(
                commandText = sql,
                parameters =
                    {| UserId = userId
                       Search =
                        query.Search
                        |> Option.map escapeLikeWildcards |},
                transaction = transaction,
                cancellationToken = cancellationToken
            )

        let! results = connection.QueryAsync<CollectionOverviewRecord>(commandDefinition)

        return
            results
            |> Seq.map(fun c ->
                { Id = c.Id
                  UserId = c.UserId
                  Name = c.Name
                  Description = c.Description
                  CreatedAt = c.CreatedAt
                  UpdatedAt = c.UpdatedAt |> Option.ofNullable
                  VocabularyCount = c.VocabularyCount }
                : CollectionOverview)
            |> Seq.toList
    }

type VocabularySummarySortBy =
    | Name
    | CreatedAt
    | UpdatedAt
    | EntryCount

type VocabularySummaryQuery =
    { Search: string option
      SortBy: VocabularySummarySortBy
      SortDirection: SortDirection }

let searchCollectionVocabulariesAsync
    (userId: int)
    (collectionId: int)
    (query: VocabularySummaryQuery)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<VocabularySummary list> =
    task {
        let sortDirection =
            match query.SortDirection with
            | SortDirection.Asc -> "ASC"
            | SortDirection.Desc -> "DESC"

        let orderByColumn =
            match query.SortBy with
            | VocabularySummarySortBy.Name -> "LOWER(v.\"Name\")"
            | VocabularySummarySortBy.CreatedAt -> "v.\"CreatedAt\""
            | VocabularySummarySortBy.UpdatedAt -> "v.\"UpdatedAt\""
            | VocabularySummarySortBy.EntryCount -> "\"EntryCount\""

        let searchText = "CAST(@Search AS text)"

        let sql =
            $"""
            SELECT
                v."Id", v."Name", v."Description", v."CreatedAt", v."UpdatedAt", v."IsDefault",
                COALESCE(e."EntryCount", 0) as "EntryCount"
            FROM wordfolio."Vocabularies" v
            INNER JOIN wordfolio."Collections" c ON c."Id" = v."CollectionId"
            LEFT JOIN (
                SELECT "VocabularyId", COUNT(*) as "EntryCount"
                FROM wordfolio."Entries"
                GROUP BY "VocabularyId"
            ) e ON e."VocabularyId" = v."Id"
            WHERE c."UserId" = @UserId AND c."Id" = @CollectionId
              AND c."IsSystem" = false AND v."IsDefault" = false
              AND (
                    {searchText} IS NULL
                    OR v."Name" ILIKE '%%' || {searchText} || '%%'
                    OR COALESCE(v."Description", '') ILIKE '%%' || {searchText} || '%%'
              )
            ORDER BY {orderByColumn} {sortDirection}
            """

        let commandDefinition =
            CommandDefinition(
                commandText = sql,
                parameters =
                    {| UserId = userId
                       CollectionId = collectionId
                       Search =
                        query.Search
                        |> Option.map escapeLikeWildcards |},
                transaction = transaction,
                cancellationToken = cancellationToken
            )

        let! results = connection.QueryAsync<VocabularyRecord>(commandDefinition)

        return
            results
            |> Seq.map(fun v ->
                { Id = v.Id
                  Name = v.Name
                  Description = v.Description
                  CreatedAt = v.CreatedAt
                  UpdatedAt = v.UpdatedAt |> Option.ofNullable
                  EntryCount = v.EntryCount })
            |> Seq.toList
    }

let getDefaultVocabularySummaryByUserIdAsync
    (userId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<VocabularySummary option> =
    task {
        let sql =
            """
            SELECT
                v."Id", v."Name", v."Description",
                v."CreatedAt", v."UpdatedAt", v."IsDefault",
                COUNT(e."Id") as "EntryCount"
            FROM wordfolio."Vocabularies" v
            INNER JOIN wordfolio."Collections" c ON c."Id" = v."CollectionId"
            LEFT JOIN wordfolio."Entries" e ON e."VocabularyId" = v."Id"
            WHERE c."UserId" = @UserId AND v."IsDefault" = true
            GROUP BY v."Id", v."Name", v."Description",
                     v."CreatedAt", v."UpdatedAt", v."IsDefault"
            """

        let commandDefinition =
            CommandDefinition(
                commandText = sql,
                parameters = {| UserId = userId |},
                transaction = transaction,
                cancellationToken = cancellationToken
            )

        let! results = connection.QueryAsync<VocabularyRecord>(commandDefinition)

        return
            results
            |> Seq.tryHead
            |> Option.map(fun v ->
                { Id = v.Id
                  Name = v.Name
                  Description = v.Description
                  CreatedAt = v.CreatedAt
                  UpdatedAt = v.UpdatedAt |> Option.ofNullable
                  EntryCount = v.EntryCount })
    }
