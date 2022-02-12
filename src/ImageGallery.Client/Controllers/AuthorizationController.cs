using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ImageGallery.Client.Controllers
{
    public class AuthorizationController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task Logout()
        {
            //Clean the cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
