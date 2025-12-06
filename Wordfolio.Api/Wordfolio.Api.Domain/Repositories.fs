namespace Wordfolio.Api.Domain

open System
open System.Threading
open System.Threading.Tasks

type CollectionData =
    { Id: CollectionId
      UserId: UserId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type VocabularyData =
    { Id: VocabularyId
      CollectionId: CollectionId
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type ICollectionRepository =
    abstract GetByIdAsync: CollectionId * CancellationToken -> Task<CollectionData option>
    abstract GetByUserIdAsync: UserId * CancellationToken -> Task<CollectionData list>
    abstract CreateAsync: UserId * string * string option * DateTimeOffset * CancellationToken -> Task<CollectionData>
    abstract UpdateAsync: CollectionId * string * string option * DateTimeOffset * CancellationToken -> Task<bool>
    abstract DeleteAsync: CollectionId * CancellationToken -> Task<bool>

type IVocabularyRepository =
    abstract GetByIdAsync: VocabularyId * CancellationToken -> Task<VocabularyData option>
    abstract GetByCollectionIdAsync: CollectionId * CancellationToken -> Task<VocabularyData list>
    abstract CreateAsync: CollectionId * string * string option * DateTimeOffset * CancellationToken -> Task<VocabularyData>
    abstract UpdateAsync: VocabularyId * string * string option * DateTimeOffset * CancellationToken -> Task<bool>
    abstract DeleteAsync: VocabularyId * CancellationToken -> Task<bool>
