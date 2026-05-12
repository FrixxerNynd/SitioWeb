// =====================================================================================
// DTO GENÉRICO PARA RESPUESTAS PAGINADAS - PaginatedResponseDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define un DTO genérico para encapsular respuestas paginadas de cualquier entidad.
// Puede ser reutilizado por cualquier servicio que necesite devolver datos paginados.
//
// =====================================================================================

namespace CRM.DTOs.Response
{
    /// <summary>
    /// DTO genérico para respuestas paginadas.
    /// </summary>
    /// <typeparam name="T">Tipo de los elementos en la lista</typeparam>
    public class PaginatedResponseDto<T>
    {
        /// <summary>
        /// Lista de elementos de la página actual
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Total de elementos en todas las páginas
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Número de página actual (base 1)
        /// </summary>
        public int Pagina { get; set; }

        /// <summary>
        /// Cantidad de elementos por página
        /// </summary>
        public int ResultadosPorPagina { get; set; }

        /// <summary>
        /// Total de páginas disponibles
        /// </summary>
        public int TotalPaginas => ResultadosPorPagina > 0 ? (int)Math.Ceiling((double)TotalItems / ResultadosPorPagina) : 0;

        /// <summary>
        /// Indica si hay una página anterior
        /// </summary>
        public bool HasPreviousPage => Pagina > 1;

        /// <summary>
        /// Indica si hay una página siguiente
        /// </summary>
        public bool HasNextPage => Pagina < TotalPaginas;
    }
}
