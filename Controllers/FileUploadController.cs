using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Threading.Tasks;

namespace demo204.Controllers
{
    public class FileUploadController : Controller
    {
        private readonly string _azureBlobConnectionString;
        private readonly string _containerName = "uploads";

        public FileUploadController(IConfiguration configuration)
        {
            _azureBlobConnectionString = configuration.GetConnectionString("StorageAccount");
        }

        // Affiche le formulaire de téléchargement
        public IActionResult Index()
        {
            //obtenir la liste des blobs    
            var blobContainerClient = new BlobContainerClient(_azureBlobConnectionString, _containerName);
            blobContainerClient.CreateIfNotExists();
            ViewBag.BlobList = GetBlobListAsync(blobContainerClient).Result; // Appel synchrone pour la démonstration
            return View();
        }

        // Gère le téléchargement du fichier
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "Veuillez sélectionner un fichier.";
                ViewBag.BlobList = GetBlobListAsync(new BlobContainerClient(_azureBlobConnectionString, _containerName)).Result; // Appel synchrone pour la démonstration
                return View("Index");
            }

            try
            {
                // Créer un client BlobContainer
                var blobContainerClient = new BlobContainerClient(_azureBlobConnectionString, _containerName);
                await blobContainerClient.CreateIfNotExistsAsync();

                // Générer un nom unique pour le fichier
                var blobName = file.FileName;
                var blobClient = blobContainerClient.GetBlobClient(blobName);

                // Télécharger le fichier dans le Blob
                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                ViewBag.Message = "Fichier téléchargé avec succès !";

                // Appeler la méthode pour obtenir la liste des blobs
                ViewBag.BlobList = await GetBlobListAsync(blobContainerClient); 

                // Fournir l'URL du fichier téléchargé
                ViewBag.FileUrl = blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Erreur lors du téléchargement : {ex.Message}";
            }

            return View("Index");
        }

        //ajouter une méthode pour obtenir la liste des blobs
        private async Task<string[]> GetBlobListAsync(BlobContainerClient blobContainerClient)
        {
            var blobList = new List<string>();
            await foreach (var blobItem in blobContainerClient.GetBlobsAsync())
            {
                blobList.Add(blobItem.Name);
            }
            return blobList.ToArray();
        }
    }
}