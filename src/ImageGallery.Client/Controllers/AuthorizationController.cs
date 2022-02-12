using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ImageGallery.Client.Controllers
{
    public class AuthorizationController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;

        public AuthorizationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ??
                throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task Logout()
        {


            //Call IDP RevocationEndpoint   to revocake access token
            var client = _httpClientFactory.CreateClient("IDPClient");
            var discoveryDocumentResponse = await client.GetDiscoveryDocumentAsync();
            if (discoveryDocumentResponse.IsError)
            { 
            throw new Exception(discoveryDocumentResponse.Error);
            }

            var accessTokenRevocationResponse = await client.RevokeTokenAsync(
                new TokenRevocationRequest
                {
                    Address = discoveryDocumentResponse.RevocationEndpoint,
                    ClientId = "imagegalleryclient",
                    ClientSecret = "secret",
                    Token = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken)
                }); ;

            if (accessTokenRevocationResponse.IsError)
            {
                throw new Exception(accessTokenRevocationResponse.Error);
            }

            //Call IDP RevocationEndpoint   to revocake refresh  token
            var refreshTokenRevocationResponse = await client.RevokeTokenAsync(
             new TokenRevocationRequest
             {
                 Address = discoveryDocumentResponse.RevocationEndpoint,
                 ClientId = "imagegalleryclient",
                 ClientSecret = "secret",
                 Token = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken)
             }); ;

            if (refreshTokenRevocationResponse.IsError)
            {
                throw new Exception(refreshTokenRevocationResponse.Error);
            }


            ////Clean the cookie
            /////Comment this to test token revocation 
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

        }
    }
}
