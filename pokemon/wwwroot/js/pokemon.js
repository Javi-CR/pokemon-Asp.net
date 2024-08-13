document.addEventListener("DOMContentLoaded", function () {
    const listaPokemon = document.querySelector("#listaPokemon");
    const URL = "https://pokeapi.co/api/v2/pokemon/";

    for (let i = 1; i <= 151; i++) {
        fetch(URL + i)
            .then(response => response.json())
            .then(data => {
                obtenerEvoluciones(data);
            });
    }

    function obtenerEvoluciones(poke) {
        fetch(poke.species.url)
            .then(response => response.json())
            .then(speciesData => {
                return fetch(speciesData.evolution_chain.url)
            })
            .then(response => response.json())
            .then(evolutionData => {
                let evoluciones = extraerEvoluciones(evolutionData.chain);
                obtenerDebilidadesYMostrarPokemon(poke, evoluciones);
            });
    }

    function extraerEvoluciones(chain) {
        let evoluciones = [];
        let actual = chain;

        do {
            evoluciones.push(actual.species.name);
            actual = actual.evolves_to[0];
        } while (actual && actual.hasOwnProperty('evolves_to'));

        return evoluciones;
    }

    function obtenerDebilidadesYMostrarPokemon(poke, evoluciones) {
        let debilidades = [];

        let tipoPromesas = poke.types.map((type) => {
            return fetch(type.type.url)
                .then(response => response.json())
                .then(typeData => {
                    let debilidadesTipo = typeData.damage_relations.double_damage_from.map(weakType => weakType.name);
                    debilidades = debilidades.concat(debilidadesTipo);
                });
        });

        Promise.all(tipoPromesas).then(() => {
            debilidades = [...new Set(debilidades)];
            mostrarPokemon(poke, debilidades, evoluciones);
        });
    }

    function mostrarPokemon(poke, debilidades, evoluciones) {
        let tipos = poke.types.map(type => `<p class="${type.type.name} tipo">${type.type.name}</p>`);
        tipos = tipos.join('');

        let pokeId = poke.id.toString().padStart(3, '0');

        let vida = poke.stats.find(stat => stat.stat.name === "hp").base_stat;

        const div = document.createElement("div");
        div.classList.add("pokemon");
        div.innerHTML = `
            <p class="pokemon-id-back">#${pokeId}</p>
            <div class="pokemon-imagen">
                <img src="${poke.sprites.other["official-artwork"].front_default}" alt="${poke.name}">
            </div>
            <div class="pokemon-info">
                <div class="nombre-contenedor">
                    <p class="pokemon-id">#${pokeId}</p>
                    <h2 class="pokemon-nombre">${poke.name}</h2>
                </div>
                <div class="pokemon-tipos">
                    ${tipos}
                </div>
                <div class="pokemon-stats">
                    <p class="stat">Vida: ${vida}</p>
                    <p class="stat">Debilidades: ${debilidades.join(', ')}</p>
                    <p class="stat">Evoluciones: ${evoluciones.join(' ➜ ')}</p>
                </div>
                <button class="btn btn-select-pokemon">Seleccionar</button>
            </div>
        `;

        // Agrega el evento para seleccionar el Pokémon
        div.querySelector(".btn-select-pokemon").addEventListener("click", function () {
            onPokemonSelected(poke.name, poke.sprites.other["official-artwork"].front_default, vida);
        });

        listaPokemon.append(div);
    }

    function onPokemonSelected(name, imageUrl, life) {
        document.getElementById('pokemonName').value = name;
        document.getElementById('pokemonImageUrl').value = imageUrl;
        document.getElementById('pokemonLife').value = life;

        const selectionDiv = document.getElementById('pokemon-selection');
        selectionDiv.innerHTML = `
            <h3>${name}</h3>
            <img src="${imageUrl}" alt="${name}" />
            <p>Vida: ${life}</p>
        `;
    }
});
