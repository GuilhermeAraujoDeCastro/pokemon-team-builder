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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Precisa vir primeiro: e' o OnModelCreating da propria IdentityDbContext
        // que mapeia AspNetUsers, AspNetRoles e companhia.
        base.OnModelCreating(builder);

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

        builder.Entity<Pokemon>().HasData(PokemonSeedData.All);
    }
}
