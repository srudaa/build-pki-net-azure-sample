﻿using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BuildPkiSample.Setup
{
    internal class Program
    {
        public static async Task Main()
        {
            var configuration = ReadConfiguration();
            AcquireTokenResult acquireTokenResult = await new AuthenticationHelper(configuration.ClientId, configuration.TenantId, AuthenticationHelper.AzureManagementScopes).AcquireTokenAsync();
            await new ResourceManagementHelper(configuration, acquireTokenResult).CreateAzureResourcesAsync(false);

            acquireTokenResult = await new AuthenticationHelper(configuration.ClientId, configuration.TenantId, AuthenticationHelper.KeyVaultScopes).AcquireTokenAsync();
            var certificate = await new RootCertificateHelper(configuration, acquireTokenResult.AccessToken).GenerateRootCertificate();

            await WriteCertificateToFile(certificate);
        }

        private static Configuration ReadConfiguration()
        {
            var configurationRoot = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();
            var configuration = configurationRoot.Get<Configuration>();
            return configuration;
        }

        private static async Task WriteCertificateToFile(X509Certificate2 certificate)
        {
            const string fileName = "RootCert.cer";
            var fullFilePath = Path.Combine(Environment.CurrentDirectory, fileName);
            await File.WriteAllTextAsync(fullFilePath, Convert.ToBase64String(certificate.Export(X509ContentType.Cert)));
            Console.WriteLine($"Stored public issuer certificate at '{fullFilePath}'");
        }
    }
}
