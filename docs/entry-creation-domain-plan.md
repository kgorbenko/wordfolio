# Entry Creation Domain Layer Implementation Plan

## 1. Domain ID Types

**Location:** `Wordfolio.Api.Domain/Ids.fs`

Add discriminated union IDs:

```fsharp
[<Struct>]
type EntryId = | EntryId of int

[<Struct>]
type DefinitionId = | DefinitionId of int

[<Struct>]
type TranslationId = | TranslationId of int

[<Struct>]
type ExampleId = | ExampleId of int

module EntryId =
    let value(EntryId id) = id

module DefinitionId =
    let value(DefinitionId id) = id

module TranslationId =
    let value(TranslationId id) = id

module ExampleId =
    let value(ExampleId id) = id
```

---

## 2. Domain Types

**Location:** `Wordfolio.Api.Domain/Entries/Entry.fs` (new module)

```fsharp
namespace Wordfolio.Api.Domain.Entries

open System
open Wordfolio.Api.Domain

type DefinitionSource =
    | Api
    | Manual

type TranslationSource =
    | Api
    | Manual

type ExampleSource =
    | Api
    | Custom

type Example =
    { Id: ExampleId
      ExampleText: string
      Source: ExampleSource }

type Definition =
    { Id: DefinitionId
      DefinitionText: string
      Source: DefinitionSource
      DisplayOrder: int
      Examples: Example list }

type Translation =
    { Id: TranslationId
      TranslationText: string
      Source: TranslationSource
      DisplayOrder: int
      Examples: Example list }

type Entry =
    { Id: EntryId
      VocabularyId: VocabularyId
      EntryText: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      Definitions: Definition list
      Translations: Translation list }
```

---

## 3. Error Types

**Location:** `Wordfolio.Api.Domain/Entries/Errors.fs` (new module)

```fsharp
namespace Wordfolio.Api.Domain.Entries

open Wordfolio.Api.Domain

type EntryError =
    | EntryNotFound of EntryId
    | EntryTextRequired
    | EntryTextTooLong of maxLength: int
    | VocabularyNotFoundOrAccessDenied of VocabularyId
    | DuplicateEntry of existingEntryId: EntryId
    | NoDefinitionsOrTranslations
    | TooManyExamples of maxCount: int
    | ExampleTextTooLong of maxLength: int
```

---

## 4. Capabilities

**Location:** `Wordfolio.Api.Domain/Entries/Capabilities.fs` (new module)

```fsharp
namespace Wordfolio.Api.Domain.Entries

open System
open System.Threading.Tasks
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Vocabularies

type DefinitionInput =
    { DefinitionText: string
      Source: DefinitionSource
      Examples: ExampleInput list }

type TranslationInput =
    { TranslationText: string
      Source: TranslationSource
      Examples: ExampleInput list }

type ExampleInput =
    { ExampleText: string
      Source: ExampleSource }

type IGetEntryById =
    abstract GetEntryById: EntryId -> Task<Entry option>

type IGetEntriesByVocabularyId =
    abstract GetEntriesByVocabularyId: VocabularyId -> Task<Entry list>

type IGetEntryByTextAndVocabularyId =
    abstract GetEntryByTextAndVocabularyId: VocabularyId * string -> Task<Entry option>

type ICreateEntry =
    abstract CreateEntry:
        VocabularyId * string * DateTimeOffset * DefinitionInput list * TranslationInput list ->
            Task<EntryId>

type IUpdateEntry =
    abstract UpdateEntry:
        EntryId * string * DateTimeOffset * DefinitionInput list * TranslationInput list -> Task<int>

type IDeleteEntry =
    abstract DeleteEntry: EntryId -> Task<int>

type IGetVocabularyByIdAndUserId =
    abstract GetVocabularyByIdAndUserId: VocabularyId * UserId -> Task<Vocabulary option>

module Capabilities =
    let getEntryById (env: #IGetEntryById) entryId = env.GetEntryById(entryId)

    let getEntriesByVocabularyId (env: #IGetEntriesByVocabularyId) vocabularyId =
        env.GetEntriesByVocabularyId(vocabularyId)

    let getEntryByTextAndVocabularyId (env: #IGetEntryByTextAndVocabularyId) vocabularyId entryText =
        env.GetEntryByTextAndVocabularyId(vocabularyId, entryText)

    let createEntry (env: #ICreateEntry) vocabularyId entryText createdAt definitions translations =
        env.CreateEntry(vocabularyId, entryText, createdAt, definitions, translations)

    let updateEntry (env: #IUpdateEntry) entryId entryText updatedAt definitions translations =
        env.UpdateEntry(entryId, entryText, updatedAt, definitions, translations)

    let deleteEntry (env: #IDeleteEntry) entryId = env.DeleteEntry(entryId)

    let getVocabularyByIdAndUserId (env: #IGetVocabularyByIdAndUserId) vocabularyId userId =
        env.GetVocabularyByIdAndUserId(vocabularyId, userId)
```

---

## 5. Operations

**Location:** `Wordfolio.Api.Domain/Entries/Operations.fs` (new module)

```fsharp
module Wordfolio.Api.Domain.Entries.Operations

open System
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries.Capabilities
open Wordfolio.Api.Domain.Transactions

[<Literal>]
let MaxEntryTextLength = 200

[<Literal>]
let MaxExampleTextLength = 200

[<Literal>]
let MaxExamplesPerItem = 5

let private validateEntryText(text: string) : Result<string, EntryError> =
    if String.IsNullOrWhiteSpace(text) then
        Error EntryTextRequired
    elif text.Length > MaxEntryTextLength then
        Error(EntryTextTooLong MaxEntryTextLength)
    else
        Ok text

let private validateExamples(examples: ExampleInput list) : Result<ExampleInput list, EntryError> =
    if examples.Length > MaxExamplesPerItem then
        Error(TooManyExamples MaxExamplesPerItem)
    else
        let tooLongExample = examples |> List.tryFind(fun e -> e.ExampleText.Length > MaxExampleTextLength)
        match tooLongExample with
        | Some _ -> Error(ExampleTextTooLong MaxExampleTextLength)
        | None -> Ok examples

let private validateDefinitions(definitions: DefinitionInput list) : Result<DefinitionInput list, EntryError> =
    definitions
    |> List.map(fun d -> validateExamples d.Examples |> Result.map(fun _ -> d))
    |> List.fold
        (fun acc result ->
            match acc, result with
            | Ok list, Ok item -> Ok(item :: list)
            | Error e, _ -> Error e
            | _, Error e -> Error e)
        (Ok [])
    |> Result.map List.rev

let private validateTranslations(translations: TranslationInput list) : Result<TranslationInput list, EntryError> =
    translations
    |> List.map(fun t -> validateExamples t.Examples |> Result.map(fun _ -> t))
    |> List.fold
        (fun acc result ->
            match acc, result with
            | Ok list, Ok item -> Ok(item :: list)
            | Error e, _ -> Error e
            | _, Error e -> Error e)
        (Ok [])
    |> Result.map List.rev

let private checkVocabularyAccess env userId vocabularyId =
    task {
        let! maybeVocabulary = getVocabularyByIdAndUserId env vocabularyId userId

        return
            match maybeVocabulary with
            | None -> Error(VocabularyNotFoundOrAccessDenied vocabularyId)
            | Some _ -> Ok()
    }

let create env userId vocabularyId entryText definitions translations now =
    runInTransaction env (fun appEnv ->
        task {
            match validateEntryText entryText with
            | Error error -> return Error error
            | Ok validText ->
                if definitions.IsEmpty && translations.IsEmpty then
                    return Error NoDefinitionsOrTranslations
                else
                    let! vocabAccessResult = checkVocabularyAccess appEnv userId vocabularyId
                    match vocabAccessResult with
                    | Error error -> return Error error
                    | Ok _ ->
                        let! maybeExistingEntry =
                            getEntryByTextAndVocabularyId appEnv vocabularyId validText.Trim()

                        match maybeExistingEntry with
                        | Some existing -> return Error(DuplicateEntry existing.Id)
                        | None ->
                            match validateDefinitions definitions with
                            | Error error -> return Error error
                            | Ok validDefinitions ->
                                match validateTranslations translations with
                                | Error error -> return Error error
                                | Ok validTranslations ->
                                    let trimmedText = validText.Trim()

                                    let! entryId =
                                        createEntry
                                            appEnv
                                            vocabularyId
                                            trimmedText
                                            now
                                            validDefinitions
                                            validTranslations

                                    let! maybeEntry = getEntryById appEnv entryId

                                    match maybeEntry with
                                    | Some entry -> return Ok entry
                                    | None -> return Error EntryTextRequired
        })

let getById env userId entryId =
    runInTransaction env (fun appEnv ->
        task {
            let! maybeEntry = getEntryById appEnv entryId

            match maybeEntry with
            | None -> return Error(EntryNotFound entryId)
            | Some entry ->
                let! vocabAccessResult = checkVocabularyAccess appEnv userId entry.VocabularyId

                match vocabAccessResult with
                | Error _ -> return Error(EntryNotFound entryId)
                | Ok _ -> return Ok entry
        })

let getByVocabularyId env userId vocabularyId =
    runInTransaction env (fun appEnv ->
        task {
            let! vocabAccessResult = checkVocabularyAccess appEnv userId vocabularyId

            match vocabAccessResult with
            | Error error -> return Error error
            | Ok _ ->
                let! entries = getEntriesByVocabularyId appEnv vocabularyId
                return Ok entries
        })
```

---

## 6. Transaction-Based Entry Creation

Entry creation uses existing data access functions (createEntryAsync, createDefinitionsAsync, createTranslationsAsync, createExamplesAsync) within a single transaction managed by `runInTransaction` at the domain level.

**Domain Operation Flow:**
1. Begin transaction (via runInTransaction)
2. Create Entry → get EntryId
3. Create Definitions → get DefinitionIds
4. Create Translations → get TranslationIds
5. Create Examples for Definitions
6. Create Examples for Translations
7. Retrieve full entry hierarchy
8. Commit transaction (automatic on success) or rollback (automatic on error)

All operations share the same connection and transaction passed through AppEnv.

---

## 7. Missing Data Access Queries

### 7.1. Duplicate Detection

**Module:** `Wordfolio.Api.DataAccess.Entries`

```fsharp
let getEntryByTextAndVocabularyIdAsync
    (vocabularyId: int)
    (entryText: string)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Entry option>
```

**Query:**
```sql
SELECT * FROM wordfolio."Entries"
WHERE "VocabularyId" = @vocabularyId AND "EntryText" = @entryText
```

---

### 7.2. Vocabulary Authorization

**Module:** `Wordfolio.Api.DataAccess.Vocabularies`

```fsharp
let getVocabularyByIdAndUserIdAsync
    (vocabularyId: int)
    (userId: string)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<Vocabulary option>
```

**Query (with JOIN):**
```sql
SELECT v.* FROM wordfolio."Vocabularies" v
INNER JOIN wordfolio."Collections" c ON v."CollectionId" = c."Id"
WHERE v."Id" = @vocabularyId AND c."UserId" = @userId
```

---

### 7.3. Full Entry Hierarchy Retrieval

**Module:** `Wordfolio.Api.DataAccess.Entries`

```fsharp
type EntryWithHierarchy =
    { Entry: Entry
      Definitions: DefinitionWithExamples list
      Translations: TranslationWithExamples list }

and DefinitionWithExamples =
    { Definition: Definition
      Examples: Example list }

and TranslationWithExamples =
    { Translation: Translation
      Examples: Example list }

let getEntryByIdWithHierarchyAsync
    (entryId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<EntryWithHierarchy option>
```

**Implementation:** Use LEFT JOINs to get all data in single query, group in-memory:

```sql
SELECT
    e.*,
    d.Id as DefId, d.DefinitionText, d.Source as DefSource, d.DisplayOrder as DefOrder,
    t.Id as TransId, t.TranslationText, t.Source as TransSource, t.DisplayOrder as TransOrder,
    de.Id as DefExId, de.ExampleText as DefExText, de.Source as DefExSource,
    te.Id as TransExId, te.ExampleText as TransExText, te.Source as TransExSource
FROM wordfolio."Entries" e
LEFT JOIN wordfolio."Definitions" d ON d."EntryId" = e."Id"
LEFT JOIN wordfolio."Translations" t ON t."EntryId" = e."Id"
LEFT JOIN wordfolio."Examples" de ON de."DefinitionId" = d."Id"
LEFT JOIN wordfolio."Examples" te ON te."TranslationId" = t."Id"
WHERE e."Id" = @entryId
ORDER BY d."DisplayOrder", t."DisplayOrder"
```

**Note:** Process results to group definitions/translations with their examples.

---


---

## 8. Capability Implementation in AppEnv

**Location:** `Wordfolio.Api/Infrastructure/Environment.fs`

Extend `AppEnv` class to implement Entry capability interfaces:

```fsharp
// Add to DataAccess module aliases
type Entry = Wordfolio.Api.DataAccess.Entries.Entry
type EntryCreationParameters = Wordfolio.Api.DataAccess.Entries.EntryCreationParameters
type Definition = Wordfolio.Api.DataAccess.Definitions.Definition
type DefinitionCreationParameters = Wordfolio.Api.DataAccess.Definitions.DefinitionCreationParameters
type Translation = Wordfolio.Api.DataAccess.Translations.Translation
type TranslationCreationParameters = Wordfolio.Api.DataAccess.Translations.TranslationCreationParameters
type Example = Wordfolio.Api.DataAccess.Examples.Example
type ExampleCreationParameters = Wordfolio.Api.DataAccess.Examples.ExampleCreationParameters

// Add conversion functions in AppEnv class
let toEntryDomain(e: DataAccess.Entry, definitions: Definition list, translations: Translation list) : Domain.Entries.Entry =
    // Convert Entry + Definitions + Translations to domain Entry with hierarchy

let toDefinitionSource(source: DataAccess.Definitions.DefinitionSource) : Domain.Entries.DefinitionSource =
    match source with
    | DataAccess.Definitions.DefinitionSource.Api -> Domain.Entries.DefinitionSource.Api
    | DataAccess.Definitions.DefinitionSource.Manual -> Domain.Entries.DefinitionSource.Manual

let toTranslationSource(source: DataAccess.Translations.TranslationSource) : Domain.Entries.TranslationSource =
    match source with
    | DataAccess.Translations.TranslationSource.Api -> Domain.Entries.TranslationSource.Api
    | DataAccess.Translations.TranslationSource.Manual -> Domain.Entries.TranslationSource.Manual

let toExampleSource(source: DataAccess.Examples.ExampleSource) : Domain.Entries.ExampleSource =
    match source with
    | DataAccess.Examples.ExampleSource.Api -> Domain.Entries.ExampleSource.Api
    | DataAccess.Examples.ExampleSource.Custom -> Domain.Entries.ExampleSource.Custom

// Implement capability interfaces
interface IGetEntryById with
    member _.GetEntryById(EntryId id) =
        task {
            // 1. Get entry
            let! maybeEntry =
                Wordfolio.Api.DataAccess.Entries.getEntryByIdAsync
                    id connection transaction cancellationToken

            match maybeEntry with
            | None -> return None
            | Some entry ->
                // 2. Get definitions with examples
                let! definitions =
                    Wordfolio.Api.DataAccess.Definitions.getDefinitionsByEntryIdAsync
                        id connection transaction cancellationToken

                let! definitionsWithExamples =
                    definitions
                    |> List.map (fun d ->
                        task {
                            let! examples =
                                Wordfolio.Api.DataAccess.Examples.getExamplesByDefinitionIdAsync
                                    d.Id connection transaction cancellationToken
                            return (d, examples)
                        })
                    |> Task.WhenAll

                // 3. Get translations with examples
                let! translations =
                    Wordfolio.Api.DataAccess.Translations.getTranslationsByEntryIdAsync
                        id connection transaction cancellationToken

                let! translationsWithExamples =
                    translations
                    |> List.map (fun t ->
                        task {
                            let! examples =
                                Wordfolio.Api.DataAccess.Examples.getExamplesByTranslationIdAsync
                                    t.Id connection transaction cancellationToken
                            return (t, examples)
                        })
                    |> Task.WhenAll

                // 4. Convert to domain model
                return Some (toEntryDomain(entry, definitionsWithExamples, translationsWithExamples))
        }

interface IGetEntriesByVocabularyId with
    member _.GetEntriesByVocabularyId(VocabularyId vocabularyId) =
        task {
            let! entries =
                Wordfolio.Api.DataAccess.Entries.getEntriesByVocabularyIdAsync
                    vocabularyId connection transaction cancellationToken

            // For each entry, fetch definitions/translations/examples
            let! entriesWithHierarchy =
                entries
                |> List.map (fun e ->
                    // Similar to GetEntryById implementation
                    task { ... })
                |> Task.WhenAll

            return entriesWithHierarchy |> Array.toList
        }

interface IGetEntryByTextAndVocabularyId with
    member _.GetEntryByTextAndVocabularyId(VocabularyId vocabularyId, entryText) =
        task {
            let! maybeEntry =
                Wordfolio.Api.DataAccess.Entries.getEntryByTextAndVocabularyIdAsync
                    vocabularyId entryText connection transaction cancellationToken

            match maybeEntry with
            | None -> return None
            | Some entry ->
                // Fetch hierarchy like in GetEntryById
                return Some (toEntryDomain(...))
        }

interface ICreateEntry with
    member _.CreateEntry(VocabularyId vocabularyId, entryText, createdAt, definitions, translations) =
        task {
            // 1. Create entry
            let entryParams: DataAccess.EntryCreationParameters =
                { VocabularyId = vocabularyId
                  EntryText = entryText
                  CreatedAt = createdAt }

            let! entryId =
                Wordfolio.Api.DataAccess.Entries.createEntryAsync
                    entryParams connection transaction cancellationToken

            // 2. Create definitions with DisplayOrder
            let definitionParams =
                definitions
                |> List.mapi (fun index (d: Domain.Entries.DefinitionInput) ->
                    { EntryId = entryId
                      DefinitionText = d.DefinitionText
                      Source = fromDefinitionSource d.Source
                      DisplayOrder = index })

            let! definitionIds =
                Wordfolio.Api.DataAccess.Definitions.createDefinitionsAsync
                    definitionParams connection transaction cancellationToken

            // 3. Create translations with DisplayOrder
            let translationParams =
                translations
                |> List.mapi (fun index (t: Domain.Entries.TranslationInput) ->
                    { EntryId = entryId
                      TranslationText = t.TranslationText
                      Source = fromTranslationSource t.Source
                      DisplayOrder = index })

            let! translationIds =
                Wordfolio.Api.DataAccess.Translations.createTranslationsAsync
                    translationParams connection transaction cancellationToken

            // 4. Create examples for definitions
            let defExampleParams =
                definitions
                |> List.mapi (fun index d ->
                    d.Examples
                    |> List.map (fun ex ->
                        { DefinitionId = Some definitionIds.[index]
                          TranslationId = None
                          ExampleText = ex.ExampleText
                          Source = fromExampleSource ex.Source }))
                |> List.concat

            do! Wordfolio.Api.DataAccess.Examples.createExamplesAsync
                    defExampleParams connection transaction cancellationToken
                |> Task.Ignore

            // 5. Create examples for translations
            let transExampleParams =
                translations
                |> List.mapi (fun index t ->
                    t.Examples
                    |> List.map (fun ex ->
                        { DefinitionId = None
                          TranslationId = Some translationIds.[index]
                          ExampleText = ex.ExampleText
                          Source = fromExampleSource ex.Source }))
                |> List.concat

            do! Wordfolio.Api.DataAccess.Examples.createExamplesAsync
                    transExampleParams connection transaction cancellationToken
                |> Task.Ignore

            return EntryId entryId
        }

interface IGetVocabularyByIdAndUserId with
    member _.GetVocabularyByIdAndUserId(VocabularyId vocabularyId, UserId userId) =
        task {
            let! maybeVocabulary =
                Wordfolio.Api.DataAccess.Vocabularies.getVocabularyByIdAndUserIdAsync
                    vocabularyId userId connection transaction cancellationToken

            return maybeVocabulary |> Option.map toVocabularyDomain
        }
```

**Key Points:**
- Conversion functions handle data access enums → domain discriminated unions
- All operations use shared connection/transaction from AppEnv
- ICreateEntry orchestrates multiple inserts within transaction
- Hierarchy retrieval assembles Entry + Definitions + Translations + Examples

---

## 9. Conversion Between Layers

### Data Access → Domain

Convert data access enums to domain discriminated unions:

```fsharp
// In Entries capability implementation
let toDefinitionSource(source: DataAccess.Definitions.DefinitionSource) =
    match source with
    | DataAccess.Definitions.DefinitionSource.Api -> Domain.Entries.DefinitionSource.Api
    | DataAccess.Definitions.DefinitionSource.Manual -> Domain.Entries.DefinitionSource.Manual

let toTranslationSource(source: DataAccess.Translations.TranslationSource) =
    match source with
    | DataAccess.Translations.TranslationSource.Api -> Domain.Entries.TranslationSource.Api
    | DataAccess.Translations.TranslationSource.Manual -> Domain.Entries.TranslationSource.Manual

let toExampleSource(source: DataAccess.Examples.ExampleSource) =
    match source with
    | DataAccess.Examples.ExampleSource.Api -> Domain.Entries.ExampleSource.Api
    | DataAccess.Examples.ExampleSource.Custom -> Domain.Entries.ExampleSource.Custom
```

### Domain → Data Access

```fsharp
let fromDefinitionSource(source: Domain.Entries.DefinitionSource) =
    match source with
    | Domain.Entries.DefinitionSource.Api -> DataAccess.Definitions.DefinitionSource.Api
    | Domain.Entries.DefinitionSource.Manual -> DataAccess.Definitions.DefinitionSource.Manual

let fromTranslationSource(source: Domain.Entries.TranslationSource) =
    match source with
    | Domain.Entries.TranslationSource.Api -> DataAccess.Translations.TranslationSource.Api
    | Domain.Entries.TranslationSource.Manual -> DataAccess.Translations.TranslationSource.Manual

let fromExampleSource(source: Domain.Entries.ExampleSource) =
    match source with
    | Domain.Entries.ExampleSource.Api -> DataAccess.Examples.ExampleSource.Api
    | Domain.Entries.ExampleSource.Custom -> DataAccess.Examples.ExampleSource.Custom
```

---

## 10. API Handler Integration

**Location:** `Wordfolio.Api/Handlers/Entries.fs` (new module)

```fsharp
module Wordfolio.Api.Handlers.Entries

open System
open System.Security.Claims
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Routing

open Npgsql

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.Operations
open Wordfolio.Api.Infrastructure.Environment

module UrlTokens = Wordfolio.Api.Urls
module Urls = Wordfolio.Api.Urls.Entries

type ExampleRequest =
    { ExampleText: string
      Source: ExampleSource }

type DefinitionRequest =
    { DefinitionText: string
      Source: DefinitionSource
      Examples: ExampleRequest list }

type TranslationRequest =
    { TranslationText: string
      Source: TranslationSource
      Examples: ExampleRequest list }

type CreateEntryRequest =
    { VocabularyId: int
      EntryText: string
      Definitions: DefinitionRequest list
      Translations: TranslationRequest list }

type ExampleResponse =
    { Id: int
      ExampleText: string
      Source: ExampleSource }

type DefinitionResponse =
    { Id: int
      DefinitionText: string
      Source: DefinitionSource
      DisplayOrder: int
      Examples: ExampleResponse list }

type TranslationResponse =
    { Id: int
      TranslationText: string
      Source: TranslationSource
      DisplayOrder: int
      Examples: ExampleResponse list }

type EntryResponse =
    { Id: int
      VocabularyId: int
      EntryText: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      Definitions: DefinitionResponse list
      Translations: TranslationResponse list }

let private toExampleInput(req: ExampleRequest) : ExampleInput =
    { ExampleText = req.ExampleText
      Source = req.Source }

let private toDefinitionInput(req: DefinitionRequest) : DefinitionInput =
    { DefinitionText = req.DefinitionText
      Source = req.Source
      Examples = req.Examples |> List.map toExampleInput }

let private toTranslationInput(req: TranslationRequest) : TranslationInput =
    { TranslationText = req.TranslationText
      Source = req.Source
      Examples = req.Examples |> List.map toExampleInput }

let private toExampleResponse(ex: Example) : ExampleResponse =
    { Id = ExampleId.value ex.Id
      ExampleText = ex.ExampleText
      Source = ex.Source }

let private toDefinitionResponse(def: Definition) : DefinitionResponse =
    { Id = DefinitionId.value def.Id
      DefinitionText = def.DefinitionText
      Source = def.Source
      DisplayOrder = def.DisplayOrder
      Examples = def.Examples |> List.map toExampleResponse }

let private toTranslationResponse(trans: Translation) : TranslationResponse =
    { Id = TranslationId.value trans.Id
      TranslationText = trans.TranslationText
      Source = trans.Source
      DisplayOrder = trans.DisplayOrder
      Examples = trans.Examples |> List.map toExampleResponse }

let private toResponse(entry: Entry) : EntryResponse =
    { Id = EntryId.value entry.Id
      VocabularyId = VocabularyId.value entry.VocabularyId
      EntryText = entry.EntryText
      CreatedAt = entry.CreatedAt
      UpdatedAt = entry.UpdatedAt
      Definitions = entry.Definitions |> List.map toDefinitionResponse
      Translations = entry.Translations |> List.map toTranslationResponse }

let private getUserId(user: ClaimsPrincipal) : int option =
    let claim = user.FindFirst(ClaimTypes.NameIdentifier)
    match claim with
    | null -> None
    | c ->
        match Int32.TryParse(c.Value) with
        | true, id -> Some id
        | false, _ -> None

let private toErrorResponse(error: EntryError) : IResult =
    match error with
    | EntryNotFound _ -> Results.NotFound()
    | EntryTextRequired -> Results.BadRequest({| error = "Entry text is required" |})
    | EntryTextTooLong maxLength ->
        Results.BadRequest({| error = $"Entry text must be at most {maxLength} characters" |})
    | VocabularyNotFoundOrAccessDenied _ -> Results.NotFound({| error = "Vocabulary not found" |})
    | DuplicateEntry existingId ->
        Results.Conflict({| error = $"Duplicate entry exists with ID {EntryId.value existingId}" |})
    | NoDefinitionsOrTranslations ->
        Results.BadRequest({| error = "At least one definition or translation required" |})
    | TooManyExamples maxCount ->
        Results.BadRequest({| error = $"Too many examples (max {maxCount} per item)" |})
    | ExampleTextTooLong maxLength ->
        Results.BadRequest({| error = $"Example text must be at most {maxLength} characters" |})

let mapEntriesEndpoints(group: RouteGroupBuilder) =
    group.MapPost(
        UrlTokens.Root,
        Func<CreateEntryRequest, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
            (fun request user dataSource cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized()
                    | Some userId ->
                        let env = TransactionalEnv(dataSource, cancellationToken)

                        let definitions = request.Definitions |> List.map toDefinitionInput
                        let translations = request.Translations |> List.map toTranslationInput

                        let! result =
                            create
                                env
                                (UserId userId)
                                (VocabularyId request.VocabularyId)
                                request.EntryText
                                definitions
                                translations
                                DateTimeOffset.UtcNow

                        return
                            match result with
                            | Ok entry ->
                                Results.Created(
                                    Urls.entryById(EntryId.value entry.Id),
                                    toResponse entry
                                )
                            | Error error -> toErrorResponse error
                })
    )
    |> ignore

    group.MapGet(
        UrlTokens.ById,
        Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
            (fun id user dataSource cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized()
                    | Some userId ->
                        let env = TransactionalEnv(dataSource, cancellationToken)

                        let! result = getById env (UserId userId) (EntryId id)

                        return
                            match result with
                            | Ok entry -> Results.Ok(toResponse entry)
                            | Error error -> toErrorResponse error
                })
    )
    |> ignore

    group.MapGet(
        "/vocabularies/{vocabularyId}/entries",
        Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
            (fun vocabularyId user dataSource cancellationToken ->
                task {
                    match getUserId user with
                    | None -> return Results.Unauthorized()
                    | Some userId ->
                        let env = TransactionalEnv(dataSource, cancellationToken)

                        let! result = getByVocabularyId env (UserId userId) (VocabularyId vocabularyId)

                        return
                            match result with
                            | Ok entries ->
                                let response = entries |> List.map toResponse
                                Results.Ok(response)
                            | Error error -> toErrorResponse error
                })
    )
    |> ignore
```

**Key Points:**
- Request/Response types separate from domain types
- Conversion functions between layers (toDefinitionInput, toResponse, etc.)
- getUserId extracts ClaimsPrincipal (with null check and parse validation)
- TransactionalEnv created per request with NpgsqlDataSource and CancellationToken
- Error mapping to HTTP status codes with JSON error messages
- URL generation via Urls module (needs to be defined)


---

## 11. Implementation Order

1. Add ID types to `Ids.fs`
2. Create `Entries/Entry.fs` (domain types with discriminated union sources)
3. Create `Entries/Errors.fs` (error types)
4. Add missing data access queries (3 functions: duplicate check, vocab auth, hierarchy retrieval)
5. Create `Entries/Capabilities.fs` (interfaces + capability functions + input types)
6. Extend `AppEnv` in `Infrastructure/Environment.fs` to implement Entry capability interfaces
7. Create `Entries/Operations.fs` (business logic with runInTransaction)
8. Create `Handlers/Entries.fs` (request/response types, endpoint mapping)
9. Add URL definitions in `Urls.fs` for Entries
10. Register handlers in `Program.fs` with OpenAPI tags ("Entries" route group)
