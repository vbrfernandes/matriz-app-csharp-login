namespace MatrizApi.Models
{
    public class Tarefa
    {
        public int Id { get; set; }
        public string? Texto { get; set; }
        public string? Quadrante { get; set; }
        public int UsuarioId { get; set; }
    }
}