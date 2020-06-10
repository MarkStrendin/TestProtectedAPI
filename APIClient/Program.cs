using System;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

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

            
        }
    }
}
