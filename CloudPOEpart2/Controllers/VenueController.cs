using CloudPOEpart2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CloudPOEpart2.Controllers
{
    public class VenueController : Controller
    {
        private readonly DataBaseContext _context;
        private readonly IConfiguration _configuration;

        public VenueController(DataBaseContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Index: Displays all venues
        public async Task<IActionResult> Index()
        {
            var venues = await _context.Venue.ToListAsync();
            return View(venues);
        }

        // Create (GET): Display the form to create a venue
        public IActionResult Create()
        {
            return View();
        }

        // Create (POST): Handles image upload to Azure Blob and creates a new venue
        [HttpPost]
        public async Task<IActionResult> Create(Venue venue, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // Upload image to Azure Blob Storage
                string blobUrl = await UploadImageToAzure(imageFile);
                venue.ImageUrl = blobUrl;
            }
            else
            {
                // Fallback if no image provided
                venue.ImageUrl = "https://via.placeholder.com/150";
            }

            if (ModelState.IsValid)
            {
                _context.Add(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(venue);
        }

        // Edit (GET): Display the form to edit a venue
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null)
                return NotFound();

            return View(venue);
        }

        // Edit (POST): Handles image re-upload and updates venue details
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Venue venue, IFormFile imageFile)
        {
            if (id != venue.VenueID)
                return NotFound();

            if (imageFile != null && imageFile.Length > 0)
            {
                // Upload new image and overwrite old URL
                string blobUrl = await UploadImageToAzure(imageFile);
                venue.ImageUrl = blobUrl;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(venue);
        }

        // Delete (GET): Display confirmation before deleting
        public async Task<IActionResult> Delete(int? id)
        {
            var venue = await _context.Venue.FirstOrDefaultAsync(m => m.VenueID == id);
            if (venue == null)
                return NotFound();

            return View(venue);
        }

        // Delete (POST): Delete venue if not linked to bookings
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);

            bool isLinked = await _context.Booking.AnyAsync(b => b.VenueID == id);
            if (isLinked)
            {
                TempData["ErrorMessage"] = "Cannot delete this venue because it is associated with existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            if (venue != null)
            {
                _context.Venue.Remove(venue);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueID == id);
        }

        // Uploads the image to Azure Blob Storage and returns the public URL
        private async Task<string> UploadImageToAzure(IFormFile imageFile)
        {
            string connectionString = _configuration["AzureBlobStorage:ConnectionString"];
            string containerName = _configuration["AzureBlobStorage:ContainerName"];

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure container exists
            await containerClient.CreateIfNotExistsAsync();

            // Generate unique filename
            var blobClient = containerClient.GetBlobClient($"{Guid.NewGuid()}_{imageFile.FileName}");

            // Set content type for image
            var contentType = imageFile.ContentType;
            var blobHttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
            {
                ContentType = contentType
            };

            // Upload the image stream with content type
            using (var stream = imageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(
                content: stream,
                options: new Azure.Storage.Blobs.Models.BlobUploadOptions
                {
                    HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                });

            }

            return blobClient.Uri.ToString(); // Return the image URL
        }
    }
}
