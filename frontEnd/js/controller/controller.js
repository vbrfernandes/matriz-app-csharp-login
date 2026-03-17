// controller.js
import MatrixModel from "../model/model.js";
import MatrixView from "../view/view.js";

class MatrixController {
  constructor(model, view) {
    this.model = model;
    this.view = view;

    this.setupEventListeners();
    this.view.bindEditTask(this.handleEditTask.bind(this));
    this.model.bindTodoListChanged(this.onTodoListChanged);

    this.view.displayTasks(this.model.tarefas);
  }

  onTodoListChanged = (tarefas) => {
    this.view.displayTasks(tarefas);
  };

  setupEventListeners() {
    const themeToggleBtn = document.getElementById("theme-toggle");
    themeToggleBtn.addEventListener("click", () => {
      document.body.classList.toggle("dark-mode");
      themeToggleBtn.textContent = document.body.classList.contains("dark-mode")
        ? "☀️ Modo Claro"
        : "🌙 Modo Escuro";
    });

    const updateBtnText = (isDark) => {
        themeToggleBtn.textContent = isDark ? "☀️ Modo Claro" : "🌙 Modo Escuro";
    };

    themeToggleBtn.addEventListener("click", () => {
        // Verifica se o tema atual é dark
        const isDark = document.documentElement.getAttribute("data-theme") === "dark";
        if (isDark) {
            // Muda para claro
            document.documentElement.removeAttribute("data-theme");
            updateBtnText(false);
            localStorage.setItem("theme", "light");
        } else {
            // Muda para escuro
            document.documentElement.setAttribute("data-theme", "dark");
            updateBtnText(true);
            localStorage.setItem("theme", "dark");
        }
    });

    const quadrantes = document.querySelectorAll(".quadrant");

    quadrantes.forEach((quadrante) => {
      const addBtn = quadrante.querySelector(".add-btn");
      const input = quadrante.querySelector(".task-input");
      const quadranteId = quadrante.classList[1];

      addBtn.addEventListener("click", (e) => {
        e.preventDefault(); //
        this.handleAddTask(input, quadranteId);
      });

      input.addEventListener("keypress", (e) => {
        if (e.key === "Enter") {
          e.preventDefault(); // 🛑 Freia o recarregamento ao apertar Enter
          this.handleAddTask(input, quadranteId);
        }
      });
    });

    const matrixContainer = document.querySelector(".eisenhower-matrix");
    matrixContainer.addEventListener("click", (e) => {
      if (e.target.classList.contains("delete-btn")) {
        e.preventDefault(); // 🛑 Freia o recarregamento ao deletar
        const idTarefa = parseInt(e.target.parentElement.id);
        this.handleDeleteTask(idTarefa);
      }
    });

    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', () => {
            localStorage.removeItem('usuarioLogado');
            window.location.href = '../index.html';
        });
    }
  }

  handleAddTask(inputElement, quadranteId) {
    const texto = inputElement.value.trim();
    if (texto !== "") {
      this.model.addTask(texto, quadranteId);
      inputElement.value = "";
      this.view.displayTasks(this.model.tarefas);
    }
  }

  handleDeleteTask(id) {
    this.model.deleteTask(id);
    this.view.displayTasks(this.model.tarefas);
  }

  handleEditTask(id, textoAtualizado) {
    this.model.editTask(id, textoAtualizado);
    this.view.displayTasks(this.model.tarefas);
  }
}

const app = new MatrixController(new MatrixModel(), new MatrixView());
