using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProjeGorevYonetimi.Services;

namespace ProjeGorevYonetimi.Filters;

/// <summary>
/// Session (ve cookie) boşsa redirectMainProjectUrl adresine yönlendirir.
/// API istekleri ve Home/Giris, Home/Error hariç tüm controller aksiyonlarında çalışır.
/// </summary>
public class RequireSessionFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var path = context.HttpContext.Request.Path;
        // API isteklerinde session zorunlu değil
        if (path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            return;

        var action = context.RouteData.Values["action"]?.ToString();
        var controller = context.RouteData.Values["controller"]?.ToString();
        // Giriş ve Error sayfaları hariç
        if (string.Equals(controller, "Home", StringComparison.OrdinalIgnoreCase) &&
            (string.Equals(action, "Giris", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(action, "Giriş", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(action, "Error", StringComparison.OrdinalIgnoreCase)))
            return;

        var currentUser = context.HttpContext.RequestServices.GetService<ICurrentUserService>();
        var cerezdenId = currentUser?.GetCurrentCerezdenKullaniciId();
        if (cerezdenId.HasValue)
            return;

        var config = context.HttpContext.RequestServices.GetService<IConfiguration>();
        var redirectUrl = config?.GetValue<string>("redirectMainProjectUrl");
        if (string.IsNullOrEmpty(redirectUrl))
            redirectUrl = "/";

        context.Result = new RedirectResult(redirectUrl);
    }
}
