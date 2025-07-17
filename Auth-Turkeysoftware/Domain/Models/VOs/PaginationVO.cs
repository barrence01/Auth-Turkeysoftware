namespace Auth_Turkeysoftware.Domain.Models.VOs
{
    public class PaginationVO<T>
    {
        public long TotalCount { get; set; }
        public int PageCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<T> Data { get; set; } = new List<T>();
        public PaginationVO() { }
        public PaginationVO(List<T> data, int paginaAtual, int tamanhoPagina, long totalRegistros)
        {
            Data = data;
            PageNumber = paginaAtual;
            PageSize = tamanhoPagina;
            TotalCount = totalRegistros;
            PageCount = (int)Math.Ceiling(totalRegistros / (double)tamanhoPagina);
        }
    }
}
