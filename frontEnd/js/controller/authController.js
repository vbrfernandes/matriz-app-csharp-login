import AuthModel from "../model/authModel.js";
import AuthView from "../view/authView.js";

class AuthController {
  constructor() {
    this.model = new AuthModel();
    this.view = new AuthView();
  }

  init() {
    document.addEventListener("DOMContentLoaded", () => {
      this.verificarSessao();
      this.bindLogin();
      this.bindRegister();
      this.bindVerificacao();
      this.bindEsqueceuSenha();
      this.bindRedefinirSenha();
    });
  }

  verificarSessao() {
    const nome = this.model.getUsuarioNome();
    if (nome) {
      this.view.exibirUsuario(nome, () => {
        this.model.removerSessao();
        // Redireciona para index dependendo de onde está
        const inPagesFolder = window.location.pathname.includes("/pages/");
        window.location.href = inPagesFolder ? "../index.html" : "index.html";
      });
    }
  }

  bindLogin() {
    // Procura o form de login e checa se NÃO estamos na página de registro
    const form = document.querySelector(".login-form");
    const isRegisterPage = document.getElementById("confirm-password");

    if (form && !isRegisterPage) {
      form.addEventListener("submit", async (e) => {
        e.preventDefault();
        const email = document.getElementById("email").value;
        const senha = document.getElementById("password").value;

        try {
          const dados = await this.model.autenticar(email, senha);
          this.model.salvarSessao(dados);
          
          const inPagesFolder = window.location.pathname.includes("/pages/");
          window.location.href = inPagesFolder ? "home.html" : "./pages/home.html";
        } catch (error) {
          this.view.exibirErro(error.message);
        }
      });
    }
  }

  bindRegister() {
    const form = document.querySelector(".login-form");
    const isRegisterPage = document.getElementById("confirm-password");

    // Só roda se for a página de registro
    if (form && isRegisterPage) {
      form.addEventListener("submit", async (e) => {
        e.preventDefault();
        const data = this.view.getRegisterFormData();

        const strongPasswordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;

        if (data.senha !== data.confirmaSenha) {
          this.view.toggleError("match-error", "Senhas não conferem.", true);
          this.view.toggleInputHighlight("confirm-password", true);
          return;
        }

        if (!strongPasswordRegex.test(data.senha)) {
          this.view.toggleError("password-error", "Senha fraca.", true);
          this.view.toggleInputHighlight("password", true);
          return;
        }

        this.view.toggleError("match-error", "", false);
        this.view.toggleError("password-error", "", false);

        try {
          await this.model.registrar({
            nome: data.nome,
            email: data.email,
            senha: data.senha,
          });
          this.view.showToast("Cadastro realizado com sucesso!");
          setTimeout(() => (window.location.href = "../index.html"), 2500);
        } catch (error) {
          this.view.showToast(error.message, true);
        }
      });
    }
  }

  async bindVerificacao() {

    const statusTitle = document.getElementById("status-title");
    if (!statusTitle) return;

    const urlParams = new URLSearchParams(window.location.search);
    const token = urlParams.get("token");

    if (!token) {
      this.view.exibirStatus("Ops!", "Token ausente.", "Link inválido.", "#e74c3c");
      return;
    }

    try {
      await this.model.verificarToken(token);
      this.view.exibirStatus("Conta Verificada!", "Tudo pronto.", "E-mail validado.", "#4caf50");
      setTimeout(() => (window.location.href = "../index.html"), 3500);
    } catch (e) {
      this.view.exibirStatus("Falha", "Erro na verificação.", e.message, "#e74c3c");
    } finally {
      this.view.toggleSpinner(false);
    }

    const btnReenvio = document.querySelector(".register-text a");
    if (btnReenvio) {
      btnReenvio.addEventListener("click", async (e) => {
        e.preventDefault();
        const email = prompt("Digite seu e-mail:");
        if (!email) return;

        try {
          const data = await this.model.reenviarCodigo(email.trim());
          alert("Sucesso: " + data.mensagem);
        } catch (e) {
          alert("Erro: " + e.message);
        }
      });
    }
  }

  bindEsqueceuSenha() {
    const formEsqueceu = document.getElementById("formEsqueceuSenha");
    if (!formEsqueceu) return;

    formEsqueceu.addEventListener("submit", async (e) => {
      e.preventDefault();
      const email = document.getElementById("email").value;

      this.view.setLoadingButton(".login-btn", true, "Enviando...", "Enviar link de recuperação");

      try {
        await this.model.solicitarRecuperacao(email);
        this.view.showToast("Se o e-mail existir na base, um link foi enviado!");
      } catch (error) {
        this.view.showToast("Erro ao enviar o link. Tente novamente.", true);
        console.error(error);
      } finally {
        this.view.setLoadingButton(".login-btn", false, "Enviando...", "Enviar link de recuperação");
      }
    });
  }

  bindRedefinirSenha() {
    const formRedefinir = document.getElementById("formRedefinir");
    if (!formRedefinir) return;

    formRedefinir.addEventListener("submit", async (e) => {
      e.preventDefault();

      const urlParams = new URLSearchParams(window.location.search);
      const token = urlParams.get("token");

      if (!token) {
        alert("Link inválido ou ausente.");
        return;
      }

      const { novaSenha, confirmaSenha } = this.view.getResetFormData();

      if (novaSenha !== confirmaSenha) {
        alert("As senhas não coincidem!");
        return;
      }

      this.view.setLoadingButton(".login-btn", true, "Salvando...", "Salvar Nova Senha");

      try {
        await this.model.redefinirSenha(token, novaSenha);
        this.view.showToast("Senha alterada com sucesso!");
        setTimeout(() => (window.location.href = "../index.html"), 2500);
      } catch (error) {
        this.view.showToast(error.message, true);
      } finally {
        this.view.setLoadingButton(".login-btn", false, "Salvando...", "Salvar Nova Senha");
      }
    });
  }
}


const appController = new AuthController();
appController.init();