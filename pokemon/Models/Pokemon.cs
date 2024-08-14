using pokemon.Models;
using System.ComponentModel.DataAnnotations;

namespace pokemon.Models 
{
    public class Pokemon
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string ImagenUrl { get; set; }
        public int Vida { get; set; }
        public int EquipoId { get; set; }
        public Equipo Equipo { get; set; } 
    }
}
