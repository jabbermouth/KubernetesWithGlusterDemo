using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KubernetesWithGlusterDemo.Web.Models;
using System.IO;

namespace KubernetesWithGlusterDemo.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string sharedFilePath = "/shared/file.txt";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Route("")]
        public IActionResult Index()
        {
            return View(FetchStats());
        }

        [Route("privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("removefile")]
        public IActionResult RemoveFile()
        {
            if (System.IO.File.Exists(sharedFilePath))
            {
                System.IO.File.Delete(sharedFilePath);
            }

            return RedirectToAction("Index");
        }

        [Route("makeunhealthy")]
        public IActionResult MakeUnhealthy()
        {
            string healthOkFile = Path.Combine(Directory.GetCurrentDirectory(), "healthok.txt");
            if (System.IO.File.Exists(healthOkFile))
            {
                System.IO.File.Delete(healthOkFile);
            }

            return RedirectToAction("Index");
        }

        private IEnumerable<KeyValuePair<string, string>> FetchStats()
        {
            var dataToWrite = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            try
            {
                if (System.IO.File.Exists(sharedFilePath))
                {
                    // Open the text file using a stream reader.
                    using (StreamReader sr = new StreamReader(sharedFilePath))
                    {
                        // Read the stream to a string, and write the string to the console.
                        dataToWrite = sr.ReadToEnd();
                    }
                }
                else
                {
                    if (!Directory.Exists("/shared"))
                    {
                        Directory.CreateDirectory("/shared");
                    }
                    // Write the string array to a new file named "WriteLines.txt".
                    using (StreamWriter outputFile = new StreamWriter(sharedFilePath))
                    {
                        outputFile.WriteLine(dataToWrite);
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            yield return new KeyValuePair<string, string>("Stored Data", dataToWrite);

            yield return new KeyValuePair<string, string>("Pod Name", Environment.MachineName);
            yield return new KeyValuePair<string, string>("Path", Request.Path);
            yield return new KeyValuePair<string, string>("PathBase", Request.PathBase);
            yield return new KeyValuePair<string, string>("QueryString", Request.QueryString.ToString());

            yield return new KeyValuePair<string, string>("Local Port", Request.HttpContext.Connection.LocalPort.ToString());
            yield return new KeyValuePair<string, string>("Local IP", Request.HttpContext.Connection.LocalIpAddress.ToString());
            yield return new KeyValuePair<string, string>("Remove Port", Request.HttpContext.Connection.RemotePort.ToString());
            yield return new KeyValuePair<string, string>("Remote IP", Request.HttpContext.Connection.RemoteIpAddress.ToString());

            foreach (var header in Request.Headers)
            {
                yield return new KeyValuePair<string, string>(header.Key, header.Value);
            }
        }
    }
}
