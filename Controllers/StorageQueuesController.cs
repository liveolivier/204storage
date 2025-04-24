using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Queues;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace demo204.Controllers
{
    public class StorageQueuesController : Controller
    {
        private readonly string _storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=storage204of;AccountKey=xyitRN3g3lme9SrFJJpTwDMloe4ZFt1Qs4wYcUChvOFo/FOCEAcwX5wqJ08FXV9BgmTcda4CR/Oc+AStp3yDhg==;EndpointSuffix=core.windows.net";
        private readonly string _queueName = "demoqueue";

        // Affiche la vue principale
        public async Task<IActionResult> Index()
        {
            var messages = await GetQueueMessagesAsync();
            ViewBag.Messages = messages;
            ViewBag.ProcessedMessage = TempData["ProcessedMessage"] as string;
            return View();
        }

        // Ajoute un message dans la queue
        [HttpPost]
        public async Task<IActionResult> AddMessage(string message, DateTime date, int quantity)
        {
            if (string.IsNullOrWhiteSpace(message) || quantity <= 0)
            {
                TempData["Error"] = "Veuillez fournir un message valide, une date et une quantité supérieure à 0.";
                return RedirectToAction(nameof(Index));
            }

            var queueClient = new QueueClient(_storageConnectionString, _queueName);
            await queueClient.CreateIfNotExistsAsync();

            var fullMessage = $"{message} | Date: {date:yyyy-MM-dd} | Quantité: {quantity}";
            await queueClient.SendMessageAsync(fullMessage);

            return RedirectToAction(nameof(Index));
        }

        // Traite un message de la queue
        [HttpPost]
        public async Task<IActionResult> ProcessMessage()
        {
            var queueClient = new QueueClient(_storageConnectionString, _queueName);
            await queueClient.CreateIfNotExistsAsync();

            var message = await queueClient.ReceiveMessageAsync();
            if (message.Value != null)
            {
                TempData["ProcessedMessage"] = message.Value.MessageText;
                await queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
            }
            else
            {
                TempData["ProcessedMessage"] = "Aucun message à traiter.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Récupère les messages de la queue
        private async Task<List<string>> GetQueueMessagesAsync()
        {
            var queueClient = new QueueClient(_storageConnectionString, _queueName);
            await queueClient.CreateIfNotExistsAsync();

            var messages = new List<string>();
            var peekedMessages = await queueClient.PeekMessagesAsync(10); // Récupère jusqu'à 10 messages
            foreach (var message in peekedMessages.Value)
            {
                messages.Add(message.MessageText);
            }

            return messages;
        }
    }
}