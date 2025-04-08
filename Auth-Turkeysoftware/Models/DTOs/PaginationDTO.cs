namespace Auth_Turkeysoftware.Models.DTOs
{
    public class PaginationDto<T>
    {
        public long TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
        public int PaginaAtual { get; set; }
        public int TamanhoPagina { get; set; }
        public List<T> Data { get; set; }

        public PaginationDto(List<T> data, int paginaAtual, int tamanhoPagina, long totalRegistros)
        {
            Data = data;
            PaginaAtual = paginaAtual;
            TamanhoPagina = tamanhoPagina;
            TotalRegistros = totalRegistros;
            TotalPaginas = (int)Math.Ceiling((double)totalRegistros / (double)tamanhoPagina);
        }
    }
}
