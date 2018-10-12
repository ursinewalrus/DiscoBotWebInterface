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
        private List<IConfigurationSection> DiscoServerVariables;
        private string Pw;
        private int MaxFiles;
        private int MaxFileSize;
        private string DlDir;

        public HomeController(IConfiguration config)
        {
            _config = config;
            DiscoServerVariables = _config.GetSection("DiscoServerVariables").GetChildren().ToList();
#if DEBUG
            DlDir = DiscoServerVariables.Where(k => k.Key == "UploadPathDebug").First().Value;
#else
            DlDir = DiscoServerVariables.Where(k => k.Key == "UploadPathProd").First().Value;
#endif
            Pw = DiscoServerVariables.Where(k => k.Key == "SubmitPW").First().Value;
            MaxFiles = Convert.ToInt32(DiscoServerVariables.Where(k => k.Key == "MaxFiles").First().Value);
            MaxFileSize = Convert.ToInt32(DiscoServerVariables.Where(k => k.Key == "MaxFileSizeInBytes").First().Value);


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

            var errors = "";


            //enforce no spaces or arguments for triggering discobot will have issues

            if (Pw != password)
            {
                errors += "Wrong password\n";
            }

            var UploadDirContents = Directory.GetFiles(DlDir);


            if (UploadDirContents.Length >= MaxFiles)
            {
                errors += "To many uploads, yell at KK(don't)\n";
            }
            if(File.FileName.Split(' ').Length > 1)
            {
                errors += "No spaces in the file name, yell at Discord";
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

            if (File.Length > MaxFileSize)
            {
                errors += "File too large - 400 kb or less\n";
            }

            }

            var result = "";

            if (errors == "")
            {
                result = "Upload success";
                var dlLocation = Path.Combine(DlDir, File.FileName);
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
