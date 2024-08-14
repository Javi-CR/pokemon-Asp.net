using System.ComponentModel.DataAnnotations;

namespace pokemon.Models
{
    public class EquipoViewModel
    {
        public string NombreEquipo { get; set; }
        public List<PokemonViewModel> Pokemons { get; set; }
    }

    public class PokemonViewModel
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int Life { get; set; }
    }
}
