namespace Wordfolio.Api.Tests.Utils.Wordfolio.Mapping

open System
open System.Collections.Generic

open Microsoft.EntityFrameworkCore

open Wordfolio.Api.DataAccess

[<CLIMutable>]
type User =
    { Id: int
      Collections: ResizeArray<Collection> }

and [<CLIMutable>] Collection =
    { Id: int
      UserId: int
      Name: string
      Description: string
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      IsSystem: bool
      User: User
      Vocabularies: ResizeArray<Vocabulary> }

and [<CLIMutable>] Vocabulary =
    { Id: int
      CollectionId: int
      Name: string
      Description: string
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      IsDefault: bool
      Collection: Collection
      Entries: ResizeArray<Entry> }

and [<CLIMutable>] Entry =
    { Id: int
      VocabularyId: int
      EntryText: string
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      Vocabulary: Vocabulary
      Definitions: ResizeArray<Definition>
      Translations: ResizeArray<Translation> }

and [<CLIMutable>] Definition =
    { Id: int
      EntryId: int
      DefinitionText: string
      Source: int16
      DisplayOrder: int
      Entry: Entry
      Examples: ResizeArray<Example> }

and [<CLIMutable>] Translation =
    { Id: int
      EntryId: int
      TranslationText: string
      Source: int16
      DisplayOrder: int
      Entry: Entry
      Examples: ResizeArray<Example> }

and [<CLIMutable>] Example =
    { Id: int
      DefinitionId: Nullable<int>
      TranslationId: Nullable<int>
      ExampleText: string
      Source: int16
      Definition: Definition
      Translation: Translation }

type internal WordfolioTestDbContext(options: DbContextOptions<WordfolioTestDbContext>) =
    inherit DbContext(options)

    member this.Users: DbSet<User> =
        base.Set<User>()

    member this.Collections: DbSet<Collection> =
        base.Set<Collection>()

    member this.Vocabularies: DbSet<Vocabulary> =
        base.Set<Vocabulary>()

    member this.Entries: DbSet<Entry> =
        base.Set<Entry>()

    member this.Definitions: DbSet<Definition> =
        base.Set<Definition>()

    member this.Translations: DbSet<Translation> =
        base.Set<Translation>()

    member this.Examples: DbSet<Example> =
        base.Set<Example>()

    override _.OnModelCreating(modelBuilder: ModelBuilder) =
        let users = modelBuilder.Entity<User>()

        users.ToTable(Schema.UsersTable.Name, Schema.Name).HasKey(fun x -> x.Id :> obj)
        |> ignore

        users.Property(_.Id).ValueGeneratedNever()
        |> ignore

        users
            .HasMany(fun u -> u.Collections :> IEnumerable<Collection>)
            .WithOne(fun c -> c.User)
            .HasForeignKey(fun c -> c.UserId :> obj)
        |> ignore

        let collections =
            modelBuilder.Entity<Collection>()

        collections
            .ToTable(Schema.CollectionsTable.Name, Schema.Name)
            .HasKey(fun x -> x.Id :> obj)
        |> ignore

        collections.Property(_.Id).ValueGeneratedOnAdd()
        |> ignore

        collections.Property(_.UserId).IsRequired()
        |> ignore

        collections
            .HasMany(fun c -> c.Vocabularies :> IEnumerable<Vocabulary>)
            .WithOne(fun v -> v.Collection)
            .HasForeignKey(fun v -> v.CollectionId :> obj)
        |> ignore

        collections.Property(_.Name).IsRequired().HasMaxLength(255)
        |> ignore

        collections.Property(_.Description).IsRequired(false)
        |> ignore

        collections.Property(_.CreatedAt).IsRequired()
        |> ignore

        collections.Property(_.UpdatedAt).IsRequired(false)
        |> ignore

        collections.Property(_.IsSystem).IsRequired()
        |> ignore

        let vocabularies =
            modelBuilder.Entity<Vocabulary>()

        vocabularies
            .ToTable(Schema.VocabulariesTable.Name, Schema.Name)
            .HasKey(fun x -> x.Id :> obj)
        |> ignore

        vocabularies.Property(_.Id).ValueGeneratedOnAdd()
        |> ignore

        vocabularies.Property(_.CollectionId).IsRequired()
        |> ignore

        vocabularies.Property(_.Name).IsRequired().HasMaxLength(255)
        |> ignore

        vocabularies.Property(_.Description).IsRequired(false)
        |> ignore

        vocabularies.Property(_.CreatedAt).IsRequired()
        |> ignore

        vocabularies.Property(_.UpdatedAt).IsRequired(false)
        |> ignore

        vocabularies.Property(_.IsDefault).IsRequired()
        |> ignore

        vocabularies
            .HasMany(fun v -> v.Entries :> IEnumerable<Entry>)
            .WithOne(fun e -> e.Vocabulary)
            .HasForeignKey(fun e -> e.VocabularyId :> obj)
        |> ignore

        let entries = modelBuilder.Entity<Entry>()

        entries
            .ToTable(Schema.EntriesTable.Name, Schema.Name)
            .HasKey(fun x -> x.Id :> obj)
        |> ignore

        entries.Property(_.Id).ValueGeneratedOnAdd()
        |> ignore

        entries.Property(_.VocabularyId).IsRequired()
        |> ignore

        entries.Property(_.EntryText).IsRequired().HasMaxLength(255)
        |> ignore

        entries.Property(_.CreatedAt).IsRequired()
        |> ignore

        entries.Property(_.UpdatedAt).IsRequired(false)
        |> ignore

        entries
            .HasMany(fun e -> e.Definitions :> IEnumerable<Definition>)
            .WithOne(fun d -> d.Entry)
            .HasForeignKey(fun d -> d.EntryId :> obj)
        |> ignore

        entries
            .HasMany(fun e -> e.Translations :> IEnumerable<Translation>)
            .WithOne(fun t -> t.Entry)
            .HasForeignKey(fun t -> t.EntryId :> obj)
        |> ignore

        let definitions =
            modelBuilder.Entity<Definition>()

        definitions
            .ToTable(Schema.DefinitionsTable.Name, Schema.Name)
            .HasKey(fun x -> x.Id :> obj)
        |> ignore

        definitions.Property(_.Id).ValueGeneratedOnAdd()
        |> ignore

        definitions.Property(_.EntryId).IsRequired()
        |> ignore

        definitions.Property(_.DefinitionText).IsRequired().HasMaxLength(255)
        |> ignore

        definitions.Property(_.Source).IsRequired()
        |> ignore

        definitions.Property(_.DisplayOrder).IsRequired()
        |> ignore

        let translations =
            modelBuilder.Entity<Translation>()

        translations
            .ToTable(Schema.TranslationsTable.Name, Schema.Name)
            .HasKey(fun x -> x.Id :> obj)
        |> ignore

        translations.Property(_.Id).ValueGeneratedOnAdd()
        |> ignore

        translations.Property(_.EntryId).IsRequired()
        |> ignore

        translations.Property(_.TranslationText).IsRequired().HasMaxLength(255)
        |> ignore

        translations.Property(_.Source).IsRequired()
        |> ignore

        translations.Property(_.DisplayOrder).IsRequired()
        |> ignore

        let examples =
            modelBuilder.Entity<Example>()

        examples
            .ToTable(Schema.ExamplesTable.Name, Schema.Name)
            .HasKey(fun x -> x.Id :> obj)
        |> ignore

        examples.Property(_.Id).ValueGeneratedOnAdd()
        |> ignore

        examples.Property(_.DefinitionId).IsRequired(false)
        |> ignore

        examples.Property(_.TranslationId).IsRequired(false)
        |> ignore

        examples.Property(_.ExampleText).IsRequired().HasMaxLength(500)
        |> ignore

        examples.Property(_.Source).IsRequired()
        |> ignore

        definitions
            .HasMany(fun d -> d.Examples :> IEnumerable<Example>)
            .WithOne(fun e -> e.Definition)
            .HasForeignKey(fun e -> e.DefinitionId :> obj)
            .IsRequired(false)
        |> ignore

        translations
            .HasMany(fun t -> t.Examples :> IEnumerable<Example>)
            .WithOne(fun e -> e.Translation)
            .HasForeignKey(fun e -> e.TranslationId :> obj)
            .IsRequired(false)
        |> ignore

        base.OnModelCreating(modelBuilder)
