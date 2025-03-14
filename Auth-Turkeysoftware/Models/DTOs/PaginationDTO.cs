namespace Auth_Turkeysoftware.Models.DTOs
{
    public class PaginationDTO<T>
    {
        public long TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public int PaginaAtual { get; set; }
        public int QtdRegistrosPorPagina { get; set; }
        public T Data { get; set; }

        public PaginationDTO(long totalRegistros, int totalPaginas, int paginaAtual, int qtdRegistrosPorPagina, T data)
        {
            TotalRegistros = totalRegistros;
            TotalPaginas = totalPaginas;
            PaginaAtual = paginaAtual;
            QtdRegistrosPorPagina = qtdRegistrosPorPagina;
            Data = data;
        }
    }
}
