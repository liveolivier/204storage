using Microsoft.AspNetCore.Mvc;
using Azure.Data.Tables;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;

namespace demo204.Controllers
{
    public class StorageTableController : Controller
    {
        private readonly string _storageConnectionString;
        private readonly string _tableName = "stagiaires";

        public StorageTableController(IConfiguration configuration)
        {
            _storageConnectionString = configuration.GetConnectionString("StorageAccount");
        }

        // Affiche la liste des stagiaires
        public async Task<IActionResult> Index()
        {
            var tableClient = new TableClient(_storageConnectionString, _tableName);
            await tableClient.CreateIfNotExistsAsync();

            var stagiaires = new List<Stagiaire>();
            await foreach (var entity in tableClient.QueryAsync<TableEntity>())
            {
                stagiaires.Add(new Stagiaire
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    Nom = entity.GetString("Nom"),
                    Prenom = entity.GetString("Prenom"),
                    Poste = entity.GetString("Poste")
                });
            }

            return View(stagiaires);
        }

        // Supprime un stagiaire de la table
        [HttpGet]
        public IActionResult Delete(string partitionKey, string rowKey)
        {
            var tableClient = new TableClient(_storageConnectionString, _tableName);
            tableClient.CreateIfNotExistsAsync();

            try
            {
                tableClient.DeleteEntityAsync(partitionKey, rowKey);
            }
            catch (RequestFailedException ex)
            {
                // Gérer les erreurs si nécessaire
                ModelState.AddModelError(string.Empty, $"Erreur lors de la suppression : {ex.Message}");
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        // Affiche le formulaire pour ajouter un stagiaire
        public IActionResult Create()
        {
            return View();
        }

        // Insère un nouveau stagiaire dans la table
        [HttpPost]
        public async Task<IActionResult> Create(Stagiaire stagiaire)
        {
            if (!ModelState.IsValid)
            {
                return View(stagiaire);
            }

            var tableClient = new TableClient(_storageConnectionString, _tableName);
            await tableClient.CreateIfNotExistsAsync();

            var entity = new TableEntity
            {
                PartitionKey = "Stagiaires",
                RowKey = Guid.NewGuid().ToString(),

                ["Nom"] = stagiaire.Nom,
                ["Prenom"] = stagiaire.Prenom,
                ["Poste"] = stagiaire.Poste
            };

            await tableClient.AddEntityAsync(entity);

            return RedirectToAction(nameof(Index));
        }
    }

    // Modèle pour représenter un stagiaire
    public class Stagiaire
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Poste { get; set; }
    }
}