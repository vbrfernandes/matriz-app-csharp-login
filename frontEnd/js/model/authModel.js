export default class AuthModel {
  constructor() {
    this.baseUrl = "https://localhost:7091/api/Auth";
  }

  getUsuarioNome() {
    return localStorage.getItem("usuarioNome");
  }

  salvarSessao(dados) {
    localStorage.setItem("usuarioNome", dados.nome);
    localStorage.setItem("usuarioId", dados.id);
  }

  removerSessao() {
    localStorage.removeItem("usuarioLogado"); // Mantive igual ao seu original
    localStorage.removeItem("usuarioNome");
    localStorage.removeItem("usuarioId");
  }

  async autenticar(email, senha) {
    const resposta = await fetch(`${this.baseUrl}/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, senha }),
    });

    if (!resposta.ok) {
      const erro = await resposta.text();
      throw new Error(erro || "Erro na autenticação");
    }
    return await resposta.json();
  }

  async registrar(dados) {
    const resposta = await fetch(`${this.baseUrl}/register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(dados),
    });

    if (!resposta.ok) throw new Error(await resposta.text());
    return true;
  }

  async verificarToken(token) {
    const resposta = await fetch(`${this.baseUrl}/verificar?token=${token}`);
    if (!resposta.ok) throw new Error(await resposta.text());
    return true;
  }

  async reenviarCodigo(email) {
    const resposta = await fetch(`${this.baseUrl}/reenviar-codigo`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email }),
    });
    if (!resposta.ok) throw new Error(await resposta.text());
    return await resposta.json();
  }

  async solicitarRecuperacao(email) {
    const resposta = await fetch(`${this.baseUrl}/forgot-password`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email }),
    });

    if (!resposta.ok) throw new Error("Falha na solicitação");
    return true;
  }

  async redefinirSenha(token, newPassword) {
    const resposta = await fetch(`${this.baseUrl}/reset-password`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ token, newPassword }),
    });

    if (!resposta.ok) {
      const erro = await resposta.json();
      throw new Error(erro.mensagem || "Erro ao redefinir a senha.");
    }
    return true;
  }
}