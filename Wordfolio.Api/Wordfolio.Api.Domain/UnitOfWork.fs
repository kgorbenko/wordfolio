namespace Wordfolio.Api.Domain

open System
open System.Threading
open System.Threading.Tasks

type IUnitOfWork =
    inherit IAsyncDisposable
    abstract CollectionRepository: ICollectionRepository
    abstract VocabularyRepository: IVocabularyRepository
    abstract CommitAsync: CancellationToken -> Task

type IUnitOfWorkFactory =
    abstract CreateAsync: CancellationToken -> Task<IUnitOfWork>
