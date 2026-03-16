export default class AuthView {
  exibirUsuario(nome, onLogout) {
    const spanNome = document.querySelector(".user-name");
    if (!spanNome) return;

    spanNome.textContent = nome;

    const logoutBtn = document.createElement("span");
    logoutBtn.textContent = " (Sair)";
    logoutBtn.style.cursor = "pointer";
    logoutBtn.onclick = onLogout;
    spanNome.parentElement.appendChild(logoutBtn);
  }

  exibirErro(msg) {
    alert(msg);
  }

  // Específico para Cadastro
  getRegisterFormData() {
    return {
      nome: document.getElementById("name")?.value,
      email: document.getElementById("email")?.value,
      senha: document.getElementById("password")?.value,
      confirmaSenha: document.getElementById("confirm-password")?.value,
    };
  }

  // Específico para Redefinir Senha
  getResetFormData() {
    return {
      novaSenha: document.getElementById("novaSenha")?.value,
      confirmaSenha: document.getElementById("confirmaSenha")?.value,
    };
  }

  toggleError(elementId, message, show) {
    const el = document.getElementById(elementId);
    if (!el) return;
    if (message) el.textContent = message;
    el.style.display = show ? "block" : "none";
  }

  toggleInputHighlight(inputId, hasError) {
    const el = document.getElementById(inputId);
    if (!el) return;
    hasError
      ? el.classList.add("input-error")
      : el.classList.remove("input-error");
  }

  showToast(msg, isError = false, duration = 3500) {
    const toast = document.getElementById("toast");
    if (!toast) return;
    
    toast.textContent = msg;
    toast.className = `mostrar ${isError ? "toast-error" : "toast-success"}`;
    
    setTimeout(() => {
      toast.classList.remove("mostrar");
      toast.className = ""; 
    }, duration);
  }

  exibirStatus(titulo, desc, msg, cor) {
    const titleEl = document.getElementById("status-title");
    const descEl = document.getElementById("status-desc");
    const msgEl = document.getElementById("message-text");

    if (titleEl) titleEl.textContent = titulo;
    if (descEl) descEl.textContent = desc;
    if (msgEl) {
      msgEl.textContent = msg;
      msgEl.style.color = cor;
    }
  }

  toggleSpinner(show) {
    const spinner = document.getElementById("loading-spinner");
    if (spinner) spinner.style.display = show ? "block" : "none";
  }


  setLoadingButton(btnSelector, isLoading, loadingText, defaultText) {
    const btn = document.querySelector(btnSelector);
    if (!btn) return;
    btn.textContent = isLoading ? loadingText : defaultText;
    btn.disabled = isLoading;
  }
}