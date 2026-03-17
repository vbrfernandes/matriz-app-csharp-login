using MatrizApi;
using MatrizApi.Models;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using System.Net;
using System.Net.Http.Json;


namespace MatrizApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. REGISTO
        [HttpPost("register")]
        public async Task<IActionResult> Register(Usuario novoUsuario)
        {
            if (_context.Usuarios.Any(u => u.Email == novoUsuario.Email))
                return BadRequest("E-mail já cadastrado.");

            novoUsuario.Senha = BCrypt.Net.BCrypt.HashPassword(novoUsuario.Senha);
            novoUsuario.TokenVerificacao = Guid.NewGuid().ToString();
            novoUsuario.EmailVerificado = false;

            _context.Usuarios.Add(novoUsuario);
            _context.SaveChanges();


            await EnviarEmailVerificacao(novoUsuario.Email, novoUsuario.TokenVerificacao);

            return Ok(new { mensagem = "Usuário criado! Por favor, verifique o seu e-mail." });
        }

        // 2. LOGIN
        [HttpPost("login")]
        public IActionResult Login([FromBody] Usuario loginInfo)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == loginInfo.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginInfo.Senha, usuario.Senha))
            {
                return Unauthorized("E-mail ou senha incorretos.");
            }

            if (!usuario.EmailVerificado)
            {
                return BadRequest("Por favor, verifique a sua conta no link enviado para o seu e-mail antes de fazer login.");
            }

            return Ok(new { id = usuario.Id, nome = usuario.Nome });
        }

        // 3. VERIFICAÇÃO DE E-MAIL
        [HttpGet("verificar")]
        public IActionResult VerificarEmail([FromQuery] string token)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.TokenVerificacao == token);

            if (usuario == null)
                return BadRequest("Token inválido ou já utilizado.");

            if (usuario.EmailVerificado)
                return Ok("Sua conta já esta verificada! Pode ir direto para o Login.");

            usuario.EmailVerificado = true;
            usuario.TokenVerificacao = null;
            _context.SaveChanges();

            return Ok("E-mail verificado com sucesso! Já pode voltar à página inicial e fazer login.");
        }

        // 4. ENVIAR E-MAIL
        private async Task EnviarEmailVerificacao(string emailDestino, string token)
        {
            string apiKey = _configuration["Brevo:ApiKey"] ?? "";
            string link = $"https://vbrfernandes.github.io/matriz-app-csharp-login/pages/verificado.html?token={token}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            var payload = new
            {
                sender = new { name = "Matriz App", email = "vitorschoolinf@gmail.com" },
                to = new[] { new { email = emailDestino } },
                subject = "Verifique sua conta no Matriz!",
                htmlContent = $@"
            <html>
                <body>
                    <h1>Bem-vindo ao Matriz!</h1>
                    <p>Clique no link para verificar sua conta:</p>
                    <a href='{link}'>Verificar Minha Conta</a>
                </body>
            </html>"
            };

            var response = await client.PostAsJsonAsync("https://api.brevo.com/v3/smtp/email", payload);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro ao enviar e-mail: {erro}");
            }
        }

        // 5. REENVIAR E-MAIL DE VERIFICAÇÃO
        [HttpPost("reenviar-codigo")]
        public async Task<IActionResult> ReenviarCodigo([FromBody] ReenviarCodigoDto request)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == request.Email);

            if (usuario == null)
                return BadRequest("Não encontramos nenhuma conta com esse e-mail.");

            if (usuario.EmailVerificado)
                return BadRequest("Esta conta já está verificada!");

            usuario.TokenVerificacao = Guid.NewGuid().ToString();
            _context.SaveChanges();

            await EnviarEmailVerificacao(usuario.Email, usuario.TokenVerificacao);

            return Ok(new { mensagem = "Novo link enviado!" });
        }

        // 6. SOLICITAR RECUPERAÇÃO DE SENHA
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
                return Ok(new { mensagem = "Se o e-mail existir, um link foi enviado." });

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

            _context.SaveChanges();

            await EnviarEmailRecuperacao(user.Email, user.PasswordResetToken);

            return Ok(new { mensagem = "Link de recuperação enviado com sucesso." });
        }

        // 7. REDEFINIR A SENHA
        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = _context.Usuarios.FirstOrDefault(u =>
                u.PasswordResetToken == request.Token && u.ResetTokenExpires > DateTime.UtcNow);

            if (user == null)
                return BadRequest(new { mensagem = "Token inválido ou expirado." });

            user.Senha = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            _context.SaveChanges(); 

            return Ok(new { mensagem = "Senha alterada com sucesso!" });
        }

        // 8. MÉTODO AUXILIAR PARA O E-MAIL DE RECUPERAÇÃO
        private async Task EnviarEmailRecuperacao(string emailDestino, string token)
        {
            string apiKey = _configuration["Brevo:ApiKey"] ?? "";
            string link = $"https://vbrfernandes.github.io/matriz-app-csharp-login/pages/redefinirSenha.html?token={token}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            var payload = new
            {
                sender = new { name = "Matriz App", email = "vitorschoolinf@gmail.com" },
                to = new[] { new { email = emailDestino } },
                subject = "Recuperação de Senha - Matriz",
                htmlContent = $"<html><body><h1>Recuperação de Senha</h1><p>Você solicitou a troca de senha.</p><a href='{link}'>Redefinir Minha Senha</a><p>Se não foi você, ignore este e-mail.</p></body></html>"
            };

            await client.PostAsJsonAsync("https://api.brevo.com/v3/smtp/email", payload);
        }
    }
    
    public class ReenviarCodigoDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Token, string NewPassword);
}