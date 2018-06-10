using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommentsDownloader.ViewModels;
using CommentsDownloader.Services;
using CommentsDownloader.DTO.Entities;
using CommentsDownloader.Data.Interfaces;
using CommentsDownloader.Mappings;

namespace CommentsDownloader.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<CommentsRequest> _repository;

        public HomeController(IRepository<CommentsRequest> repository)
        {
            _repository = repository;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMail(CommentsRequestCreate request)
        {
            var newRequest = request.ToModel();
            _repository.Create(newRequest);
            await _repository.SaveAsync();
            return RedirectToAction(nameof(ThankYou), new {id = newRequest.Id.ToString()});
        }

        public async Task<IActionResult> ThankYou(string id)
        {
            var request = await _repository.GetByIdAsync(new Guid(id));
            if (request != null)
            {
                return View(request.ToViewModel());
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Status(string id)
        {
            return RedirectToAction(nameof(ThankYou), new {id = id} );
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
