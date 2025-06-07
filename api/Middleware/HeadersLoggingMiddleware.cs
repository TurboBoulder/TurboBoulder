namespace TurboBoulder.Middleware
{

    public class HeadersLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public HeadersLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            foreach (var header in context.Request.Headers)
            {
                Console.WriteLine($"{header.Key}: {header.Value}");
            }

            await _next(context);
        }
    }
}