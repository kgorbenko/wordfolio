module Wordfolio.Api.Domain.Tests.Exercises.DispatchTests

open System

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Exercises

let timestamp =
    DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

let makeTranslation id text =
    { Id = TranslationId id
      TranslationText = text
      Source = TranslationSource.Manual
      DisplayOrder = 0
      Examples = [] }

let makeEntry entryText translations =
    { Id = EntryId 1
      VocabularyId = VocabularyId 1
      EntryText = entryText
      CreatedAt = timestamp
      UpdatedAt = timestamp
      Definitions = []
      Translations = translations }

[<Fact>]
let ``generatePrompt routes to MultipleChoice for MultipleChoice exercise type``() =
    let entry =
        makeEntry "cat" [ makeTranslation 1 "apple" ]

    let expected =
        MultipleChoice.generatePrompt entry

    let actual =
        Dispatch.generatePrompt ExerciseType.MultipleChoice entry

    Assert.Equal(expected, actual)

[<Fact>]
let ``generatePrompt routes to Translation for Translation exercise type``() =
    let entry =
        makeEntry "cat" [ makeTranslation 1 "apple" ]

    let expected =
        Translation.generatePrompt entry

    let actual =
        Dispatch.generatePrompt ExerciseType.Translation entry

    Assert.Equal(expected, actual)

[<Fact>]
let ``evaluate routes to MultipleChoice for MultipleChoice exercise type``() =
    let entry =
        makeEntry "cat" [ makeTranslation 1 "apple" ]

    let prompt =
        MultipleChoice.generatePrompt entry

    let rawAnswer = RawAnswer "a"

    let expected =
        MultipleChoice.evaluate 1s prompt.PromptData rawAnswer

    let actual =
        Dispatch.evaluate ExerciseType.MultipleChoice 1s prompt.PromptData rawAnswer

    Assert.Equal(expected, actual)

[<Fact>]
let ``evaluate routes to Translation for Translation exercise type``() =
    let entry =
        makeEntry "cat" [ makeTranslation 1 "apple" ]

    let prompt =
        Translation.generatePrompt entry

    let rawAnswer = RawAnswer "apple"

    let expected =
        Translation.evaluate 1s prompt.PromptData rawAnswer

    let actual =
        Dispatch.evaluate ExerciseType.Translation 1s prompt.PromptData rawAnswer

    Assert.Equal(expected, actual)
