using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace back_cabs.CRM.services.shared
{
    /// <summary>
    /// Servicio para procesamiento y conversión de imágenes a formato WebP.
    /// Implementa conversión optimizada, redimensionamiento y validación.
    /// </summary>
    public class ImageProcessingService
    {
        private readonly ILogger<ImageProcessingService> _logger;

        public ImageProcessingService(ILogger<ImageProcessingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Convierte una imagen a formato WebP con compresión optimizada.
        /// </summary>
        /// <param name="inputStream">Stream de la imagen original.</param>
        /// <param name="outputPath">Ruta donde guardar el archivo WebP.</param>
        /// <param name="quality">Calidad de compresión (1-100, default 80).</param>
        /// <param name="maxWidth">Ancho máximo (opcional, para redimensionar).</param>
        /// <param name="maxHeight">Alto máximo (opcional, para redimensionar).</param>
        /// <returns>Tupla con información del archivo procesado (tamaño, ancho, alto).</returns>
        /// <exception cref="InvalidOperationException">Si hay error en el procesamiento.</exception>
        public virtual async Task<(long tamanoBytes, int ancho, int alto)> ConvertToWebPAsync(
            Stream inputStream, 
            string outputPath, 
            int quality = 80,
            int? maxWidth = null,
            int? maxHeight = null)
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));
            
            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentNullException(nameof(outputPath));

            try
            {
                _logger.LogInformation("Iniciando conversión a WebP: {OutputPath}, Calidad: {Quality}", outputPath, quality);

                using var image = await Image.LoadAsync(inputStream);
                
                var originalWidth = image.Width;
                var originalHeight = image.Height;

                // Redimensionar si se especifican dimensiones máximas
                if (maxWidth.HasValue || maxHeight.HasValue)
                {
                    var resizeOptions = new ResizeOptions
                    {
                        Size = new Size(maxWidth ?? image.Width, maxHeight ?? image.Height),
                        Mode = ResizeMode.Max // Mantiene proporción
                    };
                    image.Mutate(x => x.Resize(resizeOptions));
                    
                    _logger.LogInformation("Imagen redimensionada de {OriginalW}x{OriginalH} a {NewW}x{NewH}", 
                        originalWidth, originalHeight, image.Width, image.Height);
                }

                // Configurar encoder WebP con compresión Lossy para mejor compresión
                var encoder = new WebpEncoder
                {
                    Quality = quality,
                    FileFormat = WebpFileFormatType.Lossy,
                    Method = WebpEncodingMethod.BestQuality
                };

                // Asegurar que el directorio existe
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Guardar como WebP
                await image.SaveAsync(outputPath, encoder);

                var fileInfo = new FileInfo(outputPath);
                var tamanoKB = fileInfo.Length / 1024.0;
                
                _logger.LogInformation("Imagen convertida exitosamente a WebP: {OutputPath}, Tamaño: {TamanoKB:F2}KB, Dimensiones: {Width}x{Height}", 
                    outputPath, tamanoKB, image.Width, image.Height);

                return (fileInfo.Length, image.Width, image.Height);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir imagen a WebP: {OutputPath}", outputPath);
                
                // Limpiar archivo parcial si existe
                if (File.Exists(outputPath))
                {
                    try { File.Delete(outputPath); } catch { }
                }
                
                throw new InvalidOperationException("Error al procesar la imagen.", ex);
            }
        }

        /// <summary>
        /// Valida que el archivo sea una imagen válida y permitida.
        /// </summary>
        /// <param name="file">Archivo a validar.</param>
        /// <returns>True si es válido, False en caso contrario.</returns>
        public virtual bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg", "image/webp", "image/gif" };
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            var contentType = file.ContentType?.ToLower();

            var isValidType = !string.IsNullOrEmpty(contentType) && allowedTypes.Contains(contentType);
            var isValidExtension = !string.IsNullOrEmpty(extension) && allowedExtensions.Contains(extension);

            if (!isValidType || !isValidExtension)
            {
                _logger.LogWarning("Archivo inválido: ContentType={ContentType}, Extension={Extension}", 
                    contentType, extension);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida que el archivo sea realmente una imagen analizando su contenido.
        /// </summary>
        /// <param name="stream">Stream del archivo.</param>
        /// <returns>True si es una imagen válida.</returns>
        public virtual async Task<bool> IsValidImageContentAsync(Stream stream)
        {
            try
            {
                var originalPosition = stream.Position;
                stream.Position = 0;

                using var image = await Image.LoadAsync(stream);
                stream.Position = originalPosition;

                return image != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
