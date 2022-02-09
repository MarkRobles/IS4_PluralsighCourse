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
                    new List<string>() { "role"})
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
                "imagegalleryapi"
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