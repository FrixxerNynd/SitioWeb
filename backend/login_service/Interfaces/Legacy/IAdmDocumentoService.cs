using back_cabs.CRM.DTOs.Legacy;

namespace back_cabs.CRM.Interfaces.Legacy
{
    /// <summary>
    /// Interfaz para el servicio de AdmDocumentos
    /// </summary>
    public interface IAdmDocumentoService
    {
        /// <summary>
        /// Busca documentos aplicando filtros con paginación
        /// </summary>
        Task<(List<AdmDocumentoResponseDto> documentos, int totalRegistros)> SearchPaginatedAsync(AdmDocumentoFilterDto filter);

        /// <summary>
        /// Obtiene un documento por ID incluyendo sus movimientos (productos cotizados)
        /// </summary>
        Task<AdmDocumentoResponseDto?> GetByIdWithMovimientosAsync(int idDocumento);

        /// <summary>
        /// Obtiene un documento por ID y verifica si esta facturado
        /// </summary>
        Task<bool?> GetByIdForFacturaAsync(int idDocumento);

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS POST (CREACIÓN)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Crea un nuevo documento en el sistema legacy
        /// Valida las relaciones y aplica reglas de negocio
        /// </summary>
        /// <param name="dto">DTO con los datos del documento a crear</param>
        /// <returns>El ID del documento creado</returns>
        Task<int> CreateAsync(AdmDocumentoCreateDto dto);

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS PARA COTIZACIONES MEJORADAS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Crea una nueva cotización de forma simplificada
        /// Aplica valores por defecto y calcula totales automáticamente
        /// </summary>
        /// <param name="dto">DTO simplificado de cotización</param>
        /// <returns>Respuesta con datos de la cotización creada</returns>
        Task<AdmCotizacionCreateResponseDto> CreateCotizacionAsync(AdmCotizacionCreateDto dto);

        /// <summary>
        /// Edita una cotización existente
        /// </summary>
        /// <summary>
        /// Edita una cotización existente
        /// </summary>
        Task<AdmCotizacionCreateResponseDto> EditCotizacionAsync(AdmCotizacionUpdateDto dto);

        /// <summary>
        /// Cancela una cotización existente
        /// Cambia el campo CCANCELADO a 1
        /// </summary>
        /// <param name="dto">DTO con datos de cancelación</param>
        /// <returns>Respuesta con datos de la cotización cancelada</returns>
        Task<AdmCotizacionCancelarResponseDto> CancelarCotizacionAsync(AdmCotizacionCancelarDto dto);

        /// <summary>
        /// Elimina una cotización (solo si está cancelada)
        /// </summary>
        Task DeleteAsync(int idDocumento);

        // ═══════════════════════════════════════════════════════════════
        // MÉTODOS PARA REPORTES Y ESTADÍSTICAS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene estadísticas generales de cotizaciones para dashboard
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial del rango (opcional)</param>
        /// <param name="fechaFin">Fecha final del rango (opcional)</param>
        /// <returns>Estadísticas agregadas del período</returns>
        Task<EstadisticasGeneralesDto> GetEstadisticasGeneralesAsync(DateTime? fechaInicio, DateTime? fechaFin);

        /// <summary>
        /// Obtiene el top N de clientes con más cotizaciones y montos
        /// </summary>
        /// <param name="top">Número de clientes a retornar</param>
        /// <param name="fechaInicio">Fecha inicial del rango (opcional)</param>
        /// <param name="fechaFin">Fecha final del rango (opcional)</param>
        /// <returns>Lista ordenada de top clientes</returns>
        Task<List<TopClienteDto>> GetTopClientesAsync(int top, DateTime? fechaInicio, DateTime? fechaFin);

        /// <summary>
        /// Obtiene cotizaciones próximas a vencer en los próximos N días con paginación
        /// </summary>
        /// <param name="dias">Número de días para considerar como "próximas a vencer"</param>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Registros por página</param>
        /// <returns>Lista de cotizaciones próximas a vencer y total de registros</returns>
        Task<(List<CotizacionVencimientoDto> items, int total)> GetProximasVencerAsync(int dias, int page, int pageSize);

        /// <summary>
        /// Obtiene rendimiento por agente de ventas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial del rango (opcional)</param>
        /// <param name="fechaFin">Fecha final del rango (opcional)</param>
        /// <returns>Lista de agentes con métricas de rendimiento</returns>
        Task<List<RendimientoAgenteDto>> GetRendimientoAgentesAsync(DateTime? fechaInicio, DateTime? fechaFin);

        /// <summary>
        /// Obtiene productos más cotizados por frecuencia y volumen
        /// </summary>
        /// <param name="top">Número de productos a retornar (top N)</param>
        /// <param name="fechaInicio">Fecha inicial del rango (opcional)</param>
        /// <param name="fechaFin">Fecha final del rango (opcional)</param>
        /// <returns>Lista de productos más cotizados</returns>
        Task<List<ProductoCotizadoDto>> GetProductosMasCotizadosAsync(int top, DateTime? fechaInicio, DateTime? fechaFin);

        /// <summary>
        /// Obtiene distribución de cotizaciones por rangos de monto
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial del rango (opcional)</param>
        /// <param name="fechaFin">Fecha final del rango (opcional)</param>
        /// <returns>Lista de rangos con estadísticas de cotizaciones</returns>
        Task<List<CotizacionPorRangoDto>> GetCotizacionesPorRangoMontoAsync(DateTime? fechaInicio, DateTime? fechaFin);

        /// <summary>
        /// Obtiene estadísticas de cotizaciones agrupadas por mes para un año específico
        /// </summary>
        /// <param name="anio">Año a consultar</param>
        /// <returns>Lista de estadísticas por mes (12 elementos)</returns>
        Task<List<AdmEstadisticasMensualesDto>> GetEstadisticasMensualesAsync(int anio);
    }
}
