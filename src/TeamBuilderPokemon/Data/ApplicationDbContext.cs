using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeamBuilderPokemon.Models;

namespace TeamBuilderPokemon.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pokemon> Pokemons => Set<Pokemon>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<TeamSlot> TeamSlots => Set<TeamSlot>();

    public DbSet<TeamRevision> TeamRevisions => Set<TeamRevision>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Primeiro: e' ele que mapeia as tabelas do Identity (AspNetUsers e companhia).
        base.OnModelCreating(builder);

        // Nome unico: a importacao da PokeAPI nao pode duplicar especie.
        builder.Entity<Pokemon>()
            .HasIndex(p => p.Name)
            .IsUnique();

        builder.Entity<TeamSlot>()
            .HasIndex(s => new { s.TeamId, s.SlotNumber })
            .IsUnique();

        builder.Entity<TeamSlot>()
            .HasOne(s => s.Team)
            .WithMany(t => t.Slots)
            .HasForeignKey(s => s.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TeamSlot>()
            .HasOne(s => s.Pokemon)
            .WithMany()
            .HasForeignKey(s => s.PokemonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TeamSlot>()
            .Property(s => s.Nickname)
            .HasMaxLength(TeamSlot.NicknameMaxLength);

        builder.Entity<TeamRevision>()
            .HasOne(r => r.Team)
            .WithMany(t => t.Revisions)
            .HasForeignKey(r => r.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Pokemon>().HasData(PokemonSeedData.All);
    }
}
