using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProjeGorevYonetimi.Filters;

/// <summary>
/// Tüm API isteklerinde apiKey parametresini (query veya header) ApiAuthGuid ile doğrular.
/// </summary>
public class ApiKeyAuthFilter : IAuthorizationFilter
{
    private const string ApiKeyParam = "apiKey";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var config = context.HttpContext.RequestServices.GetService<IConfiguration>();
        var expectedGuid = config?.GetValue<string>("ApiAuthGuid") ?? "";

        var apiKey = context.HttpContext.Request.Query[ApiKeyParam].FirstOrDefault()
            ?? context.HttpContext.Request.Headers["X-Api-Key"].FirstOrDefault()
            ?? context.HttpContext.Request.Headers[ApiKeyParam].FirstOrDefault();

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(expectedGuid) ||
            !string.Equals(apiKey.Trim(), expectedGuid.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new JsonResult(new { error = "Yetkisiz erişim. Geçerli apiKey gerekli." })
            {
                StatusCode = 401
            };
        }
    }
}
