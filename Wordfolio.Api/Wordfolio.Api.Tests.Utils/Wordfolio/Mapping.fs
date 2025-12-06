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
      User: User
      Vocabularies: ResizeArray<Vocabulary> }

and [<CLIMutable>] Vocabulary =
    { Id: int
      CollectionId: int
      Name: string
      Description: string
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      Collection: Collection }

type internal WordfolioTestDbContext(options: DbContextOptions<WordfolioTestDbContext>) =
    inherit DbContext(options)

    member this.Users: DbSet<User> =
        base.Set<User>()

    member this.Collections: DbSet<Collection> =
        base.Set<Collection>()

    member this.Vocabularies: DbSet<Vocabulary> =
        base.Set<Vocabulary>()

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

        base.OnModelCreating(modelBuilder)
