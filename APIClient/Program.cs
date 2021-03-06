﻿using System;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Identity.Client;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;

namespace APIClient
{
    class Program
    {
        static IConfiguration configuration = new ConfigurationBuilder()
                   .AddEnvironmentVariables()
                   .Build();

        static void Main(string[] args)
        {
            string azkvEndpoint = configuration["KEYVAULT_ENDPOINT"];
            Console.WriteLine("> Connecting to Azure Key Vault: " + azkvEndpoint);            
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(
                    azureServiceTokenProvider.KeyVaultTokenCallback));
            configuration = new ConfigurationBuilder()
                    .AddConfiguration(configuration)
                    .AddAzureKeyVault(azkvEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager())
                    .Build();

            AuthConfig authConfig = new AuthConfig() {
                Instance = configuration["Client:Instance"],
                TenantId = configuration["AAD:TenantId"],
                ClientSecret = configuration["Client:ClientSecret"],
                ClientId = configuration["Client:ClientId"],
                ResourceId = $"{configuration["AAD:ResourceId"]}/.default" // ".default" is the "scope" for the request
            };

            DoTheThing(authConfig).GetAwaiter().GetResult();
            
            Console.WriteLine("Done");
        }

        private static async Task DoTheThing(AuthConfig authConfig)
        {
            IConfidentialClientApplication application = ConfidentialClientApplicationBuilder
                .Create(authConfig.ClientId)
                .WithClientSecret(authConfig.ClientSecret)
                .WithAuthority(new Uri(authConfig.Authority))
                .Build();

            string[] ResourceIds = new string[] { authConfig.ResourceId };

            AuthenticationResult result = null;

            try {
                result = await application.AcquireTokenForClient(ResourceIds).ExecuteAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Token acquired");
                Console.WriteLine(result.AccessToken);
                Console.ResetColor();
            } catch(Exception ex) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();                
            }

            if (!string.IsNullOrEmpty(result.AccessToken)) {
                var httpClient = new HttpClient();
                var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

                if ((defaultRequestHeaders.Accept == null) || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json")) {
                    httpClient.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json")
                    );
                }

                defaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("bearer", result.AccessToken);

                HttpResponseMessage response = await httpClient.GetAsync("https://localhost:5001/WeatherForecast");

                if (response.IsSuccessStatusCode) {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Success");
                } else {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Fail");

                }

                string content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
                Console.ResetColor();
            }
        }
    }
}
