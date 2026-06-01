using System.Text.Json.Serialization;

namespace login_service.services
{
    public class RecaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecaptchaService> _logger;

        public RecaptchaService(HttpClient httpClient, IConfiguration configuration, ILogger<RecaptchaService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> ValidarV3Async(string token)
        {
            var secretKey = _configuration["Recaptcha:SecretKeyV3"];
            var scoreMinimo = _configuration.GetValue<double>("Recaptcha:ScoreMinimoV3");

            var resultado = await VerificarConGoogle(secretKey!, token);

            if (!resultado.Success)
            {
                _logger.LogWarning("reCAPTCHA v3 falló. Errores: {Errores}",
                    string.Join(", ", resultado.ErrorCodes ?? []));
                return false;
            }
            else if (resultado.Score < scoreMinimo)
            {
                _logger.LogWarning("reCAPTCHA v3 score muy bajo: {Score} (mínimo: {Minimo})",
                    resultado.Score, scoreMinimo);
                return false;
            }
            else
            {
                _logger.LogInformation("reCAPTCHA v3 verificado correctamente. Score: {Score}", resultado.Score);
                return true;
            }
        }

        public async Task<bool> ValidarV2Async(string token)
        {
            var secretKey = _configuration["Recaptcha:SecretKeyV2"];
            var resultado = await VerificarConGoogle(secretKey!, token);

            if (!resultado.Success)
            {
                _logger.LogWarning("reCAPTCHA v2 falló. Errores: {Errores}",
                    string.Join(", ", resultado.ErrorCodes ?? []));
                return false;
            }
            else
            {
                _logger.LogInformation("reCAPTCHA v2 verificado correctamente.");
                return true;
            }
        }

        private async Task<RecaptchaResponse> VerificarConGoogle(string secretKey, string token)
        {
            var respuesta = await _httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}",
                null
            );

            var json = await respuesta.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<RecaptchaResponse>(json)
                   ?? new RecaptchaResponse();
        }
    }

    public class RecaptchaResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

        [JsonPropertyName("error-codes")]
        public List<string>? ErrorCodes { get; set; }
    }
}