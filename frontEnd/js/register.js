document.querySelector(".login-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  const nome = document.getElementById("name").value;
  const email = document.getElementById("email").value;
  const senha = document.getElementById("password").value;
  const confirmaSenha = document.getElementById("confirm-password").value;
  const passwordInput = document.getElementById("password");
  const errorSpan = document.getElementById("password-error");
  const matchErrorSpan = document.getElementById("match-error");
  const password = passwordInput.value;


  const strongPasswordRegex =
    /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;

  if (password !== confirmaSenha) {
    // Mostra a mensagem de erro
    matchErrorSpan.style.display = "block";

    // Opcional: Destaca o campo de confirmação com erro (usando a classe CSS anterior)
    confirmaSenha.classList.add("input-error");

    return; // Para a execução (evita que o formulário seja enviado)
  } else {
    // Se forem iguais, esconde o erro
    matchErrorSpan.style.display = "none";
    confirmaSenha.classList.remove("input-error");
  }

  if (!strongPasswordRegex.test(password)) {
    // Mostra a mensagem de erro na tela
    errorSpan.textContent =
      "Use pelo menos 8 caracteres, incluindo maiúsculas, minúsculas, números e símbolos.";
    errorSpan.style.display = "block";

    // Deixa a borda do input vermelha
    passwordInput.classList.add("input-error");

    return;
  } else {
    // Se a senha passar na validação, escondemos o erro e removemos a borda vermelha
    errorSpan.style.display = "none";
    passwordInput.classList.remove("input-error");
  }

  try {
    const resposta = await fetch("https://localhost:7091/api/Auth/register", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ nome: nome, email: email, senha: senha }),
    });

    console.log(resposta);

    if (resposta.ok) {
      const toast = document.getElementById("toast");

      // Adiciona a classe "mostrar" para ela aparecer na tela
      toast.classList.add("mostrar");

      // Espera 2 segundos (2000 milissegundos) para o usuário ler, e redireciona!
      setTimeout(() => {
        window.location.href = "../index.html";
      }, 2000);
    } else {
      const erro = await resposta.text();
      alert("Erro: " + erro);
    }
  } catch (error) {
    alert(
      "Erro ao conectar com o servidor. Verifique se o Back-end está rodando!",
    );
  }
});
