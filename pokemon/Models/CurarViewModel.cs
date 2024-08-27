public class CurarViewModel
{
    public string EntrenadorNombre { get; set; } // Nombre del entrenador
    public int EquipoId { get; set; } // ID del equipo
    public string EquipoNombre { get; set; } // Nombre del equipo
    public List<PokemonViewModel> Pokemons { get; set; } // Lista de Pokémon con vida en 0
}

public class PokemonViewModel
{
    public int PokemonId { get; set; } // ID del Pokémon
    public string Nombre { get; set; } // Nombre del Pokémon
    public string ImagenUrl { get; set; } // URL de la imagen del Pokémon
    public int Vida { get; set; } // Vida del Pokémon
}
