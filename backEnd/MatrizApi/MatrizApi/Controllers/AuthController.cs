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
            string link = $"https://vbrfernandes.github.io/matriz-app-csharp-login/pages/verificado.html?token={token}";

            string html = $@"
    <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
        <div style='background-color: #4F46E5; padding: 20px; text-align: center;'>
            <h1 style='color: white; margin: 0; font-size: 24px;'>Bem-vindo ao Matriz!</h1>
        </div>
        <div style='padding: 30px; color: #333; line-height: 1.6;'>
            <p style='font-size: 16px;'>Olá!</p>
            <p>Ficamos felizes em ter você conosco. Para começar a explorar nossa plataforma, precisamos apenas que confirme seu endereço de e-mail.</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{link}' style='background-color: #4F46E5; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Verificar Minha Conta</a>
            </div>
            <p style='font-size: 12px; color: #666;'>Se o botão não funcionar, copie: {link}</p>
        </div>
    </div>";

            await EnviarEmailGenerico(emailDestino, "Verifique sua conta no Matriz!", html);
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
            string link = $"https://vbrfernandes.github.io/matriz-app-csharp-login/pages/redefinirSenha.html?token={token}";

            string html = $@"
    <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
        <div style='background-color: #1f2937; padding: 20px; text-align: center;'>
            <h1 style='color: white; margin: 0; font-size: 24px;'>Recuperação de Senha</h1>
        </div>
        <div style='padding: 30px; color: #333; line-height: 1.6;'>
            <p>Você solicitou a redefinição de senha para sua conta.</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{link}' style='background-color: #1f2937; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Redefinir Minha Senha</a>
            </div>
            <p style='font-size: 14px; color: #ef4444;'>Se não foi você, ignore este e-mail.</p>
        </div>
    </div>";

            await EnviarEmailGenerico(emailDestino, "Recuperação de Senha - Matriz", html);
        }

        private async Task EnviarEmailGenerico(string emailDestino, string assunto, string htmlCorpo)
        {
            string apiKey = _configuration["Brevo:ApiKey"] ?? "";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            var payload = new
            {
                sender = new { name = "Matriz App", email = "vitorschoolinf@gmail.com" },
                to = new[] { new { email = emailDestino } },
                subject = assunto,
                htmlContent = htmlCorpo
            };

            var response = await client.PostAsJsonAsync("https://api.brevo.com/v3/smtp/email", payload);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro ao enviar e-mail: {erro}");
            }
        }
    }
    
    public class ReenviarCodigoDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Token, string NewPassword);

    
}