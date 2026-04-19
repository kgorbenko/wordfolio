module Wordfolio.Api.Configuration.ExerciseSessionPurge

open System

[<CLIMutable>]
type ExerciseSessionPurgeConfiguration =
    { Enabled: bool
      RetentionPeriod: TimeSpan
      Interval: TimeSpan }
