namespace MatrizApi.Models
{
    public class Tarefa
    {
        // O ID único da tarefa (em C# usamos int para números inteiros)
        public int Id { get; set; }

        // O texto da tarefa (em C# usamos string para textos)
        public string? Texto { get; set; }

        // O quadrante onde ela está (ex: "q1", "q2")
        public string? Quadrante { get; set; }
    }
}