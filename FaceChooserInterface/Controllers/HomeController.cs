using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FaceChooserInterface.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace FaceChooserInterface.Controllers
{
    public class HomeController : Controller
    {

        private IConfiguration _config;

        public HomeController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index(string result = "", string errors = "")
        {
            ViewData["Result"] = result;
            ViewData["Errors"] = errors;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessFile(string password, IFormFile File = null)
        {
            var serverVariables = _config.GetSection("ServerVariables").GetChildren().ToList();

            var errors = "";

            var pw = serverVariables.Where(k => k.Key == "SubmitPW").First().Value;

            //enforce no spaces or arguments for triggering discobot will have issues

            if (pw != password)
            {
                errors += "Wrong password\n";
            }
#if DEBUG
            var dlDir = serverVariables.Where(k => k.Key == "UploadPathDebug").First().Value;
#else
            var dlDir = serverVariables.Where(k => k.Key == "UploadPathProd").First().Value;
#endif

            var UploadDirContents = Directory.GetFiles(dlDir);

            var maxFiles = Convert.ToInt32(serverVariables.Where(k => k.Key == "MaxFiles").First().Value);

            if (UploadDirContents.Length >= maxFiles)
            {
                errors += "To many uploads, yell at KK(don't)\n";
            }
            if (File == null)
            {
                errors += "No file uploaded\n";
            }
            else
            {
                var extension = File.FileName.Split('.').ToList().Last();
                if (extension != "png" && extension != "jpg" && extension != "jpeg")
                {
                    errors += "Wrong file type\n";
                }
                 var maxFileSize = Convert.ToInt32(serverVariables.Where(k => k.Key == "MaxFileSizeInBytes").First().Value);

            if (File.Length > maxFileSize)
            {
                errors += "File too large - 400 kb or less\n";
            }

            }

            var result = "";

            if (errors == "")
            {
                result = "Upload success";
                var dlLocation = Path.Combine(dlDir, File.FileName);
                using(var fs = new FileStream(dlLocation, FileMode.Create))
                {
                    await File.CopyToAsync(fs);
                }
            }
            else
            {
                result = "There were problems with your submission";
            }


            return RedirectToAction("Index", "Home", new { result, errors });

        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
