module Wordfolio.Common.Task

open System.Threading.Tasks

let ignore(t: Task<'a>) : Task = t :> Task
