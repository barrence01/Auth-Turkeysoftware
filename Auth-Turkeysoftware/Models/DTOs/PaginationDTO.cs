namespace Auth_Turkeysoftware.Models.DTOs
{
    public class PaginationDto<T>
    {
        public long TotalCount { get; set; }
        public int PageCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<T> Data { get; set; } = new List<T>();
        public PaginationDto() { } 
        public PaginationDto(List<T> data, int paginaAtual, int tamanhoPagina, long totalRegistros)
        {
            Data = data;
            PageNumber = paginaAtual;
            PageSize = tamanhoPagina;
            TotalCount = totalRegistros;
            PageCount = (int)Math.Ceiling((double)totalRegistros / (double)tamanhoPagina);
        }
    }
}
