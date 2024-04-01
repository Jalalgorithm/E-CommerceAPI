using Backend.ApiModel.Base;
using Newtonsoft.Json;

namespace Backend.Data
{
    public class Custom401Middleware
    {
        private readonly RequestDelegate _next;

        public Custom401Middleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == 401)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new {errorMessage = "User not authenticated"}));
            }
        }
    }
}
