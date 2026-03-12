using Microsoft.AspNetCore.Mvc;
using MatrizApi.Models;

namespace MatrizApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TarefaController : ControllerBase
    {
        // 1. ADEUS lista provisória! OLÁ Banco de Dados!
        private readonly AppDbContext _context;

        // O Construtor: Quando o C# cria o Garçom, ele "injeta" o Banco de Dados aqui
        public TarefaController(AppDbContext context)
        {
            _context = context;
        }

        // 1. O Garçom que ENTREGA o cardápio (GET)
        [HttpGet]
        public ActionResult<IEnumerable<Tarefa>> BuscarTodas()
        {
            // Vai no SQLite, pega a tabela 'Tarefas' inteira e transforma numa Lista
            var tarefas = _context.Tarefas.ToList();

            return Ok(tarefas);
        }

        // 2. O Garçom que ANOTA um novo pedido (POST)
        [HttpPost]
        public ActionResult<Tarefa> CriarNova(Tarefa novaTarefa)
        {
            // Não precisamos mais criar o ID manualmente. O Banco de Dados faz isso sozinho!
            // Adicionamos a tarefa na tabela
            _context.Tarefas.Add(novaTarefa);

            // 🔥 A MÁGICA: Manda gravar fisicamente no arquivo tarefas.db
            _context.SaveChanges();

            return CreatedAtAction(nameof(BuscarTodas), new { id = novaTarefa.Id }, novaTarefa);
        }

        // 3. O Garçom que APAGA um pedido (DELETE)
        [HttpDelete("{id}")]
        public IActionResult Deletar(int id)
        {
            // Procura a tarefa pelo ID lá no Banco de Dados
            var tarefa = _context.Tarefas.FirstOrDefault(t => t.Id == id);
            if (tarefa == null) return NotFound();

            // Remove e manda gravar a alteração no arquivo
            _context.Tarefas.Remove(tarefa);
            _context.SaveChanges();

            return NoContent();
        }

        // 4. O Garçom que EDITA um pedido (PUT)
        [HttpPut("{id}")]
        public IActionResult Editar(int id, Tarefa tarefaAtualizada)
        {
            // Procura a tarefa pelo ID no Banco de Dados
            var tarefa = _context.Tarefas.FirstOrDefault(t => t.Id == id);
            if (tarefa == null) return NotFound();

            // Atualiza os dados
            tarefa.Texto = tarefaAtualizada.Texto;
            tarefa.Quadrante = tarefaAtualizada.Quadrante;

            // Manda gravar a alteração no arquivo
            _context.SaveChanges();

            return Ok(tarefa);
        }
    }
}