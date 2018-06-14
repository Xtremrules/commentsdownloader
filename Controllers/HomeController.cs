using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommentsDownloader.Data.Interfaces;
using CommentsDownloader.DTO.Entities;
using CommentsDownloader.Mappings;
using CommentsDownloader.Models;
using CommentsDownloader.Services;
using CommentsDownloader.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CommentsDownloader.Controllers {
    public class HomeController : Controller {
        private readonly IRepository<CommentsRequest> _repository;
        private readonly ILogger<HomeController> _logger;

        public HomeController (IRepository<CommentsRequest> repository, ILogger<HomeController> logger) {
            _repository = repository;
            _logger = logger;
        }
        public IActionResult Index () {
            return View ();
        }

        public IActionResult About () {
            ViewData["Message"] = "Your application description page.";

            return View ();
        }

        [HttpPost]
        public async Task<IActionResult> SendMail (CommentsRequestCreate request) {
            var newRequest = request.ToModel ();
            _repository.Create (newRequest);
            await _repository.SaveAsync ();
            return RedirectToAction (nameof (ThankYou), new { id = newRequest.Id.ToString () });
        }

        public async Task<IActionResult> ThankYou (string id) {
            var request = await _repository.GetByIdAsync (new Guid (id));
            if (request != null) {
                return View (request.ToViewModel ());
            }
            return RedirectToAction (nameof (Index));
        }

        public IActionResult Status (string id) {
            return RedirectToAction (nameof (ThankYou), new { id = id });
        }

        public async Task<IActionResult> Download (string filename) {
            if (filename == null)
                return Content ("filename does not exist, maybe it has expired");

            try {
                var path = Path.Combine (AppConstants.TempFileDirectory, filename);

                var memory = new MemoryStream ();
                using (var stream = new FileStream (path, FileMode.Open)) {
                    await stream.CopyToAsync (memory);
                }
                memory.Position = 0;
                return File (memory, GetContentType (path), "request.csv");
            } catch {
                _logger.LogInformation ("Something happened, file is still in use by a previous process.");
            }
            return RedirectToAction(nameof (ThankYou), new { id = filename.Split('.')[0] });
        }

        public IActionResult Privacy () {
            return View ();
        }

        [ResponseCache (Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error () {
            return View (new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Helpers
        private string GetContentType (string path) {
            var types = GetMimeTypes ();
            var ext = Path.GetExtension (path).ToLowerInvariant ();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes () {
            return new Dictionary<string, string> { { ".txt", "text/plain" },
                { ".pdf", "application/pdf" },
                { ".doc", "application/vnd.ms-word" },
                { ".docx", "application/vnd.ms-word" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats officedocument.spreadsheetml.sheet" },
                { ".png", "image/png" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".gif", "image/gif" },
                { ".csv", "text/csv" }
            };
        }
        #endregion
    }
}