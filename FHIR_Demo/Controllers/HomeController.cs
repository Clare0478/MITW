//using FHIR_Demo.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace FHIR_Demo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string url) 
        {
            var settings = new FhirClientSettings
            {
                Timeout = 120,
                PreferredFormat = ResourceFormat.Json,
                VerifyFhirVersion = true,
            };

            var client = new FhirClient(url, settings);
            var q = new SearchParams().LimitTo(20);
            Bundle results = client.Search<Patient>(q);
            return Content(results.ToJson());
        }

        
    }
}