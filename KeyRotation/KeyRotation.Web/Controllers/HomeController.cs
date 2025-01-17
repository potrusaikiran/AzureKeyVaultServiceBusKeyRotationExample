﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KeyRotation.Web.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.ServiceBus;
using System.Text;
using Newtonsoft.Json;

namespace KeyRotation.Web.Controllers
{
    public class HomeController : Controller
    {
        private static string KeyVaultBaseUrl = "https://keyrotationtestdemo.vault.azure.net";
        private static string ServiceBusSecretKey = "ServiceBusPrimaryKey";

        private static string ServiceBusName = "keyrotationservicebus";
        private static string ServiceBusAccessPolicyName = "RootManageSharedAccessKey";
        private static string ServiceBusQueueName = "myqueue";

        private string ServiceBusPrimaryKey;


        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> About()
        {
            var tokenProvider = new AzureServiceTokenProvider();
            var vaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(tokenProvider.KeyVaultTokenCallback));

            var secret = await vaultClient.GetSecretAsync($"{KeyVaultBaseUrl}/secrets/{ServiceBusSecretKey}").ConfigureAwait(false);
            ServiceBusPrimaryKey = secret.Value;

            var person = new Person { FirstName = "Kasun", LastName = "Kodagoda", Age = 30 };

            ViewBag.Message = await SendMessage(person);

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<string> SendMessage(Person person)
        {
            try
            {
                var connectionString = $"Endpoint=sb://{ServiceBusName}.servicebus.windows.net/;SharedAccessKeyName={ServiceBusAccessPolicyName};SharedAccessKey={ServiceBusPrimaryKey}";
                var client = new QueueClient(connectionString, ServiceBusQueueName);

                var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(person)));
                await client.SendAsync(message);
                await client.CloseAsync();

                return "Message Send person";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
