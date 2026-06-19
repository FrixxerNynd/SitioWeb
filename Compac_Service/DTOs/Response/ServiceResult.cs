
namespace back_cabs.CRM.DTOs.Response
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public string? Error { get; set; }

        // Para respuestas sin datos (errores, operaciones void)
        public static ServiceResult Fail(string message, string? error = null) => new() { Success = false, Message = message, Error = error };
        public static ServiceResult Ok(string? message = null) => new() { Success = true, Message = message };
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }

        public static ServiceResult<T> Ok(T data, string? message = null) =>
            new() { Success = true, Data = data, Message = message };

        public static new ServiceResult<T> Fail(string message, string? error = null) =>
            new() { Success = false, Message = message, Error = error };
    }

    public class PaginatedResponseDto<T>
    {
        public List<T> Items { get; set; } = [];
        public int TotalRegistros { get; set; }
        public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / PageSize);
        public int PaginaActual { get; set; }
        public int PageSize { get; set; }
    }
}