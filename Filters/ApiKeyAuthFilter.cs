using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProjeGorevYonetimi.Services;

namespace ProjeGorevYonetimi.Filters;

/// <summary>
/// API isteklerinde: oturum açmış kullanıcı (session/cookie) varsa geçer; yoksa apiKey (query/header) ApiAuthGuid ile eşleşmeli.
/// Böylece tarayıcıdan yapılan istekler apiKey olmadan, harici API çağrıları apiKey ile çalışır.
/// </summary>
public class ApiKeyAuthFilter : IAuthorizationFilter
{
    private const string ApiKeyParam = "apiKey";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var currentUser = context.HttpContext.RequestServices.GetService<ICurrentUserService>();
        var cerezdenId = currentUser?.GetCurrentCerezdenKullaniciId();

        // Oturum açmış kullanıcı varsa (aynı site üzerinden kullanım) apiKey zorunlu değil
        if (cerezdenId.HasValue)
            return;

        var config = context.HttpContext.RequestServices.GetService<IConfiguration>();
        var expectedGuid = config?.GetValue<string>("ApiAuthGuid") ?? "";

        var apiKey = context.HttpContext.Request.Query[ApiKeyParam].FirstOrDefault()
            ?? context.HttpContext.Request.Headers["X-Api-Key"].FirstOrDefault()
            ?? context.HttpContext.Request.Headers[ApiKeyParam].FirstOrDefault();

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(expectedGuid) ||
            !string.Equals(apiKey.Trim(), expectedGuid.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new JsonResult(new { error = "Yetkisiz erişim. Oturum açın veya geçerli apiKey gönderin." })
            {
                StatusCode = 401
            };
        }
    }
}
