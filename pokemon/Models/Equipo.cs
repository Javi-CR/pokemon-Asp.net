using pokemon.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace pokemon.Models
{
    public class Equipo
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string UsuarioId { get; set; }
        public ICollection<Pokemon> Pokemons { get; set; } 
    }
}