using FHIR_Demo.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;
using static Hl7.Fhir.Model.Identifier;

namespace FHIR_Demo.Controllers
{
    public class CreateResourceController : Controller
    {
        private CookiesController cookies = new CookiesController();
        private HttpClientEventHandler handler = new HttpClientEventHandler();
        private FHIRResourceModel db = new FHIRResourceModel();



        // GET: CreateResource
        public ActionResult Index()
        {
            handler.ServerCertificateCustomValidationCallback += (sender2, cert, chain, sslPolicyErrors) => true;
            if (cookies.FHIR_Server_Cookie(HttpContext) == "IBM")
            {
                //使用Basic 登入
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(cookies.FHIR_Token_Cookie(HttpContext))));
                };
            }
            else
            {
                handler.OnBeforeRequest += (sender, e) =>
                {
                    e.RawRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cookies.FHIR_Token_Cookie(HttpContext));
                };
            }
            FhirClient client = new FhirClient(cookies.FHIR_URL_Cookie(HttpContext), cookies.settings, handler);
            ViewBag.status = TempData["status"];
            ViewBag.Error = TempData["Error"];
            try
            {
                cookies.Init_Cookie(HttpContext);

                //設定Server選單
                var Server_selectList = new List<SelectListItem>()
            {
                new SelectListItem {Text="IBM", Value="IBM" },
                new SelectListItem {Text="HAPi", Value="HAPi" },
                new SelectListItem {Text="Smart on Fhir", Value="Smart" },
            };
                //預設選擇哪一筆
                Server_selectList.Where(q => q.Value == cookies.FHIR_Server_Cookie(HttpContext)).First().Selected = true;
                ViewBag.Server_selectList = Server_selectList;

                //下拉ResourceType
                var resourceTypes = db.ResourceInfos.Select(r => r.ResourceType).Distinct().ToList();
                if (resourceTypes.Contains("Patient"))
                {
                    resourceTypes.Remove("Patient");
                    resourceTypes.Insert(0, "Patient");
                }
                ViewBag.ResourceTypes = resourceTypes;

                return View();
            }
            catch (Exception e)
            {
                ViewBag.Error = "發生錯誤";
                return View();
            }
        }


        public ActionResult Create(string resourceType)
        {
            List<resourceinfo> resourceinfos = db.ResourceInfos
            .Where(c => c.ResourceType == resourceType)
            .ToList();

            List<datatypes> datatypes = db.datatypes
                .ToList();

            // resourceinfos 轉換成 JSON 格式
            var resourceInfoJson = JsonConvert.SerializeObject(resourceinfos);
            ViewBag.ResourceInfo = resourceInfoJson;

            //datatype轉json資料
            var Datatypes = JsonConvert.SerializeObject(datatypes);
            ViewBag.Datatype = Datatypes;

            //性別下拉
            ViewBag.GenderOptions = Enum.GetValues(typeof(Gender))
                .Cast<Gender>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
                });

            return View();

        }


        [HttpPost]
        //public ActionResult Create(Dictionary<string, List<string>> dataToSend)
        public ActionResult Create(string resourceType, List<Dictionary<string, string>> jsonData)
        {
            //// 解析 JSON 
            Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();

            foreach (var item in jsonData)
            {
                var fieldName = item["fieldName"];
                var value = item["value"];

                if (!data.ContainsKey(fieldName))
                {
                    data[fieldName] = new List<string>();
                }

                data[fieldName].Add(value);
            }

            string startstr = "";
            for(int i=0; i < data.Count(); i++)
            {
                var itemdata = data.Keys.ToArray()[i];
                if (itemdata.Contains('.') && startstr == "")
                {
                    int pointnum = itemdata.IndexOf('.');
                    startstr = itemdata.Substring(0, pointnum);
                }
            }

            Resource resource = CreateResourcePromData(resourceType, data);

            return View();
        }

        private Resource CreateResourcePromData(string resourceType, Dictionary<string, List<string>> data)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Hl7.Fhir.Model.Patient));
            Type resourceObjectType = assembly.GetType("Hl7.Fhir.Model.Patient");


            //Type resourceObjectType2 = Type.GetType("Hl7.Fhir.Model.Patient"); Module = {Hl7.Fhir.R4.Core.dll}
            if (resourceObjectType == null)
            {
                return null;
            }

            //抓到此資源的所含欄位
            Resource resource = (Resource)Activator.CreateInstance(resourceObjectType);

            //  巡identifierData 創建 Identifier 
            //foreach (var firstPart in identifierData.Keys)
            //{
            //    if (resourceObjectType.GetProperty(firstPart) != null)
            //    {
            //        Identifier identifier = new Identifier();

            //        if (identifierData[firstPart].ContainsKey("use") && identifierData[firstPart]["use"].Count > 0)
            //        {
            //            if (Enum.TryParse(identifierData[firstPart]["use"][0], out IdentifierUse useValue))
            //            {
            //                identifier.Use = useValue;
            //            }
            //        }

            //        resourceObjectType.GetProperty(firstPart).SetValue(resource, identifier);
            //    }
            //}

            foreach (var dataEntry in data)
            {
                var fieldName = dataEntry.Key;
                var values = dataEntry.Value;
                int pointnum = fieldName.IndexOf('.');

                if (values.Any(v => !string.IsNullOrEmpty(v)))//判斷哪個input有輸入值
                {
                    //判斷是否包含.
                    string[] fieldParts = fieldName.Split('.');
                    string firstPart = fieldParts[0];
                    string secondPart = null;

                    if (fieldParts.Length > 1)
                    {
                        secondPart = fieldParts[1];
                    }

                    firstPart = char.ToUpper(firstPart[0]) + firstPart.Substring(1);//開頭變大寫

                    // 在資源類型中尋找與第一層資料firstPart相符的屬性
                    PropertyInfo property = resourceObjectType.GetProperty(firstPart);

                    if (property != null)
                    {
                        Type propertyType = property.PropertyType;//判別此欄位型別



                        //抓到目前資料的值
                        object propertyValue = property.GetValue(resource);

                        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>) &&
                            propertyType.GetGenericArguments().Length == 1 &&
                            propertyType.GetGenericArguments()[0] == typeof(Identifier))
                        {

                            List<Identifier> identifierList = new List<Identifier>();
                            Identifier identifier = new Identifier();
                            //Identifier identifier = (Identifier)propertyValue;

                            if (secondPart == "use" && values.Count > 0)
                            {
                                if (Enum.TryParse(values[0], out IdentifierUse useValue))
                                {
                                    identifier.Use = useValue;
                                }
                            }
                            else if (secondPart == "system" && values.Count > 0)
                            {
                                identifier.System = values[0];
                            }
                            else if (secondPart == "value" && values.Count > 0)
                            {
                                identifier.Value = values[0];
                            }

                            identifierList.Add(identifier);
                            property.SetValue(resource, identifierList);
                        }
                        else if (propertyType == typeof(List<string>))
                        {
                            List<string> stringList = (List<string>)propertyValue;
                            stringList.AddRange(values);
                        }
                        else if (propertyType == typeof(bool?) && values.Count > 0)
                        {
                            bool boolValue;
                            if (bool.TryParse(values[0], out boolValue))
                            {
                                property.SetValue(resource, (bool?)boolValue);
                            }
                        }
                        //else
                        //{
                        //    if (values.Count > 0)
                        //    {
                        //        object convertedValue = Convert.ChangeType(values[0], propertyType);
                        //        property.SetValue(resource, convertedValue);
                        //    }
                        //}
                    }
                }
            }

            return resource;
        }

        private Dictionary<string, Dictionary<string, List<string>>> ProcessHierarchicalData(Dictionary<string, List<string>> data)
        {
            Dictionary<string, Dictionary<string, List<string>>> identifierData = new Dictionary<string, Dictionary<string, List<string>>>();

            foreach (var dataEntry in data)
            {
                var fieldName = dataEntry.Key;
                var values = dataEntry.Value;
                int pointnum = fieldName.IndexOf('.');

                if (values.Any(v => !string.IsNullOrEmpty(v)))
                {
                    string[] fieldParts = fieldName.Split('.');
                    string firstPart = fieldParts[0];
                    string secondPart = null;

                    if (fieldParts.Length > 1)
                    {
                        secondPart = fieldParts[1];
                    }

                    firstPart = char.ToUpper(firstPart[0]) + firstPart.Substring(1);

                    if (!identifierData.ContainsKey(firstPart))
                    {
                        identifierData[firstPart] = new Dictionary<string, List<string>>();
                    }

                    if (secondPart != null && !identifierData[firstPart].ContainsKey(secondPart))
                    {
                        identifierData[firstPart][secondPart] = new List<string>();
                    }

                    identifierData[firstPart][secondPart].AddRange(values);
                }
            }

            return identifierData;
        }

        [HttpPost]
        public ActionResult GenerateFields(string selectedResourceType)
        {
            List<resourceinfo> resourceinfos = db.ResourceInfos
                .Where(c => c.ResourceType == selectedResourceType)
                .ToList();

            // resourceinfos 轉換成 JSON 格式
            var resourceInfoJson = JsonConvert.SerializeObject(resourceinfos);
            ViewBag.ResourceInfo = resourceInfoJson;

            return Json(resourceInfoJson);
        }
    }
} 