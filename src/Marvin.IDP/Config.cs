// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace Marvin.IDP
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                new IdentityResource(
                    "roles",
                    "Your roles(s)",
                    new List<string>() { "role"}),
                     new IdentityResource(
                    "country",
                    "The country you're living in",
                    new List<string>() { "country"}),
                     new IdentityResource(
                    "suscriptionlevel",
                    "Your suscription level",
                    new List<string>() { "suscriptionlevel"})
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            { 
                new ApiScope("imagegalleryapi","Image Gallery API",   new List<string>() { "role"})
            };

        public static IEnumerable<Client> Clients =>
            new Client[] 
            { 
            new Client
            { 
                /*Al usar AccessTokenType.Reference cuando se quire autenticar a nivel api, la api llama al token instrospection endpoint del idp
                 * para validar y obtener el contenido real del token, pero para poder hacer eso, ese endpoint requiere que estes autenticado
                 * asi que necesitamos definir una contraseña-secret
                 Como el cliente del token instrospection endpoint es la api, debemos definir el secret a nivel de api*/
                AccessTokenType=AccessTokenType.Reference,
               // IdentityTokenLifetime = //number of seconds, default is 5 minutes
           
                /*The authorization code is exchanged for one or more tokens when the token endpoint is called
                 * that something that happens during the initial flow,  default is 5 minutes*/
                    //AuthorizationCodeLifetime

                //default is 1 hour
                  AccessTokenLifetime =30, //we set it to litte time to try, no funciono, se supone que me deberia impedir el acceso a la api despues de 10 segundos porque el token vencia...
                    AllowOfflineAccess =true,
            //   AbsoluteRefreshTokenLifetime//use this property if you want to chance the default 30 days,
               //RefreshTokenExpiration = TokenExpiration.Sliding,//once a new refresh token is requested its live time will be renueve by the SlidingRefreshTokenLifetime(opposito to absolute..)
            // SlidingRefreshTokenLifeTime 
               UpdateAccessTokenClaimsOnRefresh=true,
                /*imagine one of the users claims is changed, the adress
                by default the claims in the access token states as ther are when refreshing the access token 
                */
                ClientName="ImageGallery",
                ClientId="imagegalleryclient",
                AllowedGrantTypes= GrantTypes.Code,
                RedirectUris = new List<string>()
                {
                "https://localhost:44389/signin-oidc"
                },
                PostLogoutRedirectUris= new List<string>()
                {
                      "https://localhost:44389/signout-callback-oidc"
                },
                RequireConsent=true,
                AllowedScopes={ 
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.Address,
                "roles",
                "imagegalleryapi",
                "country",
                "suscriptionlevel"
                },
                ClientSecrets=
                { 
                new Secret("secret".Sha256())
                },
                RequirePkce=true
            }
            };
    }
}