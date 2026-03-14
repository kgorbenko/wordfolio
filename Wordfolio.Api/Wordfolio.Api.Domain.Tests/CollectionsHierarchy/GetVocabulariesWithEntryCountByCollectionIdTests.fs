module Wordfolio.Api.Domain.Tests.CollectionsHierarchy.GetVocabulariesWithEntryCountByCollectionIdTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.CollectionsHierarchy
open Wordfolio.Api.Domain.CollectionsHierarchy.Operations

type TestEnv(getVocabulariesWithEntryCountByCollectionId: UserId -> CollectionId -> Task<VocabularyWithEntryCount list>)
    =
    let getVocabulariesWithEntryCountByCollectionIdCalls =
        ResizeArray<UserId * CollectionId>()

    member _.GetVocabulariesWithEntryCountByCollectionIdCalls =
        getVocabulariesWithEntryCountByCollectionIdCalls
        |> Seq.toList

    interface IGetVocabulariesWithEntryCountByCollectionId with
        member _.GetVocabulariesWithEntryCountByCollectionId userId collectionId =
            getVocabulariesWithEntryCountByCollectionIdCalls.Add((userId, collectionId))
            getVocabulariesWithEntryCountByCollectionId userId collectionId

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let now =
    DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

let makeVocabulary vocabularyId name entryCount =
    { Id = VocabularyId vocabularyId
      Name = name
      Description = None
      CreatedAt = now
      UpdatedAt = None
      EntryCount = entryCount }

[<Fact>]
let ``returns Ok vocabularies and passes correct userId and collectionId to capability``() =
    task {
        let vocabularies =
            [ makeVocabulary 10 "Vocabulary 1" 3; makeVocabulary 11 "Vocabulary 2" 1 ]

        let env =
            TestEnv(fun userId collectionId ->
                if userId <> UserId 1 then
                    failwith $"Unexpected userId: {userId}"

                if collectionId <> CollectionId 5 then
                    failwith $"Unexpected collectionId: {collectionId}"

                Task.FromResult(vocabularies))

        let! result =
            getVocabulariesWithEntryCountByCollectionId
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5 }

        Assert.Equal(Ok vocabularies, result)

        Assert.Equal<(UserId * CollectionId) list>(
            [ (UserId 1, CollectionId 5) ],
            env.GetVocabulariesWithEntryCountByCollectionIdCalls
        )
    }
