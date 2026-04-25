module Wordfolio.Api.Domain.Exercises.Dispatch

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Exercises

let generatePrompt (exerciseType: ExerciseType) (entry: Entry) : GeneratedPrompt =
    match exerciseType with
    | ExerciseType.MultipleChoice -> MultipleChoice.generatePrompt entry
    | ExerciseType.Translation -> Translation.generatePrompt entry

let evaluate
    (exerciseType: ExerciseType)
    (promptSchemaVersion: int16)
    (promptData: PromptData)
    (rawAnswer: RawAnswer)
    : Result<bool, EvaluateError> =
    match exerciseType with
    | ExerciseType.MultipleChoice -> MultipleChoice.evaluate promptSchemaVersion promptData rawAnswer
    | ExerciseType.Translation -> Translation.evaluate promptSchemaVersion promptData rawAnswer
