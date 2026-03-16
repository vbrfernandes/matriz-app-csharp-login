// model.js
export default class MatrixModel {
  constructor() {
    this.tarefas = [];
    this.carregarTarefasDoServidor();
  }

  carregarTarefasDoServidor() {

    const usuarioId = localStorage.getItem("usuarioId");

    // Se não tiver ninguém logado, para por aqui e nem tenta buscar
    if (!usuarioId) return;

    // 2. Apontamos para a nova rota no C# passando o ID na URL
    fetch(`https://localhost:7091/api/Tarefa/usuario/${usuarioId}`)
      .then((resposta) => resposta.json())
      .then((dadosDoServidor) => {
        // Agora o C# e o JS falam exatamente a mesma língua!
        this.tarefas = dadosDoServidor;

        if (this.onTodoListChanged) {
          this.onTodoListChanged(this.tarefas);
        }

        console.log("📥 Tarefas carregadas do C# com sucesso!", this.tarefas);
      })
      .catch((erro) => {
        console.error("Ops! Erro ao buscar tarefas do C#:", erro);
      });
  }

  bindTodoListChanged(callback) {
    this.onTodoListChanged = callback;
    if (this.tarefas.length > 0) {
      this.onTodoListChanged(this.tarefas);
    }
  }

  addTask(texto, quadrante) {

    const usuarioId = localStorage.getItem("usuarioId");

    const novaTarefa = {
      id: Date.now(),
      texto: texto,
      quadrante: quadrante,
      usuarioId: parseInt(usuarioId), // Vincula a tarefa ao usuário no Front
    };
    this.tarefas.push(novaTarefa);

    const tarefaParaOBackend = {
      texto: texto,
      quadrante: quadrante,
      usuarioId: parseInt(usuarioId), // Avisa o C# de quem é essa tarefa!
    };

    fetch("https://localhost:7091/api/Tarefa", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(tarefaParaOBackend),
    })
      .then((resposta) => resposta.json())
      .then((dadosSalvos) => {
        const tarefaNaLista = this.tarefas.find((t) => t.id === novaTarefa.id);
        if (tarefaNaLista) {
          tarefaNaLista.id = dadosSalvos.id;

          if (this.onTodoListChanged) {
            this.onTodoListChanged(this.tarefas); // Manda a tela redesenhar com os botões corretos!
          }
        }
        console.log("🎉 ID atualizado para o oficial do C#:", dadosSalvos.id);
      })
      .catch((erro) => console.error("Ops! Erro ao enviar para o C#:", erro));
  }

  deleteTask(id) {
    this.tarefas = this.tarefas.filter((t) => t.id !== id);

    fetch(`https://localhost:7091/api/Tarefa/${id}`, {
      method: "DELETE",
    })
      .then(() => console.log(`🗑️ SUCESSO! C# deletou a tarefa ${id}`))
      .catch((erro) => console.error("Ops! Erro ao deletar no C#:", erro));
  }

  editTask(id, textoAtualizado) {
    const tarefaAtual = this.tarefas.find((t) => t.id === id);
    if (!tarefaAtual) return;

    this.tarefas = this.tarefas.map((t) =>
      t.id === id ? { ...t, texto: textoAtualizado } : t,
    );

    if (this.onTodoListChanged) {
      this.onTodoListChanged(this.tarefas);
    }

    const tarefaEditada = {
      id: id,
      texto: textoAtualizado,
      quadrante: tarefaAtual.quadrante,
    };

    fetch(`https://localhost:7091/api/Tarefa/${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(tarefaEditada),
    })
      .then((resposta) => {
        if (resposta.ok) {
          console.log(
            `✏️ SUCESSO! C# editou a tarefa ${id} sem apagar o quadrante!`,
          );
        }
      })
      .catch((erro) => console.error("Ops! Erro ao editar no C#:", erro));
  }
}
