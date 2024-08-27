namespace pokemon.Models
{
    public class RetosViewModel
    {
        public Equipo CurrentUserTeam { get; set; } // Equipo del usuario actual
        public List<EntrenadorViewModel> Entrenadores { get; set; } // Lista de entrenadores
        public bool TodosPokemonsConVida0 { get; set; } // Indica si todos los Pokémon del equipo del usuario logueado tienen 0 de vida
    }

    public class EntrenadorViewModel
    {
        public string UsuarioId { get; set; } // ID del entrenador
        public string UserName { get; set; } // Nombre de usuario del entrenador
        public bool HasTeam { get; set; } // Indicador de si el entrenador tiene un equipo
        public string EquipoNombre { get; set; } // Nombre del equipo
        public bool TodosPokemonsConVida0 { get; set; } // Indica si todos los Pokémon del equipo del entrenador tienen 0 de vida
    }
}
