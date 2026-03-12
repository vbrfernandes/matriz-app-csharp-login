using MatrizApi;
using MatrizApi.Models;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context) { _context = context; }

    [HttpPost("register")]
    public IActionResult Register(Usuario novoUsuario)
    {
        if (_context.Usuarios.Any(u => u.Email == novoUsuario.Email))
            return BadRequest("E-mail já cadastrado.");


        novoUsuario.Senha = BCrypt.Net.BCrypt.HashPassword(novoUsuario.Senha);

        _context.Usuarios.Add(novoUsuario);
        _context.SaveChanges();
        return Ok(new { mensagem = "Usuário criado com sucesso!" });
    }
    [HttpPost("login")]
    public IActionResult Login([FromBody] Usuario loginInfo)
    {
        // 1. Procura o usuário apenas pelo E-mail primeiro
        var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == loginInfo.Email);

        // 2. AQUI A MÁGICA: O BCrypt verifica se a senha digitada bate com a criptografada
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginInfo.Senha, usuario.Senha))
        {
            return Unauthorized("E-mail ou senha incorretos.");
        }

        return Ok(new { id = usuario.Id, nome = usuario.Nome });
    }
}