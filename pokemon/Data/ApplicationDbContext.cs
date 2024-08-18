using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using pokemon.Models;

namespace pokemon.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<PokemonSelection> PokemonSelections { get; set; }

        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<Pokemon> Pokemons { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Equipo>()
                .HasMany(e => e.Pokemons)
                .WithOne(p => p.Equipo)
                .HasForeignKey(p => p.EquipoId);
        }
    }
}
