namespace Wordfolio.Api.Domain

open System.Threading.Tasks

type ITransactional<'env> =
    abstract RunInTransaction<'a, 'err> : ('env -> Task<Result<'a, 'err>>) -> Task<Result<'a, 'err>>

module Transactions =
    let runInTransaction (env: #ITransactional<'env>) operation = env.RunInTransaction(operation)
