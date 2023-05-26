using RestSharp;

using System.Net;

namespace Aticas.CoreAPI.Services
{
    public class TokenManagerMiddleware : IMiddleware
    {
        private readonly string responseHeaders = string.Join(", ", new string[4]{
               "origin",
               "authorization",
               "content-type",
               "accept"
            });
        private readonly ISessionManager _sessionManager;

        public TokenManagerMiddleware(ISessionManager tokenManager)
        {
            _sessionManager = tokenManager;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            #region CORS
            context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            context.Response.Headers.Add("Access-Control-Allow-Headers", responseHeaders);
            #endregion

            if (context.Request.Method == "OPTIONS")
            {
                context.Response.StatusCode = 204;
                await context.Response.CompleteAsync();
                return;
            }
            if (await _sessionManager.IsCurrentActiveToken())
            {
                await next(context);
                return;
            }
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            
        }
    }
}
