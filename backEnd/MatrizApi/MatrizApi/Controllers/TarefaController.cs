using Microsoft.AspNetCore.Mvc;
using MatrizApi.Models;

namespace MatrizApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TarefaController : ControllerBase
    {
        // Banco de Dados!
        private readonly AppDbContext _context;

        // O Construtor: Quando o C# cria o Garçom, ele "injeta" o Banco de Dados aqui
        public TarefaController(AppDbContext context)
        {
            _context = context;
        }

        // 1.(GET)
        [HttpGet("usuario/{usuarioId}")]
        public ActionResult<IEnumerable<Tarefa>> BuscarTodasDoUsuario(int usuarioId)
        {
            var tarefas = _context.Tarefas.Where(t => t.UsuarioId == usuarioId).ToList();

            return Ok(tarefas);
        }

        // 2.(POST)
        [HttpPost]
        public ActionResult<Tarefa> CriarNova(Tarefa novaTarefa)
        {
           
            _context.Tarefas.Add(novaTarefa);

            _context.SaveChanges();

            return CreatedAtAction(nameof(BuscarTodasDoUsuario), new { usuarioId = novaTarefa.UsuarioId }, novaTarefa);
        }

        // 3.(DELETE)
        [HttpDelete("{id}")]
        public IActionResult Deletar(int id)
        {
            var tarefa = _context.Tarefas.FirstOrDefault(t => t.Id == id);
            if (tarefa == null) return NotFound();

            _context.Tarefas.Remove(tarefa);
            _context.SaveChanges();

            return NoContent();
        }

        // 4.(PUT)
        [HttpPut("{id}")]
        public IActionResult Editar(int id, Tarefa tarefaAtualizada)
        {
            var tarefa = _context.Tarefas.FirstOrDefault(t => t.Id == id);
            if (tarefa == null) return NotFound();

            tarefa.Texto = tarefaAtualizada.Texto;
            tarefa.Quadrante = tarefaAtualizada.Quadrante;

            _context.SaveChanges();

            return Ok(tarefa);
        }
    }
}