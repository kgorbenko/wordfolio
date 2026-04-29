module Wordfolio.Api.Api.Exercises.Mappers

open System.Text.Json

open Wordfolio.Api.Api.Exercises.Types
open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Exercises
open Wordfolio.Api.Infrastructure.ResourceIdEncoder

let private parsePromptData(PromptData jsonStr) : JsonElement =
    use doc = JsonDocument.Parse(jsonStr)
    doc.RootElement.Clone()

let toExerciseTypeDto(exerciseType: ExerciseType) : ExerciseTypeDto =
    match exerciseType with
    | MultipleChoice -> ExerciseTypeDto.MultipleChoice
    | Translation -> ExerciseTypeDto.Translation

let toExerciseTypeDomain(dto: ExerciseTypeDto) : ExerciseType =
    match dto with
    | ExerciseTypeDto.MultipleChoice -> MultipleChoice
    | ExerciseTypeDto.Translation -> Translation
    | x -> failwith $"Unknown ExerciseTypeDto value: {x}"

let private toAttemptSummaryResponse(summary: AttemptSummary) : AttemptSummaryResponse =
    { RawAnswer = RawAnswer.value summary.RawAnswer
      IsCorrect = summary.IsCorrect
      AttemptedAt = summary.AttemptedAt }

let private toSessionBundleEntryResponse
    (encoder: IResourceIdEncoder)
    (entry: SessionBundleEntry)
    : SessionBundleEntryResponse =
    { EntryId = encoder.Encode(EntryId.value entry.EntryId)
      DisplayOrder = entry.DisplayOrder
      PromptData = parsePromptData entry.PromptData
      Attempt =
        entry.Attempt
        |> Option.map toAttemptSummaryResponse }

let toSessionBundleResponse (encoder: IResourceIdEncoder) (bundle: SessionBundle) : SessionBundleResponse =
    { SessionId = encoder.Encode(ExerciseSessionId.value bundle.SessionId)
      ExerciseType = toExerciseTypeDto bundle.ExerciseType
      Entries =
        bundle.Entries
        |> List.map(toSessionBundleEntryResponse encoder) }

let private tryMapWorstKnownScope
    (encoder: IResourceIdEncoder)
    (scopeRequest: WorstKnownScopeRequest option)
    : Result<WorstKnownScope, string> =
    match scopeRequest with
    | None -> Ok AllUserEntries
    | Some(scope: WorstKnownScopeRequest) ->
        match scope.Type with
        | "allUserEntries" -> Ok AllUserEntries
        | "vocabulary" ->
            match
                scope.VocabularyId
                |> Option.bind encoder.Decode
            with
            | None -> Error "Invalid or missing vocabularyId in worstKnown scope"
            | Some id -> Ok(WithinVocabulary(VocabularyId id))
        | "collection" ->
            match
                scope.CollectionId
                |> Option.bind encoder.Decode
            with
            | None -> Error "Invalid or missing collectionId in worstKnown scope"
            | Some id -> Ok(WithinCollection(CollectionId id))
        | t -> Error $"Unknown worstKnown scope type: {t}"

let tryMapSelector (encoder: IResourceIdEncoder) (request: EntrySelectorRequest) : Result<EntrySelector, string> =
    match request.Type with
    | "vocabulary" ->
        match
            request.VocabularyId
            |> Option.bind encoder.Decode
        with
        | None -> Error "Invalid or missing vocabularyId"
        | Some id -> Ok(VocabularyScope(VocabularyId id))
    | "collection" ->
        match
            request.CollectionId
            |> Option.bind encoder.Decode
        with
        | None -> Error "Invalid or missing collectionId"
        | Some id -> Ok(CollectionScope(CollectionId id))
    | "explicitEntries" ->
        let entryIds =
            request.EntryIds
            |> Option.defaultValue [||]

        let decoded =
            entryIds |> Array.map encoder.Decode

        if decoded |> Array.forall Option.isSome then
            Ok(
                ExplicitEntries(
                    decoded
                    |> Array.choose id
                    |> Array.distinct
                    |> Array.map EntryId
                    |> Array.toList
                )
            )
        else
            Error "One or more entry IDs are invalid"
    | "worstKnown" ->
        match request.Count with
        | None -> Error "Count is required for worstKnown selector"
        | Some count ->
            match tryMapWorstKnownScope encoder request.Scope with
            | Error e -> Error e
            | Ok scope -> Ok(WorstKnown(scope, count))
    | t -> Error $"Unknown selector type: {t}"
