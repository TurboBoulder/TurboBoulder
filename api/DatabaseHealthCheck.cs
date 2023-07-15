using IdaWebApplicationTemplate.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IdaWebApplicationTemplate
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly ApplicationDbContext _context;

        public DatabaseHealthCheck(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var canConnect = await _context.Database.CanConnectAsync();

            if (canConnect)
            {
                return HealthCheckResult.Healthy();
            }

            return HealthCheckResult.Unhealthy("Cannot connect to database");
        }
    }
}
