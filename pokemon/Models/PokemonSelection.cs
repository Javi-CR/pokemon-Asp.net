using System.ComponentModel.DataAnnotations;

namespace pokemon.Models
{
    public class PokemonSelection
    {

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int Life { get; set; }
        public string UserId { get; set; } // Para identificar qué usuario seleccionó el Pokémon

    }
}
