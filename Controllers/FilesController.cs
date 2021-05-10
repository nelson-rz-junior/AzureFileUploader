using AzureFileUploader.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AzureFileUploader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private const int MaxFilenameLength = 50;
        private readonly IStorage _storage;
        private static readonly Regex regex = new("[^a-zA-Z0-9._]");

        public FilesController(IStorage storage)
        {
            _storage = storage;
        }

        // GET /api/Files
        // Called by the page when it's first loaded, whenever new files are uploaded, and every five seconds on a timer.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var names = await _storage.GetNames();

            var baseUrl = Request.Path.Value;

            var urls = names.Select(n => $"{baseUrl}/{n}");

            return Ok(urls);
        }

        // POST /api/Files
        // Called once for each file uploaded.
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            // IFormFile.FileName is untrustworthy user input, and we're using it for both blob names and for display on the page,
            // so we aggressively sanitize. In a real app, we'd probably do something more complex and robust for handling filenames.
            var name = SanitizeFilename(file.FileName);

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException();
            }

            using (Stream stream = file.OpenReadStream())
            {
                await _storage.Save(stream, name);
            }

            return Accepted();
        }

        // GET /api/Files/{filename}
        // Called when clicking a link to download a specific file.
        [HttpGet("{filename}")]
        public async Task<IActionResult> Download(string filename)
        {
            var stream = await _storage.Load(filename);

            // This usage of File() always triggers the browser to perform a file download.
            // We always use "application/octet-stream" as the content type because we don't record any information about content type from
            // the user when they upload a file.
            return File(stream, "application/octet-stream", filename);
        }

        private static string SanitizeFilename(string filename)
        {
            var sanitizedFilename = regex.Replace(filename, "").TrimEnd('.');

            if (sanitizedFilename.Length > MaxFilenameLength)
            {
                sanitizedFilename = sanitizedFilename.Substring(0, MaxFilenameLength);
            }

            return sanitizedFilename;
        }
    }
}
