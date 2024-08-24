document.addEventListener("DOMContentLoaded", function () {
    const URL = "https://pokeapi.co/api/v2/pokemon/";
    const equipoDiv = document.getElementById('equipo');
    const modal = document.getElementById('modalSeleccion');
    const closeModal = document.querySelector('#modalSeleccion .close');
    const listaPokemonFiltro = document.getElementById('listaPokemonFiltro');
    const btnCrearEquipo = document.getElementById('btnCrearEquipo');
    const nombreEquipoInput = document.getElementById('nombreEquipo');
    const nombreEquipoContainer = document.getElementById('nombreEquipoContainer');

    document.getElementById('btnAgregarPokemon').addEventListener('click', function () {
        modal.style.display = 'block';
        cargarPokemon();
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
        cargarPokemon();
    });

    document.querySelectorAll('.nav-item button').forEach(button => {
        button.addEventListener('click', function () {
            const tipo = this.id.replace('-filtro', '');
            cargarPokemon(tipo);
        });
    });

    function cargarPokemon(tipo = '') {
        listaPokemonFiltro.innerHTML = '';
        for (let i = 1; i <= 151; i++) {
            fetch(URL + i)
                .then(response => response.json())
                .then(data => {
                    if (tipo === '' || data.types.some(t => t.type.name === tipo)) {
                        const pokemonDiv = document.createElement('div');
                        pokemonDiv.className = 'pokemon-card';
                        pokemonDiv.innerHTML = `
                            <img src="${data.sprites.front_default}" alt="${data.name}" />
                            <h3>${data.name}</h3>
                            <button onclick="seleccionarPokemon('${data.name}', '${data.sprites.front_default}', ${data.stats[0].base_stat})">Seleccionar</button>
                        `;
                        listaPokemonFiltro.appendChild(pokemonDiv);
                    }
                });
        }
    }

    window.seleccionarPokemon = function (name, imageUrl, life) {
        if (equipoDiv.children.length >= 5) {
            alert('No puedes agregar más de 5 Pokémon al equipo.');
            return;
        }

        const pokemonDiv = document.createElement('div');
        pokemonDiv.className = 'pokemon-item';
        pokemonDiv.innerHTML = `
            <img src="${imageUrl}" alt="${name}" />
            <h4>${name}</h4>
            <button onclick="eliminarPokemon(this)">Eliminar</button>
        `;
        equipoDiv.appendChild(pokemonDiv);
        modal.style.display = 'none';

        const pokemon = {
            Name: name,
            ImageUrl: imageUrl,
            Life: life
        };

        // Guardar en localStorage
        const pokemons = JSON.parse(localStorage.getItem('pokemons')) || [];
        if (!pokemons.some(p => p.Name === name)) {  // Verificar si ya está en la lista
            pokemons.push(pokemon);
            localStorage.setItem('pokemons', JSON.stringify(pokemons));
        }

        nombreEquipoContainer.style.display = 'block';
        nombreEquipoInput.addEventListener('input', function () {
            btnCrearEquipo.disabled = !this.value.trim();
        });

        if (equipoDiv.children.length >= 5) {
            btnCrearEquipo.style.display = 'block';
        }
    };

    window.eliminarPokemon = function (button) {
        button.parentElement.remove();
        const pokemons = JSON.parse(localStorage.getItem('pokemons')) || [];
        const nameToRemove = button.previousElementSibling.textContent;
        const updatedPokemons = pokemons.filter(p => p.Name !== nameToRemove);
        localStorage.setItem('pokemons', JSON.stringify(updatedPokemons));

        if (equipoDiv.children.length < 5) {
            btnCrearEquipo.style.display = 'none';
        }

        if (equipoDiv.children.length === 0) {
            nombreEquipoContainer.style.display = 'none';
            btnCrearEquipo.style.display = 'none';
        }
    };

    btnCrearEquipo.addEventListener('click', function () {
        const pokemons = JSON.parse(localStorage.getItem('pokemons')) || [];
        const nombreEquipo = nombreEquipoInput.value;

        // Enviar datos al servidor
        fetch('/Equipo/Crear', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                NombreEquipo: nombreEquipo,
                Pokemons: pokemons
            })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('Equipo creado con éxito!');
                    localStorage.removeItem('pokemons');  // Limpiar localStorage
                    window.location.reload();
                } else {
                    alert('Error al crear el equipo: ' + (data.message || 'Error desconocido.'));
                }
            });
    });
});
