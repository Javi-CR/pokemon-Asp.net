document.addEventListener("DOMContentLoaded", function () {
    const URL = "https://pokeapi.co/api/v2/pokemon/";
    const equipoDiv = document.getElementById('equipo');
    const modal = document.getElementById('modalSeleccion');
    const closeModal = document.querySelector('#modalSeleccion .close');
    const listaPokemonFiltro = document.getElementById('listaPokemonFiltro');

    document.getElementById('btnAgregarPokemon').addEventListener('click', function () {
        modal.style.display = 'block';
        cargarPokemon(); // Carga todos los Pokémon al abrir el modal
    });

    closeModal.addEventListener('click', function () {
        modal.style.display = 'none';
    });

    window.addEventListener('click', function (event) {
        if (event.target === modal) {
            modal.style.display = 'none';
        }
    });

    document.getElementById('ver-todos-filtro').addEventListener('click', function () {
        cargarPokemon(); // Muestra todos los Pokémon
    });

    document.querySelectorAll('.nav-item button').forEach(button => {
        button.addEventListener('click', function () {
            const tipo = this.id.replace('-filtro', '');
            cargarPokemon(tipo); // Filtra por tipo
        });
    });

    function cargarPokemon(tipo = '') {
        listaPokemonFiltro.innerHTML = '';
        for (let i = 1; i <= 151; i++) {
            fetch(URL + i)
                .then(response => response.json())
                .then(data => {
                    if (tipo === '' || data.types.some(t => t.type.name === tipo)) {
                        mostrarPokemonFiltro(data);
                    }
                });
        }
    }

    function mostrarPokemonFiltro(poke) {
        let tipos = poke.types.map(type => `<p class="${type.type.name} tipo">${type.type.name}</p>`).join('');
        let pokeId = poke.id.toString().padStart(3, '0');
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
                <button class="btn btn-select-pokemon" data-name="${poke.name}" data-image="${poke.sprites.other["official-artwork"].front_default}" data-life="${poke.stats.find(stat => stat.stat.name === "hp").base_stat}">Seleccionar</button>
            </div>
        `;

        div.querySelector(".btn-select-pokemon").addEventListener("click", function () {
            agregarPokemonAEquipo(this.dataset.name, this.dataset.image, this.dataset.life);
        });

        listaPokemonFiltro.appendChild(div);
    }

    function agregarPokemonAEquipo(name, imageUrl, life) {
        if (equipoDiv.children.length < 5) {
            const div = document.createElement("div");
            div.classList.add("pokemon-equipo");
            div.innerHTML = `
                <h3>${name}</h3>
                <img src="${imageUrl}" alt="${name}" />
                <p>Vida: ${life}</p>
                <button class="btn btn-remove-pokemon">Eliminar</button>
            `;

            div.querySelector(".btn-remove-pokemon").addEventListener("click", function () {
                equipoDiv.removeChild(div);
            });

            equipoDiv.appendChild(div);
            modal.style.display = 'none';
        } else {
            alert('Tu equipo ya está completo. Puedes eliminar un Pokémon antes de agregar uno nuevo.');
        }
    }
});
