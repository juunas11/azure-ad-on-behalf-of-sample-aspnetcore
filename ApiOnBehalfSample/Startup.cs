using System.Collections.Generic;
using ApiOnBehalfSample.Constants;
using ApiOnBehalfSample.Options;
using ApiOnBehalfSample.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.IdentityModel.Tokens;

namespace ApiOnBehalfSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(o =>
            {
                o.Filters.Add(new AuthorizeFilter("default"));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddAuthorization(o =>
            {
                o.AddPolicy("default", builder =>
                {
                    builder
                        .RequireAuthenticatedUser()
                        .RequireClaim(AzureAdClaimTypes.Scope, "user_impersonation");
                    //Require additional claims, setup other policies etc.
                });
            });

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    AuthenticationOptions authSettings = Configuration.GetSection("Authentication").Get<AuthenticationOptions>();

                    //Identify the identity provider
                    o.Authority = authSettings.Authority;

                    //Require tokens be saved in the AuthenticationProperties on the request
                    //We need the token later to get another token
                    o.SaveToken = true;

                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        //Both the client id and app id URI of this API should be valid audiences
                        ValidAudiences = new List<string> { authSettings.ClientId, authSettings.AppIdUri }
                    };
                });

            services.Configure<AuthenticationOptions>(Configuration.GetSection("Authentication"));
            services.AddSingleton<IGraphApiService, GraphApiService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAuthenticationProvider, OnBehalfOfMsGraphAuthenticationProvider>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
