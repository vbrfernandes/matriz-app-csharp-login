document.querySelector('.login-form').addEventListener('submit', async (e) => {
    e.preventDefault();

    const email = document.getElementById('email').value;
    const senha = document.getElementById('password').value;

    try {
        const resposta = await fetch("https://localhost:7091/api/Auth/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email: email, senha: senha })
        });

        if (resposta.ok) {
            const dados = await resposta.json();
            
            // Salvamos o nome e agora também o ID do usuário!
            localStorage.setItem('usuarioNome', dados.nome);
            localStorage.setItem('usuarioId', dados.id); 
            
            window.location.href = "./pages/home.html";
        } else {
            alert("E-mail ou senha incorretos.");
        }
    } catch (error) {
        alert("Erro ao conectar com o servidor.");
    }
});