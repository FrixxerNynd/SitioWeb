using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace back_cabs.CRM.Middleware
{
    public class CustomRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomRateLimitMiddleware> _logger;
        private readonly RateLimitOptions _options;
        private readonly ConcurrentDictionary<string, TokenBucketRateLimiter> _limiters = new();

        public CustomRateLimitMiddleware(RequestDelegate next, ILogger<CustomRateLimitMiddleware> logger, IOptions<RateLimitOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var key = GetRateLimitKey(context);
            var limiter = _limiters.GetOrAdd(key, _ => CreateLimiter());
            var lease = await limiter.AcquireAsync(1);
            if (!lease.IsAcquired)
            {
                _logger.LogWarning("Rate limit exceeded for key: {Key}", key);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too Many Requests");
                return;
            }
            await _next(context);
        }

        private string GetRateLimitKey(HttpContext context)
        {
            // Prefer UserId if authenticated, else IP
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "id")?.Value;
                if (!string.IsNullOrEmpty(userId))
                    return $"user:{userId}";
            }
            return $"ip:{context.Connection.RemoteIpAddress}";
        }

        private TokenBucketRateLimiter CreateLimiter()
        {
            return new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = _options.TokenLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = _options.QueueLimit,
                ReplenishmentPeriod = TimeSpan.FromSeconds(_options.ReplenishmentPeriodSeconds),
                TokensPerPeriod = _options.TokensPerPeriod,
                AutoReplenishment = true
            });
        }
    }

    public class RateLimitOptions
    {
        public int TokenLimit { get; set; } = 3;
        public int QueueLimit { get; set; } = 0;
        public int ReplenishmentPeriodSeconds { get; set; } = 60;
        public int TokensPerPeriod { get; set; } = 3;
    }

    public static class CustomRateLimitMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomRateLimit(this IApplicationBuilder builder, Action<RateLimitOptions> configureOptions)
        {
            var options = new RateLimitOptions();
            configureOptions(options);
            builder.ApplicationServices.GetRequiredService<IServiceCollection>().AddSingleton(Options.Create(options));
            return builder.UseMiddleware<CustomRateLimitMiddleware>();
        }
    }
}
