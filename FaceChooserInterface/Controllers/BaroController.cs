using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FaceChooserInterface.Controllers
{
    [Produces("application/json")]
    [Route("api/Baro")]
    public class BaroController : Controller
    {

        private IConfiguration _config;
        private List<IConfigurationSection> BaroServerVariables;
        private string DownloadableZipsLocation;

        public BaroController(IConfiguration config)
        {
            _config = config;
            BaroServerVariables = _config.GetSection("BaroServerVariables").GetChildren().ToList();
#if DEBUG
            DownloadableZipsLocation = BaroServerVariables.Where(k => k.Key == "DownloadableFilesLocationDebug").First().Value;
#else
            DownloadableZipsLocation = BaroServerVariables.Where(k => k.Key == "DownloadableFilesLocationProd").First().Value;
#endif
        }

        [Route("Download")]
        [HttpGet]
        public string GetDownloadableList()
        {
            var ext = new List<string> { ".zip" };
            var files = Directory.GetFiles(DownloadableZipsLocation).Where(f => ext.Contains(Path.GetExtension(f))).ToList();
            string returnOptions = string.Join("|", files);
            return returnOptions;
        }

        [Route("DownloadOption")]
        [HttpGet()]
        public async Task<IActionResult> Download([FromQuery(Name = "download")] string zipFile)
        {
            string zipLoc = DownloadableZipsLocation + "\\" + zipFile;
            ;
            using (FileStream fs = System.IO.File.OpenRead(zipLoc))
            {
                var returnData = await File(fs, "application/octet-stream");
                return returnData;
            }
        }

    }
}