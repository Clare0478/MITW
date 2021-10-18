using FHIR_Demo.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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


        public ActionResult New_Patient()
        {
            var settings = new FhirClientSettings
            {
                Timeout = 120,
                PreferredFormat = ResourceFormat.Json,
                VerifyFhirVersion = true,
            };

            var client = new FhirClient("https://hapi.fhir.tw/fhir", settings);
            var bundle = client.Search<Patient>(null);
            List<Patient> Patients = new List<Patient>();
            foreach (var entry in bundle.Entry)
            {
                Patients.Add((Patient)entry.Resource);
            }
            return View(Patients);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult New_Patient(PatientViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var settings = new FhirClientSettings
        //        {
        //            Timeout = 12000,
        //            PreferredFormat = ResourceFormat.Json,
        //            VerifyFhirVersion = true,
        //        };

        //        var client = new FhirClient(URL, settings);

        //        Patient patient = new Patient()
        //        {
        //            Name = new List<HumanName>()
        //            {
        //                new HumanName()
        //                {
        //                    Text = model.name,
        //                    Given = new List<string>
        //                    {
        //                        model.name,
        //                    }
        //                }
        //            },
        //            BirthDate = model.birthDate,
        //            Gender = (AdministrativeGender)model.Gender,
        //            Identifier = new List<Identifier> {
        //                new Identifier
        //                {
        //                    Value = model.identifier
        //                }
        //            },
        //            Telecom = new List<ContactPoint>
        //            {
        //                new ContactPoint
        //                {
        //                    System = ContactPoint.ContactPointSystem.Phone,
        //                    Value = model.telecom
        //                },
        //                new ContactPoint
        //                {
        //                    System = ContactPoint.ContactPointSystem.Email,
        //                    Value = model.email
        //                },
        //            },
        //            Address = new List<Address>
        //            {
        //                new Address
        //                {
        //                    Text = model.address
        //                }
        //            },
        //            Contact = new List<Patient.ContactComponent>
        //            {
        //                new Patient.ContactComponent
        //                {
        //                    Name = new HumanName()
        //                    {
        //                        Text = model.contact_name,
        //                        Given = new List<string>
        //                        {
        //                            model.contact_name,
        //                        }
        //                    },
        //                    Relationship = new List<CodeableConcept>
        //                    {
        //                        new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0131", "N", model.contact_relationship)
        //                    },
        //                    Telecom = new List<ContactPoint>
        //                    {
        //                        new ContactPoint
        //                        {
        //                            System = ContactPoint.ContactPointSystem.Phone,
        //                            Value = model.contact_telecom
        //                        },
        //                    }

        //                },
        //            }

        //        };

        //        var conditions = new SearchParams();
        //        conditions.Add("identifier", model.identifier);

        //        var patient_ToJson = patient.ToJson();
        //        var created_pat_A = client.Create<Patient>(patient, conditions);
        //    }
        //    return View(model);
        //}


    }
}