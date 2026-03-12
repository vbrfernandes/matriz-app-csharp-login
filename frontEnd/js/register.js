document.querySelector('.login-form').addEventListener('submit', async (e) => {
    e.preventDefault();

    const nome = document.getElementById('name').value;
    const email = document.getElementById('email').value;
    const senha = document.getElementById('password').value;
    const confirmaSenha = document.getElementById('confirm-password').value;

    if (senha !== confirmaSenha) {
        alert("As senhas não coincidem!");
        return;
    }

    try {
        const resposta = await fetch("https://localhost:7091/api/Auth/register", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ nome: nome, email: email, senha: senha })
        });

        if (resposta.ok) {
            alert("Conta criada com sucesso!");
            window.location.href = "login.html";
        } else {
            const erro = await resposta.text();
            alert("Erro: " + erro);
        }
    } catch (error) {
        alert("Erro ao conectar com o servidor. Verifique se o Back-end está rodando!");
    }
});