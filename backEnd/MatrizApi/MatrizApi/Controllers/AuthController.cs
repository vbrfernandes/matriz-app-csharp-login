using MatrizApi;
using MatrizApi.Models;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using System.Net;
using System.Net.Mail;


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
        public IActionResult Register(Usuario novoUsuario)
        {
            if (_context.Usuarios.Any(u => u.Email == novoUsuario.Email))
                return BadRequest("E-mail já cadastrado.");

            novoUsuario.Senha = BCrypt.Net.BCrypt.HashPassword(novoUsuario.Senha);

            novoUsuario.TokenVerificacao = Guid.NewGuid().ToString();
            novoUsuario.EmailVerificado = false; 

            _context.Usuarios.Add(novoUsuario);
            _context.SaveChanges();

            
            EnviarEmailVerificacao(novoUsuario.Email, novoUsuario.TokenVerificacao);

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
        private void EnviarEmailVerificacao(string emailDestino, string token)
        {
            string smtpUser = _configuration["Smtp:User"] ?? "";
            string smtpPass = _configuration["Smtp:Pass"] ?? "";

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            string link = $"https://vbrfernandes.github.io/matriz-app-csharp-login/pages/verificado.html?token={token}";

            var corpoEmail = $@"
            <div style='font-family: ""Segoe UI"", Tahoma, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f4f7f6; border-radius: 10px;'>
                <div style='text-align: center; margin-bottom: 20px;'>
                    <h2 style='color: #333333; margin: 0; font-size: 28px; letter-spacing: -1px;'>Matriz</h2>
                </div>
                <div style='background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 15px rgba(0,0,0,0.05); text-align: center;'>
                    <h1 style='color: #2196f3; font-size: 24px; margin-top: 0;'>Bem-vindo(a)!</h1>
                    <p style='color: #555555; font-size: 16px; line-height: 1.6;'>
                        Falta muito pouco para você começar a organizar suas tarefas com mais eficiência. Clique no botão abaixo para ativar a sua conta:
                    </p>
                    <div style='margin: 30px 0;'>
                        <a href='{link}' style='background-color: #2196f3; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 16px; display: inline-block;'>
                            Verificar Minha Conta
                        </a>
                    </div>
                    <p style='color: #999999; font-size: 12px; margin-bottom: 0;'>
                        Se você não solicitou este cadastro, pode ignorar este e-mail em segurança.
                    </p>
                </div>
            </div>";

            var mailMessage = new MailMessage
            {
                From = new MailAddress("vitorschoolinf@gmail.com"),
                Subject = "Verifique sua conta no nosso App!",
                Body = corpoEmail,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(emailDestino);
            client.Send(mailMessage);
        }

        // 5. REENVIAR E-MAIL DE VERIFICAÇÃO
        [HttpPost("reenviar-codigo")]
        public IActionResult ReenviarCodigo([FromBody] ReenviarCodigoDto request)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == request.Email);

            if (usuario == null)
                return BadRequest("Não encontramos nenhuma conta com esse e-mail.");

            if (usuario.EmailVerificado)
                return BadRequest("Esta conta já está verificada! Pode ir direto para o Login.");

            usuario.TokenVerificacao = Guid.NewGuid().ToString();
            _context.SaveChanges();

            EnviarEmailVerificacao(usuario.Email, usuario.TokenVerificacao);

            return Ok(new { mensagem = "Novo link de verificação enviado! Cheque sua caixa de entrada e o Spam." });
        }

        // 6. SOLICITAR RECUPERAÇÃO DE SENHA
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
                return Ok(new { mensagem = "Se o e-mail existir, um link foi enviado." });

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

            _context.SaveChanges();

            EnviarEmailRecuperacao(user.Email, user.PasswordResetToken);

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
        private void EnviarEmailRecuperacao(string emailDestino, string token)
        {
            string smtpUser = _configuration["Smtp:User"] ?? "";
            string smtpPass = _configuration["Smtp:Pass"] ?? "";

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            string link = $"https://vbrfernandes.github.io/matriz-app-csharp-login/pages/verificado.html?token={token}";

            var corpoEmail = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; text-align: center;'>
                <h2>Recuperação de Senha</h2>
                <p>Você solicitou a redefinição de sua senha.</p>
                <p>Clique no link abaixo para criar uma nova senha (válido por 1 hora):</p>
                <br>
                <a href='{link}' style='background: #2196f3; color: #fff; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                    Redefinir Minha Senha
                </a>
                <br><br>
                <p style='font-size: 12px; color: #777;'>Se você não solicitou isso, ignore este e-mail.</p>
            </div>";

            var mailMessage = new MailMessage
            {
                From = new MailAddress("vitorschoolinf@gmail.com"),
                Subject = "Redefinição de Senha - Matriz",
                Body = corpoEmail,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(emailDestino);
            client.Send(mailMessage);
        }
    }
    
    public class ReenviarCodigoDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Token, string NewPassword);
}