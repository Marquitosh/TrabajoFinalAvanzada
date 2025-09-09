document.addEventListener('DOMContentLoaded', () => {
    const listaRoles = document.getElementById('lista-roles');
    const roles = [
        { nombre: 'Administrador' },
        { nombre: 'Cliente' },
        { nombre: 'Empleado' }
    ];

    function renderizarRoles() {
        listaRoles.innerHTML = '';
        roles.forEach(rol => {
            const li = document.createElement('li');
            li.className = 'rol-item';
            li.innerHTML = `
                <span class="rol-nombre">${rol.nombre}</span>
                <div class="rol-acciones">
                    <button class="btn-editar">Editar</button>
                    <button class="btn-eliminar">Eliminar</button>
                </div>
            `;
            listaRoles.appendChild(li);
        });
    }

    renderizarRoles();

    const btnCrearRol = document.getElementById('btn-crear-rol');
    btnCrearRol.addEventListener('click', () => {
        alert('Redirigiendo a la página de Nuevo Rol...');
        
    });
});