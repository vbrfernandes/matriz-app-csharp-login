// Este script serve para gerenciar a interface baseada no login
document.addEventListener('DOMContentLoaded', () => {
    const nomeDoUsuario = localStorage.getItem('usuarioNome');
    const spanNome = document.querySelector('.user-name');

    if (nomeDoUsuario && spanNome) {
        // Troca o texto "Meu Perfil" pelo nome real
        spanNome.textContent = nomeDoUsuario;

        // Opcional: Adicionar um botão de sair dinamicamente
        const logoutBtn = document.createElement('span');
        logoutBtn.onclick = () => {
            localStorage.removeItem('usuarioLogado');
            window.location.href = 'index.html';
        };
        spanNome.parentElement.appendChild(logoutBtn);
    }
});