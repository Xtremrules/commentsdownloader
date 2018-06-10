using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommentsDownloader.Models;
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

        public async Task<IActionResult> SendMail([FromServices] IMailService mailService, CommentsRequestCreate request)
        {
            _repository.Create(request.ToModel());
            await _repository.SaveAsync();
            return RedirectToAction(nameof(ThankYou));
        }

        public IActionResult ThankYou()
        {
            return View();
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
