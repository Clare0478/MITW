using FHIR_Demo.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace FHIR_Demo.Controllers
{
    public class MHBController : Controller
    {
        // GET: MHB
        public ActionResult Index()
        {
            return View();
        }
        //解析健康存摺XML檔案
        //儲存健康存摺內容陣列
        Bundle_MHB Bun = new Bundle_MHB();
        Patient_MHB Pat = new Patient_MHB();
        Composition_MHB Comp = new Composition_MHB();
        Encounter_MHB Enc = new Encounter_MHB();
        Condition_MHB Con = new Condition_MHB();
        Observation_MHB Obs = new Observation_MHB();
        Procedure_MHB Pro = new Procedure_MHB();
        MedicationRequest_MHB Med = new MedicationRequest_MHB();
        AllergyIntolerance_MHB Allergy = new AllergyIntolerance_MHB();
        Immunization_MHB Immun = new Immunization_MHB();
        Coverage_MHB Cov = new Coverage_MHB();
        Organization_MHB Org = new Organization_MHB();

        //list 用來儲存所有資料的
        //List<Patient> patlist = new List<Patient>();
        List<Composition_MHB> complist = new List<Composition_MHB>();
        List<Encounter_MHB> enclist = new List<Encounter_MHB>();
        List<Condition_MHB> conlist = new List<Condition_MHB>();
        List<Observation_MHB> obslist = new List<Observation_MHB>();
        List<Procedure_MHB> prolist = new List<Procedure_MHB>();
        List<MedicationRequest_MHB> medlist = new List<MedicationRequest_MHB>();
        List<AllergyIntolerance_MHB> allergylist = new List<AllergyIntolerance_MHB>();
        List<Immunization_MHB> immlist = new List<Immunization_MHB>();
        List<Coverage_MHB> covlist = new List<Coverage_MHB>();
        List<Organization_MHB> orglist = new List<Organization_MHB>();

        string fhir = ""; //轉檔後fhir檔案名稱

        //解析健康存摺XML檔案
        public void XML_Path(string path)
        {
            if (path != null)
            {
                XmlDocument doc = new XmlDocument();
                //取得健康存摺的檔案位置
                //doc.Load(Server.MapPath("~/PHRUploads/健康存摺醫療類_1080613-CYHSU.xml"));  //Server.Mapath("~PHR....") 相對路徑
                //doc.Load(Server.MapPath("~/PHRUploads/willie's myhealthbank data_chinese.xml"));  //Server.Mapath("~PHR....") 相對路徑
                doc.Load(path);

                XML(doc);
            }
            //刪除上傳的Excel
            //System.IO.File.Delete(path);
        }

        //解析健康存摺XML string
        public void XML_String(string xml)
        {
            if (xml != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                XML(doc);
            }
        }

        public void XML(XmlDocument doc)
        {
            int num_id = 0; // id 設定變數

            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/myhealthbank/bdata");

            //取得身分證ID
            Bun.id = nodes[0].SelectSingleNode("b1.1").InnerText;

            //Pat.id = "1111111111";
            //健康存摺b1.1不是完整身分證(A1275*****)，產出的FHIR檔案會因為這個身分證的特殊符號而無法輸入
            Pat.id = nodes[0].SelectSingleNode("b1.1").InnerText;
            Bun.meta = nodes[0].SelectSingleNode("b1.2").InnerText.Substring(0, 4) + "-" + nodes[0].SelectSingleNode("b1.2").InnerText.Substring(4, 2) + "-" + nodes[0].SelectSingleNode("b1.2").InnerText.Substring(6, 2);
            //Pat.name = ((ClaimsIdentity)User.Identity).FindFirstValue("RealName");
            Pat.name = "";

            //國民健康保健署 組織資料
            Org.id = "MOHW";
            Org.name = "衛生福利部中央健康保險署(National Health Insurance Administration)";
            Org.type = "中央(Center)";
            Org.telecom = "+886 2 2706-5866";
            Org.address = "10634臺北市大安區信義路三段140號";
            orglist.Add(Org);

            //判斷性別
            if (Pat.id.Substring(1, 1) == "1")
            {
                Pat.gender = "male";
            }
            else if (Pat.id.Substring(1, 1) == "2")
            {
                Pat.gender = "female";
            }

            //取得r1資料
            nodes = doc.DocumentElement.SelectNodes("/myhealthbank/bdata/r1");

            //變數
            //時間變數
            string time_start = "";
            string time_end = "";

            //encounter 變數
            int com_num = 0;
            int en_reason_num = 0;
            int en_diag_num = 0;
            int obs_comp_num = 0;


            //暫存變數陣列，因List的陣列無法自由改變陣列長度，所以先用一般變數去儲存資料，在丟入List中
            var Enc_reasonReference = Enc.reasonReference;
            var Enc_diagnosis = Enc.diagnosis;
            var Comp_section_entry = Comp.section_entry;
            var Pat_valueDate = Pat.valueDate;
            var Pat_valuestring = Pat.valuestring;
            var Obs_com_code = Obs.com_code;
            var Obs_com_code_display = Obs.com_code_display;
            var Obs_valueQuantity = Obs.valueQuantity;
            var Obs_referenceRange_text = Obs.referenceRange_text;
            var Obs_interpretation = Obs.interpretation;

            //r1 資料擷取
            //foreach (XmlNode node in nodes)
            for (int i = 0; i < nodes.Count; i++)
            {
                //判斷r1是否有資料
                if (nodes[i].SelectSingleNode("r1.1") == null)
                {
                    break;
                }

                //時間重製
                time_start = "";
                time_end = "";

                //重製struct 資料
                Comp = new Composition_MHB();
                Enc = new Encounter_MHB();
                Con = new Condition_MHB();
                //Obs = new Observation();
                Pro = new Procedure_MHB();
                Med = new MedicationRequest_MHB();
                //Allergy = new AllergyIntolerance();
                //Immun = new Immunization();
                Cov = new Coverage_MHB();
                Org = new Organization_MHB();


                //判斷組織是否存在
                int org_yn = 0; //0代表有  1代表沒有
                for (int o_num = 0; o_num < orglist.Count; o_num++)
                {
                    if (orglist[o_num].id == nodes[i].SelectSingleNode("r1.3").InnerText)
                    {
                        Org.id = orglist[o_num].id;
                        org_yn = 1;
                        break;
                    }
                }
                if (org_yn == 0)
                {
                    Org.id = nodes[i].SelectSingleNode("r1.3").InnerText;
                    Org.type = nodes[i].SelectSingleNode("r1.2").InnerText;
                    Org.name = nodes[i].SelectSingleNode("r1.4").InnerText;
                    orglist.Add(Org);
                }

                //時間
                if (nodes[i].SelectSingleNode("r1.5").InnerText != "")
                {
                    time_start = nodes[i].SelectSingleNode("r1.5").InnerText;
                    time_end = nodes[i].SelectSingleNode("r1.5").InnerText;
                }
                else if (nodes[i].SelectSingleNode("r1.6").InnerText != "")
                {
                    time_start = nodes[i].SelectSingleNode("r1.6").InnerText;
                    time_end = nodes[i].SelectSingleNode("r1.6").InnerText;
                }
                time_start = time_start.Substring(0, 4) + "-" + time_start.Substring(4, 2) + "-" + time_start.Substring(6, 2);
                time_end = time_end.Substring(0, 4) + "-" + time_end.Substring(4, 2) + "-" + time_end.Substring(6, 2);

                //Composition 資料
                Comp.id = num_id.ToString();
                num_id++;
                Comp.title = "r1";
                Comp.patient = "Patient/" + Pat.id;
                Comp.section_entry = new string[0];

                //Encounter 資料
                Enc.id = num_id.ToString();
                num_id++;
                Enc.en_class = "ambulatory"; //門診 ambulatory  住院 inpatient encounter
                Enc.subject = "Patient/" + Pat.id;
                Enc.period_start = time_start;
                Enc.period_end = time_end;
                Enc.Organization = "Organization/" + Org.id;
                Enc.reasonReference = new string[0];
                Enc.diagnosis = new string[0];

                //陣列變數初始化
                com_num = 0;
                en_reason_num = 0;
                en_diag_num = 0;

                //Composition 資料
                Comp.encounter = "Encounter/" + Enc.id;
                Comp.date = time_end;
                Comp.author = "Organization/" + Org.id;


                //Conditions 資料
                if (nodes[i].SelectSingleNode("r1.8").InnerText != "")
                {

                    Con.id = num_id.ToString();
                    num_id++;
                    Con.code_code = nodes[i].SelectSingleNode("r1.8").InnerText;
                    Con.code_diplay = nodes[i].SelectSingleNode("r1.9").InnerText;
                    Con.subject = "Patient/" + Pat.id;
                    Con.encounter = "Encounter/" + Enc.id;
                    Con.recoredDate = time_end;
                    conlist.Add(Con);

                    //Encounter reference condition
                    en_reason_num++;
                    en_diag_num++;
                    Enc_reasonReference = Enc.reasonReference;
                    Enc_diagnosis = Enc.diagnosis;
                    Array.Resize(ref Enc_reasonReference, en_reason_num);
                    Array.Resize(ref Enc_diagnosis, en_diag_num);
                    Enc.reasonReference = Enc_reasonReference;
                    Enc.diagnosis = Enc_diagnosis;
                    Enc.reasonReference[en_reason_num - 1] = "Condition/" + Con.id;
                    Enc.diagnosis[en_diag_num - 1] = "Condition/" + Con.id;

                    //Composition reference condition
                    com_num++;
                    Comp_section_entry = Comp.section_entry;
                    Array.Resize(ref Comp_section_entry, com_num);
                    Comp.section_entry = Comp_section_entry;
                    Comp.section_entry[com_num - 1] = "Condition/" + Con.id;
                }

                //Procedure 資料
                if (nodes[i].SelectSingleNode("r1.10").InnerText != "")
                {
                    Pro.id = num_id.ToString();
                    num_id++;
                    Pro.code_code = nodes[i].SelectSingleNode("r1.10").InnerText;
                    Pro.code_diplay = nodes[i].SelectSingleNode("r1.11").InnerText;
                    Pro.subject = "Patient/" + Pat.id;
                    Pro.encounter = "Encounter/" + Enc.id;
                    Pro.performedPeriod_start = time_start;
                    Pro.performedPeriod_end = time_end;
                    Pro.actor = "Organization/" + Org.id;
                    Pro.reasonReference = "Condition/" + Con.id;
                    prolist.Add(Pro);

                    //Encounter reference Procedure
                    en_reason_num++;
                    en_diag_num++;
                    Enc_reasonReference = Enc.reasonReference;
                    Enc_diagnosis = Enc.diagnosis;
                    Array.Resize(ref Enc_reasonReference, en_reason_num);
                    Array.Resize(ref Enc_diagnosis, en_diag_num);
                    Enc.reasonReference = Enc_reasonReference;
                    Enc.diagnosis = Enc_diagnosis;
                    Enc.reasonReference[en_reason_num - 1] = "Procedure/" + Pro.id;
                    Enc.diagnosis[en_diag_num - 1] = "Procedure/" + Pro.id;

                    //Composition reference Procedure
                    com_num++;
                    Comp_section_entry = Comp.section_entry;
                    Array.Resize(ref Comp_section_entry, com_num);
                    Comp.section_entry = Comp_section_entry;
                    Comp.section_entry[com_num - 1] = "Procedure/" + Pro.id;
                }
                XmlNodeList r1_1_nodes = nodes[i].SelectNodes("r1_1");
                //藥物與處置
                for (int j = 0; j < r1_1_nodes.Count; j++)
                {
                    //數字開頭 處置 
                    if (System.Text.RegularExpressions.Regex.IsMatch(r1_1_nodes[j].SelectSingleNode("r1_1.1").InnerText, @"^[0-9]"))
                    {
                        Pro = new Procedure_MHB();
                        Pro.id = num_id.ToString();
                        num_id++;
                        Pro.code_code = r1_1_nodes[j].SelectSingleNode("r1_1.1").InnerText;
                        Pro.code_diplay = r1_1_nodes[j].SelectSingleNode("r1_1.2").InnerText;
                        Pro.subject = "Patient/" + Pat.id;
                        Pro.encounter = "Encounter/" + Enc.id;
                        Pro.performedPeriod_start = time_start;
                        Pro.performedPeriod_end = time_end;
                        Pro.actor = "Organization/" + Org.id;
                        Pro.reasonReference = "Condition/" + Con.id;
                        prolist.Add(Pro);

                        //Encounter reference Procedure
                        en_reason_num++;
                        en_diag_num++;
                        Enc_reasonReference = Enc.reasonReference;
                        Enc_diagnosis = Enc.diagnosis;
                        Array.Resize(ref Enc_reasonReference, en_reason_num);
                        Array.Resize(ref Enc_diagnosis, en_diag_num);
                        Enc.reasonReference = Enc_reasonReference;
                        Enc.diagnosis = Enc_diagnosis;
                        Enc.reasonReference[en_reason_num - 1] = "Procedure/" + Pro.id;
                        Enc.diagnosis[en_diag_num - 1] = "Procedure/" + Pro.id;

                        //Composition reference Procedure
                        com_num++;
                        Comp_section_entry = Comp.section_entry;
                        Array.Resize(ref Comp_section_entry, com_num);
                        Comp.section_entry = Comp_section_entry;
                        Comp.section_entry[com_num - 1] = "Procedure/" + Pro.id;
                    }
                    //字母開頭 藥物
                    else if (System.Text.RegularExpressions.Regex.IsMatch(r1_1_nodes[j].SelectSingleNode("r1_1.1").InnerText, @"^[a-zA-Z]"))
                    {
                        Med = new MedicationRequest_MHB();
                        //Medecation 藥品名 
                        Med.Medication_id = r1_1_nodes[j].SelectSingleNode("r1_1.1").InnerText;
                        Med.Medication_code = r1_1_nodes[j].SelectSingleNode("r1_1.1").InnerText;
                        Med.Medication_display = r1_1_nodes[j].SelectSingleNode("r1_1.2").InnerText;

                        //Request 藥囑處方
                        Med.id = num_id.ToString();
                        num_id++;
                        Med.medicationReference = "#" + Med.Medication_id;
                        Med.subject = "Patient/" + Pat.id;
                        Med.encounter = "Encounter/" + Enc.id;
                        Med.authoredOn = time_start;
                        Med.quantity = r1_1_nodes[j].SelectSingleNode("r1_1.3").InnerText;
                        Med.expectedSupplyDuration = r1_1_nodes[j].SelectSingleNode("r1_1.4").InnerText;
                        medlist.Add(Med);

                        //Composition reference Procedure
                        com_num++;
                        Comp_section_entry = Comp.section_entry;
                        Array.Resize(ref Comp_section_entry, com_num);
                        Comp.section_entry = Comp_section_entry;
                        Comp.section_entry[com_num - 1] = "MedicationRequest/" + Med.id;
                    }
                }

                //Coverage 資料
                //部分負擔(自付)
                Cov.id = num_id.ToString();
                num_id++;
                Cov.subscriber = "Patient/" + Pat.id;
                Cov.beneficiary = "Patient/" + Pat.id;
                Cov.period_start = time_start;
                Cov.period_end = time_end;
                Cov.payor = "Patient/" + Pat.id;
                Cov.valueMoney = nodes[i].SelectSingleNode("r1.12").InnerText;
                covlist.Add(Cov);

                //Composition reference Procedure
                com_num++;
                Comp_section_entry = Comp.section_entry;
                Array.Resize(ref Comp_section_entry, com_num);
                Comp.section_entry = Comp_section_entry;
                Comp.section_entry[com_num - 1] = "Coverage/" + Cov.id;

                //健保給付
                Cov = new Coverage_MHB();
                Cov.id = num_id.ToString();
                num_id++;
                Cov.subscriber = "Patient/" + Pat.id;
                Cov.beneficiary = "Patient/" + Pat.id;
                Cov.period_start = time_start;
                Cov.period_end = time_end;
                Cov.payor = "Organization/MOHW";
                Cov.valueMoney = nodes[i].SelectSingleNode("r1.13").InnerText;
                covlist.Add(Cov);

                //Encounter
                enclist.Add(Enc);

                //Composition reference Coverage
                com_num++;
                Comp_section_entry = Comp.section_entry;
                Array.Resize(ref Comp_section_entry, com_num);
                Comp.section_entry = Comp_section_entry;
                Comp.section_entry[com_num - 1] = "Coverage/" + Cov.id;
                complist.Add(Comp);
            }

            //取得r2資料
            nodes = doc.DocumentElement.SelectNodes("/myhealthbank/bdata/r2");

            //encounter 變數
            com_num = 0;
            en_reason_num = 0;
            en_diag_num = 0;
            //int en_type_num = 0;

            //r2 資料擷取
            //foreach (XmlNode node in nodes)
            for (int i = 0; i < nodes.Count; i++)
            {
                //判斷r2是否有資料
                if (nodes[i].SelectSingleNode("r2.1") == null)
                {
                    break;
                }

                //時間重製
                time_start = "";
                time_end = "";

                //重製struct 資料
                Comp = new Composition_MHB();
                Enc = new Encounter_MHB();
                Con = new Condition_MHB();
                //Obs = new Observation();
                Pro = new Procedure_MHB();
                Med = new MedicationRequest_MHB();
                //Allergy = new AllergyIntolerance();
                //Immun = new Immunization();
                Cov = new Coverage_MHB();
                Org = new Organization_MHB();


                //判斷組織是否存在
                int org_yn = 0; //0代表有  1代表沒有
                for (int o_num = 0; o_num < orglist.Count; o_num++)
                {
                    if (orglist[o_num].id == nodes[i].SelectSingleNode("r2.3").InnerText)
                    {
                        Org.id = orglist[o_num].id;
                        org_yn = 1;
                        break;
                    }
                }
                if (org_yn == 0)
                {
                    Org.id = nodes[i].SelectSingleNode("r2.3").InnerText;
                    Org.type = nodes[i].SelectSingleNode("r2.2").InnerText;
                    Org.name = nodes[i].SelectSingleNode("r2.4").InnerText;
                    orglist.Add(Org);
                }

                //時間
                time_start = nodes[i].SelectSingleNode("r2.5").InnerText;
                time_end = nodes[i].SelectSingleNode("r2.6").InnerText;
                if (time_start == "")
                {
                    time_start = time_end;
                }
                else if (time_end == "")
                {
                    time_end = time_start;
                }
                time_start = time_start.Substring(0, 4) + "-" + time_start.Substring(4, 2) + "-" + time_start.Substring(6, 2);
                time_end = time_end.Substring(0, 4) + "-" + time_end.Substring(4, 2) + "-" + time_end.Substring(6, 2);

                //Composition 資料
                Comp.id = num_id.ToString();
                num_id++;
                Comp.title = "r2";
                Comp.patient = "Patient/" + Pat.id;
                Comp.section_entry = new string[0];

                //Encounter 資料
                Enc.id = num_id.ToString();
                num_id++;
                Enc.en_class = "inpatient encounter"; //門診 ambulatory  住院 inpatient encounter
                Enc.subject = "Patient/" + Pat.id;
                Enc.period_start = time_start;
                Enc.period_end = time_end;
                Enc.Organization = "Organization/" + Org.id;
                Enc.reasonReference = new string[0];
                Enc.diagnosis = new string[0];
                Enc.type_code = new string[0];  //r2特定
                Enc.type_display = new string[0]; //r2特定

                //陣列變數初始化
                com_num = 0;
                en_reason_num = 0;
                en_diag_num = 0;
                //en_type_num = 0;

                //Composition 資料
                Comp.encounter = "Encounter/" + Enc.id;
                Comp.date = time_end;
                Comp.author = "Organization/" + Org.id;


                //Conditions 資料
                if (nodes[i].SelectSingleNode("r2.10").InnerText != "")
                {
                    Con.id = num_id.ToString();
                    num_id++;
                    Con.code_code = nodes[i].SelectSingleNode("r2.10").InnerText;
                    Con.code_diplay = nodes[i].SelectSingleNode("r2.11").InnerText;
                    Con.subject = "Patient/" + Pat.id;
                    Con.encounter = "Encounter/" + Enc.id;
                    Con.recoredDate = time_end;
                    conlist.Add(Con);

                    //Encounter reference condition
                    en_reason_num++;
                    en_diag_num++;
                    Enc_reasonReference = Enc.reasonReference;
                    Enc_diagnosis = Enc.diagnosis;
                    Array.Resize(ref Enc_reasonReference, en_reason_num);
                    Array.Resize(ref Enc_diagnosis, en_diag_num);
                    Enc.reasonReference = Enc_reasonReference;
                    Enc.diagnosis = Enc_diagnosis;
                    Enc.reasonReference[en_reason_num - 1] = "Condition/" + Con.id;
                    Enc.diagnosis[en_diag_num - 1] = "Condition/" + Con.id;

                    //Composition reference condition
                    com_num++;
                    Comp_section_entry = Comp.section_entry;
                    Array.Resize(ref Comp_section_entry, com_num);
                    Comp.section_entry = Comp_section_entry;
                    Comp.section_entry[com_num - 1] = "Condition/" + Con.id;
                }

                //Procedure 資料
                if (nodes[i].SelectSingleNode("r2.12").InnerText != "")
                {
                    Pro.id = num_id.ToString();
                    num_id++;
                    Pro.code_code = nodes[i].SelectSingleNode("r2.12").InnerText;
                    Pro.code_diplay = nodes[i].SelectSingleNode("r2.13").InnerText;
                    Pro.subject = "Patient/" + Pat.id;
                    Pro.encounter = "Encounter/" + Enc.id;
                    Pro.performedPeriod_start = time_start;
                    Pro.performedPeriod_end = time_end;
                    Pro.actor = "Organization/" + Org.id;
                    Pro.reasonReference = "Condition/" + Con.id;
                    prolist.Add(Pro);

                    //Encounter reference Procedure
                    en_reason_num++;
                    en_diag_num++;
                    Enc_reasonReference = Enc.reasonReference;
                    Enc_diagnosis = Enc.diagnosis;
                    Array.Resize(ref Enc_reasonReference, en_reason_num);
                    Array.Resize(ref Enc_diagnosis, en_diag_num);
                    Enc.reasonReference = Enc_reasonReference;
                    Enc.diagnosis = Enc_diagnosis;
                    Enc.reasonReference[en_reason_num - 1] = "Procedure/" + Pro.id;
                    Enc.diagnosis[en_diag_num - 1] = "Procedure/" + Pro.id;

                    //Composition reference Procedure
                    com_num++;
                    Comp_section_entry = Comp.section_entry;
                    Array.Resize(ref Comp_section_entry, com_num);
                    Comp.section_entry = Comp_section_entry;
                    Comp.section_entry[com_num - 1] = "Procedure/" + Pro.id;
                }
                XmlNodeList r2_1_nodes = nodes[i].SelectNodes("r2_1");
                //藥物與處置
                for (int j = 0; j < r2_1_nodes.Count; j++)
                {
                    //數字開頭 處置 放到encounter type  因為是住院
                    if (System.Text.RegularExpressions.Regex.IsMatch(r2_1_nodes[j].SelectSingleNode("r2_1.4").InnerText, @"^[0-9]"))
                    {
                        Pro = new Procedure_MHB();
                        Pro.id = num_id.ToString();
                        num_id++;
                        Pro.code_code = r2_1_nodes[j].SelectSingleNode("r2_1.4").InnerText;
                        Pro.code_diplay = r2_1_nodes[j].SelectSingleNode("r2_1.5").InnerText;
                        Pro.subject = "Patient/" + Pat.id;
                        Pro.encounter = "Encounter/" + Enc.id;
                        Pro.performedPeriod_start = DateTime.ParseExact(r2_1_nodes[j].SelectSingleNode("r2_1.2").InnerText, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                        Pro.performedPeriod_end = DateTime.ParseExact(r2_1_nodes[j].SelectSingleNode("r2_1.3").InnerText, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                        Pro.actor = "Organization/" + Org.id;
                        Pro.reasonReference = "Condition/" + Con.id;
                        prolist.Add(Pro);

                        //Encounter reference Procedure
                        en_reason_num++;
                        en_diag_num++;
                        Enc_reasonReference = Enc.reasonReference;
                        Enc_diagnosis = Enc.diagnosis;
                        Array.Resize(ref Enc_reasonReference, en_reason_num);
                        Array.Resize(ref Enc_diagnosis, en_diag_num);
                        Enc.reasonReference = Enc_reasonReference;
                        Enc.diagnosis = Enc_diagnosis;
                        Enc.reasonReference[en_reason_num - 1] = "Procedure/" + Pro.id;
                        Enc.diagnosis[en_diag_num - 1] = "Procedure/" + Pro.id;

                        //Composition reference Procedure
                        com_num++;
                        Comp_section_entry = Comp.section_entry;
                        Array.Resize(ref Comp_section_entry, com_num);
                        Comp.section_entry = Comp_section_entry;
                        Comp.section_entry[com_num - 1] = "Procedure/" + Pro.id;
                    }
                    //字母開頭 藥物
                    else if (System.Text.RegularExpressions.Regex.IsMatch(r2_1_nodes[j].SelectSingleNode("r2_1.4").InnerText, @"^[a-zA-Z]"))
                    {
                        Med = new MedicationRequest_MHB();
                        //Medecation 藥品名 
                        Med.Medication_id = r2_1_nodes[j].SelectSingleNode("r2_1.4").InnerText;
                        Med.Medication_code = r2_1_nodes[j].SelectSingleNode("r2_1.4").InnerText;
                        Med.Medication_display = r2_1_nodes[j].SelectSingleNode("r2_1.5").InnerText;

                        //Request 藥囑處方
                        Med.id = num_id.ToString();
                        num_id++;
                        Med.medicationReference = "#" + Med.Medication_id;
                        Med.subject = "Patient/" + Pat.id;
                        Med.encounter = "Encounter/" + Enc.id;
                        Med.authoredOn = DateTime.ParseExact(r2_1_nodes[j].SelectSingleNode("r2_1.2").InnerText, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"); ;
                        Med.quantity = r2_1_nodes[j].SelectSingleNode("r2_1.6").InnerText;
                        Med.expectedSupplyDuration = "1";//住院給藥以天
                        medlist.Add(Med);

                        //Composition reference Procedure
                        com_num++;
                        Comp_section_entry = Comp.section_entry;
                        Array.Resize(ref Comp_section_entry, com_num);
                        Comp.section_entry = Comp_section_entry;
                        Comp.section_entry[com_num - 1] = "MedicationRequest/" + Med.id;
                    }
                }

                //Coverage 資料
                //部分負擔(自付)
                Cov.id = num_id.ToString();
                num_id++;
                Cov.subscriber = "Patient/" + Pat.id;
                Cov.beneficiary = "Patient/" + Pat.id;
                Cov.period_start = time_start;
                Cov.period_end = time_end;
                Cov.payor = "Patient/" + Pat.id;
                Cov.valueMoney = nodes[i].SelectSingleNode("r2.14").InnerText;
                covlist.Add(Cov);

                //Composition reference Procedure
                com_num++;
                Comp_section_entry = Comp.section_entry;
                Array.Resize(ref Comp_section_entry, com_num);
                Comp.section_entry = Comp_section_entry;
                Comp.section_entry[com_num - 1] = "Coverage/" + Cov.id;

                //健保給付
                Cov = new Coverage_MHB();
                Cov.id = num_id.ToString();
                num_id++;
                Cov.subscriber = "Patient/" + Pat.id;
                Cov.beneficiary = "Patient/" + Pat.id;
                Cov.period_start = time_start;
                Cov.period_end = time_end;
                Cov.payor = "Organization/MOHW";
                Cov.valueMoney = nodes[i].SelectSingleNode("r2.15").InnerText;
                covlist.Add(Cov);

                //Encounter
                enclist.Add(Enc);

                //Composition reference Coverage
                com_num++;
                Comp_section_entry = Comp.section_entry;
                Array.Resize(ref Comp_section_entry, com_num);
                Comp.section_entry = Comp_section_entry;
                Comp.section_entry[com_num - 1] = "Coverage/" + Cov.id;
                complist.Add(Comp);
            }


            //取得r3資料
            nodes = doc.DocumentElement.SelectNodes("/myhealthbank/bdata/r3");

            //變數
            //時間變數
            time_start = "";
            time_end = "";

            //encounter 變數
            com_num = 0;
            en_reason_num = 0;
            en_diag_num = 0;

            //r3 資料擷取
            //foreach (XmlNode node in nodes)
            for (int i = 0; i < nodes.Count; i++)
            {
                //判斷r3是否有資料
                if (nodes[i].SelectSingleNode("r3.1") == null)
                {
                    break;
                }

                //時間重製
                time_start = "";
                time_end = "";

                //重製struct 資料
                Comp = new Composition_MHB();
                Enc = new Encounter_MHB();
                Con = new Condition_MHB();
                //Obs = new Observation();
                Pro = new Procedure_MHB();
                Med = new MedicationRequest_MHB();
                //Allergy = new AllergyIntolerance();
                //Immun = new Immunization();
                Cov = new Coverage_MHB();
                Org = new Organization_MHB();


                //判斷組織是否存在
                int org_yn = 0; //0代表有  1代表沒有
                for (int o_num = 0; o_num < orglist.Count; o_num++)
                {
                    if (orglist[o_num].id == nodes[i].SelectSingleNode("r3.3").InnerText)
                    {
                        Org.id = orglist[o_num].id;
                        org_yn = 1;
                        break;
                    }
                }
                if (org_yn == 0)
                {
                    Org.id = nodes[i].SelectSingleNode("r3.3").InnerText;
                    Org.type = nodes[i].SelectSingleNode("r3.2").InnerText;
                    Org.name = nodes[i].SelectSingleNode("r3.4").InnerText;
                    orglist.Add(Org);
                }

                //時間
                time_start = nodes[i].SelectSingleNode("r3.5").InnerText;
                time_end = nodes[i].SelectSingleNode("r3.5").InnerText;
                time_start = time_start.Substring(0, 4) + "-" + time_start.Substring(4, 2) + "-" + time_start.Substring(6, 2);
                time_end = time_end.Substring(0, 4) + "-" + time_end.Substring(4, 2) + "-" + time_end.Substring(6, 2);

                //Composition 資料
                Comp.id = num_id.ToString();
                num_id++;
                Comp.title = "r3";
                Comp.patient = "Patient/" + Pat.id;
                Comp.section_entry = new string[0];

                //Encounter 資料
                Enc.id = num_id.ToString();
                num_id++;
                Enc.en_class = "ambulatory"; //門診 ambulatory  住院 inpatient encounter
                Enc.subject = "Patient/" + Pat.id;
                Enc.period_start = time_start;
                Enc.period_end = time_end;
                Enc.Organization = "Organization/" + Org.id;
                Enc.reasonReference = new string[0];
                Enc.diagnosis = new string[0];

                //陣列變數初始化
                com_num = 0;
                en_reason_num = 0;
                en_diag_num = 0;

                //Composition 資料
                Comp.encounter = "Encounter/" + Enc.id;
                Comp.date = time_end;
                Comp.author = "Organization/" + Org.id;


                //Conditions 資料
                if (nodes[i].SelectSingleNode("r3.7").InnerText != "")
                {
                    Con.id = num_id.ToString();
                    num_id++;
                    Con.code_code = nodes[i].SelectSingleNode("r3.7").InnerText;
                    Con.code_diplay = nodes[i].SelectSingleNode("r3.8").InnerText;
                    Con.subject = "Patient/" + Pat.id;
                    Con.encounter = "Encounter/" + Enc.id;
                    Con.recoredDate = time_end;
                    conlist.Add(Con);

                    //Encounter reference condition
                    en_reason_num++;
                    en_diag_num++;
                    Enc_reasonReference = Enc.reasonReference;
                    Enc_diagnosis = Enc.diagnosis;
                    Array.Resize(ref Enc_reasonReference, en_reason_num);
                    Array.Resize(ref Enc_diagnosis, en_diag_num);
                    Enc.reasonReference = Enc_reasonReference;
                    Enc.diagnosis = Enc_diagnosis;
                    Enc.reasonReference[en_reason_num - 1] = "Condition/" + Con.id;
                    Enc.diagnosis[en_diag_num - 1] = "Condition/" + Con.id;

                    //Composition reference condition
                    com_num++;
                    Comp_section_entry = Comp.section_entry;
                    Array.Resize(ref Comp_section_entry, com_num);
                    Comp.section_entry = Comp_section_entry;
                    Comp.section_entry[com_num - 1] = "Condition/" + Con.id;
                }

                //Procedure 資料
                if (nodes[i].SelectSingleNode("r3.9").InnerText != "")
                {
                    Pro.id = num_id.ToString();
                    num_id++;
                    Pro.code_code = nodes[i].SelectSingleNode("r3.9").InnerText;
                    Pro.code_diplay = nodes[i].SelectSingleNode("r3.10").InnerText;
                    Pro.subject = "Patient/" + Pat.id;
                    Pro.encounter = "Encounter/" + Enc.id;
                    Pro.performedPeriod_start = time_start;
                    Pro.performedPeriod_end = time_end;
                    Pro.actor = "Organization/" + Org.id;
                    Pro.reasonReference = "Condition/" + Con.id;
                    prolist.Add(Pro);

                    //Encounter reference Procedure
                    en_reason_num++;
                    en_diag_num++;
                    Enc_reasonReference = Enc.reasonReference;
                    Enc_diagnosis = Enc.diagnosis;
                    Array.Resize(ref Enc_reasonReference, en_reason_num);
                    Array.Resize(ref Enc_diagnosis, en_diag_num);
                    Enc.reasonReference = Enc_reasonReference;
                    Enc.diagnosis = Enc_diagnosis;
                    Enc.reasonReference[en_reason_num - 1] = "Procedure/" + Pro.id;
                    Enc.diagnosis[en_diag_num - 1] = "Procedure/" + Pro.id;

                    //Composition reference Procedure
                    com_num++;
                    Comp_section_entry = Comp.section_entry;
                    Array.Resize(ref Comp_section_entry, com_num);
                    Comp.section_entry = Comp_section_entry;
                    Comp.section_entry[com_num - 1] = "Procedure/" + Pro.id;
                }
                XmlNodeList r3_1_nodes = nodes[i].SelectNodes("r3_1");
                //藥物與處置
                for (int j = 0; j < r3_1_nodes.Count; j++)
                {
                    //數字開頭 處置 
                    if (System.Text.RegularExpressions.Regex.IsMatch(r3_1_nodes[j].SelectSingleNode("r3_1.1").InnerText, @"^[0-9]"))
                    {
                        Pro = new Procedure_MHB();
                        Pro.id = num_id.ToString();
                        num_id++;
                        Pro.code_code = r3_1_nodes[j].SelectSingleNode("r3_1.1").InnerText;
                        Pro.code_diplay = r3_1_nodes[j].SelectSingleNode("r3_1.2").InnerText;
                        Pro.bodySite_code = r3_1_nodes[j].SelectSingleNode("r3_1.4").InnerText;
                        if (r3_1_nodes[j].SelectSingleNode("r3_1.5") != null)
                        {
                            Pro.bodySite_display = r3_1_nodes[j].SelectSingleNode("r3_1.5").InnerText;
                        }
                        Pro.subject = "Patient/" + Pat.id;
                        Pro.encounter = "Encounter/" + Enc.id;
                        Pro.performedPeriod_start = time_start;
                        Pro.performedPeriod_end = time_end;
                        Pro.actor = "Organization/" + Org.id;
                        Pro.reasonReference = "Condition/" + Con.id;
                        prolist.Add(Pro);

                        //Encounter reference Procedure
                        en_reason_num++;
                        en_diag_num++;
                        Enc_reasonReference = Enc.reasonReference;
                        Enc_diagnosis = Enc.diagnosis;
                        Array.Resize(ref Enc_reasonReference, en_reason_num);
                        Array.Resize(ref Enc_diagnosis, en_diag_num);
                        Enc.reasonReference = Enc_reasonReference;
                        Enc.diagnosis = Enc_diagnosis;
                        Enc.reasonReference[en_reason_num - 1] = "Procedure/" + Pro.id;
                        Enc.diagnosis[en_diag_num - 1] = "Procedure/" + Pro.id;

                        //Composition reference Procedure
                        com_num++;
                        Comp_section_entry = Comp.section_entry;
                        Array.Resize(ref Comp_section_entry, com_num);
                        Comp.section_entry = Comp_section_entry;
                        Comp.section_entry[com_num - 1] = "Procedure/" + Pro.id;
                    }
                    //字母開頭 藥物
                    else if (System.Text.RegularExpressions.Regex.IsMatch(r3_1_nodes[j].SelectSingleNode("r3_1.1").InnerText, @"^[a-zA-Z]"))
                    {
                        Med = new MedicationRequest_MHB();
                        //Medecation 藥品名 
                        Med.Medication_id = r3_1_nodes[j].SelectSingleNode("r3_1.1").InnerText;
                        Med.Medication_code = r3_1_nodes[j].SelectSingleNode("r3_1.1").InnerText;
                        Med.Medication_display = r3_1_nodes[j].SelectSingleNode("r3_1.2").InnerText;

                        //Request 藥囑處方
                        Med.id = num_id.ToString();
                        num_id++;
                        Med.medicationReference = "#" + Med.Medication_id;
                        Med.subject = "Patient/" + Pat.id;
                        Med.encounter = "Encounter/" + Enc.id;
                        Med.authoredOn = time_start;
                        Med.quantity = r3_1_nodes[j].SelectSingleNode("r3_1.3").InnerText;
                        Med.expectedSupplyDuration = r3_1_nodes[j].SelectSingleNode("r3_1.6").InnerText;
                        medlist.Add(Med);

                        //Composition reference Procedure
                        com_num++;
                        Comp_section_entry = Comp.section_entry;
                        Array.Resize(ref Comp_section_entry, com_num);
                        Comp.section_entry = Comp_section_entry;
                        Comp.section_entry[com_num - 1] = "MedicationRequest/" + Med.id;
                    }
                }

                //Coverage 資料
                //部分負擔(自付)
                Cov.id = num_id.ToString();
                num_id++;
                Cov.subscriber = "Patient/" + Pat.id;
                Cov.beneficiary = "Patient/" + Pat.id;
                Cov.period_start = time_start;
                Cov.period_end = time_end;
                Cov.payor = "Patient/" + Pat.id;
                Cov.valueMoney = nodes[i].SelectSingleNode("r3.11").InnerText;
                covlist.Add(Cov);

                //Composition reference Procedure
                com_num++;
                Comp_section_entry = Comp.section_entry;
                Array.Resize(ref Comp_section_entry, com_num);
                Comp.section_entry = Comp_section_entry;
                Comp.section_entry[com_num - 1] = "Coverage/" + Cov.id;

                //健保給付
                Cov = new Coverage_MHB();
                Cov.id = num_id.ToString();
                num_id++;
                Cov.subscriber = "Patient/" + Pat.id;
                Cov.beneficiary = "Patient/" + Pat.id;
                Cov.period_start = time_start;
                Cov.period_end = time_end;
                Cov.payor = "Organization/MOHW";
                Cov.valueMoney = nodes[i].SelectSingleNode("r3.12").InnerText;
                covlist.Add(Cov);

                //Encounter
                enclist.Add(Enc);

                //Composition reference Coverage
                com_num++;
                Comp_section_entry = Comp.section_entry;
                Array.Resize(ref Comp_section_entry, com_num);
                Comp.section_entry = Comp_section_entry;
                Comp.section_entry[com_num - 1] = "Coverage/" + Cov.id;
                complist.Add(Comp);
            }

            //取得r4資料
            nodes = doc.DocumentElement.SelectNodes("/myhealthbank/bdata/r4");

            //變數
            //時間變數
            time_start = "";
            time_end = "";

            //encounter 變數
            com_num = 0;

            //r4 資料擷取
            //foreach (XmlNode node in nodes)
            for (int i = 0; i < nodes.Count; i++)
            {
                //判斷r4是否有資料
                if (nodes[i].SelectSingleNode("r4.1") == null)
                {
                    break;
                }

                //時間重製
                time_start = "";
                time_end = "";

                //重製struct 資料
                Comp = new Composition_MHB();
                //Enc = new Encounter_MHB();
                //Con = new Condition_MHB();
                //Obs = new Observation_MHB();
                //Pro = new Procedure_MHB();
                //Med = new MedicationRequest_MHB();
                Allergy = new AllergyIntolerance_MHB();
                //Immun = new Immunization_MHB();
                //Cov = new Coverage_MHB();
                Org = new Organization_MHB();

                //判斷組織是否存在
                int org_yn = 0; //0代表有  1代表沒有
                for (int o_num = 0; o_num < orglist.Count; o_num++)
                {
                    if (orglist[o_num].id == nodes[i].SelectSingleNode("r4.4").InnerText)
                    {
                        Org.id = orglist[o_num].id;
                        org_yn = 1;
                        break;
                    }
                }
                if (org_yn == 0)
                {
                    Org.id = nodes[i].SelectSingleNode("r4.4").InnerText;
                    Org.name = nodes[i].SelectSingleNode("r4.5").InnerText;
                    Org.telecom = nodes[i].SelectSingleNode("r4.6").InnerText + " " + nodes[i].SelectSingleNode("r4.7").InnerText;
                    Org.address = nodes[i].SelectSingleNode("r4.8").InnerText;
                    orglist.Add(Org);
                }

                //時間
                time_start = nodes[i].SelectSingleNode("r4.1").InnerText;
                time_end = nodes[i].SelectSingleNode("r4.1").InnerText;
                time_start = time_start.Substring(0, 4) + "-" + time_start.Substring(4, 2) + "-" + time_start.Substring(6, 2);
                time_end = time_end.Substring(0, 4) + "-" + time_end.Substring(4, 2) + "-" + time_end.Substring(6, 2);

                //Composition 資料
                Comp.id = num_id.ToString();
                num_id++;
                Comp.title = "r4";
                Comp.patient = "Patient/" + Pat.id;
                Comp.section_entry = new string[0];

                //陣列變數初始化
                com_num = 0;

                //Composition 資料
                Comp.date = time_end;
                Comp.author = "Organization/" + Org.id;

                //AllergyIntolerance 資料
                Allergy.id = num_id.ToString();
                num_id++;
                Allergy.patient = "Patient/" + Pat.id;
                Allergy.code = nodes[i].SelectSingleNode("r4.2").InnerText;
                Allergy.recorder = nodes[i].SelectSingleNode("r4.3").InnerText;
                Allergy.recordedDate = time_end;
                allergylist.Add(Allergy);


                //Composition reference AllergyIntolerance
                com_num++;
                Comp_section_entry = Comp.section_entry;
                Array.Resize(ref Comp_section_entry, com_num);
                Comp.section_entry = Comp_section_entry;
                Comp.section_entry[com_num - 1] = "AllergyIntolerance/" + Allergy.id;
                complist.Add(Comp);
            }

            //取得r5資料
            nodes = doc.DocumentElement.SelectNodes("/myhealthbank/bdata/r5");

            //變數
            //時間變數
            time_start = "";
            time_end = "";

            //encounter 變數
            int pat_ex = 0;

            Pat.valueDate = new string[0];
            Pat.valuestring = new string[0];

            //r5 資料擷取
            //foreach (XmlNode node in nodes)
            for (int i = 0; i < nodes.Count; i++)
            {
                //判斷r5是否有資料
                if (nodes[i].SelectSingleNode("r5.1") == null)
                {
                    break;
                }
                pat_ex++;
                Pat_valueDate = Pat.valueDate;
                Pat_valuestring = Pat.valuestring;
                Array.Resize(ref Pat_valueDate, pat_ex);
                Array.Resize(ref Pat_valuestring, pat_ex);
                Pat.valueDate = Pat_valueDate;
                Pat.valuestring = Pat_valuestring;
                //時間重製
                time_start = nodes[i].SelectSingleNode("r5.1").InnerText;
                time_start = time_start.Substring(0, 4) + "-" + time_start.Substring(4, 2) + "-" + time_start.Substring(6, 2);

                Pat.valueDate[pat_ex - 1] = time_start;
                Pat.valuestring[pat_ex - 1] = nodes[i].SelectSingleNode("r5.3").InnerText;
            }


            //取得r6資料
            nodes = doc.DocumentElement.SelectNodes("/myhealthbank/bdata/r6");

            //變數
            //時間變數
            time_start = "";
            time_end = "";

            //encounter 變數
            com_num = 0;

            //r6 資料擷取
            //foreach (XmlNode node in nodes)
            for (int i = 0; i < nodes.Count; i++)
            {
                //判斷r4是否有資料
                if (nodes[i].SelectSingleNode("r6.1") == null)
                {
                    break;
                }

                //時間重製
                time_start = "";
                time_end = "";

                //重製struct 資料
                Comp = new Composition_MHB();
                Enc = new Encounter_MHB();
                //Con = new Condition_MHB();
                //Obs = new Observation_MHB();
                //Pro = new Procedure_MHB();
                //Med = new MedicationRequest_MHB();
                //Allergy = new AllergyIntolerance_MHB();
                Immun = new Immunization_MHB();
                //Cov = new Coverage_MHB();
                //Org = new Organization_MHB();

                //時間
                time_start = nodes[i].SelectSingleNode("r6.1").InnerText;
                time_end = nodes[i].SelectSingleNode("r6.1").InnerText;
                time_start = time_start.Substring(0, 4) + "-" + time_start.Substring(4, 2) + "-" + time_start.Substring(6, 2);
                time_end = time_end.Substring(0, 4) + "-" + time_end.Substring(4, 2) + "-" + time_end.Substring(6, 2);

                //Composition 資料
                Comp.id = num_id.ToString();
                num_id++;
                Comp.title = "r6";
                Comp.patient = "Patient/" + Pat.id;
                Comp.section_entry = new string[0];

                //Encounter 資料
                Enc.id = num_id.ToString();
                num_id++;
                Enc.en_class = "ambulatory"; //門診 ambulatory  住院 inpatient encounter
                Enc.subject = "Patient/" + Pat.id;
                Enc.period_start = time_start;
                Enc.period_end = time_end;
                Enc.reasonReference = new string[0];

                //陣列變數初始化
                com_num = 0;
                en_reason_num = 0;

                //Composition 資料
                Comp.encounter = "Encounter/" + Enc.id;
                Comp.date = time_end;
                //Comp.author = "Organization/" + Org.id;

                //Immunization 資料
                Immun.id = num_id.ToString();
                num_id++;
                Immun.vaccineCode = nodes[i].SelectSingleNode("r6.3").InnerText;
                Immun.patient = "Patient/" + Pat.id;
                Immun.encounter = "Encounter/" + Enc.id;
                Immun.occurrenceDateTime = time_end;
                immlist.Add(Immun);

                //Encounter reference Immunization
                en_reason_num++;
                Enc_reasonReference = Enc.reasonReference;
                Array.Resize(ref Enc_reasonReference, en_reason_num);
                Enc.reasonReference = Enc_reasonReference;
                Enc.reasonReference[en_reason_num - 1] = "ImmunizationRecommendation/" + Immun.id;

                //Encounter
                enclist.Add(Enc);

                //Composition reference Immunization
                com_num++;
                Comp_section_entry = Comp.section_entry;
                Array.Resize(ref Comp_section_entry, com_num);
                Comp.section_entry = Comp_section_entry;

                Comp.section_entry[com_num - 1] = "ImmunizationRecommendation/" + Immun.id;
                complist.Add(Comp);
            }

            //取得r7資料
            nodes = doc.DocumentElement.SelectNodes("/myhealthbank/bdata/r7");

            //變數
            //時間變數
            time_start = "";
            time_end = "";

            //encounter 變數
            com_num = 0;
            en_reason_num = 0;
            obs_comp_num = 0;

            //r7 資料擷取
            //foreach (XmlNode node in nodes)
            for (int i = 0; i < nodes.Count; i++)
            {
                //判斷r7是否有資料
                if (nodes[i].SelectSingleNode("r7.1") == null)
                {
                    break;
                }

                //時間重製
                time_start = "";
                time_end = "";

                //重製struct 資料
                Comp = new Composition_MHB();
                Enc = new Encounter_MHB();
                //Con = new Condition_MHB();
                Obs = new Observation_MHB();
                //Pro = new Procedure_MHB();
                //Med = new MedicationRequest_MHB();
                //Allergy = new AllergyIntolerance_MHB();
                //Immun = new Immunization_MHB();
                //Cov = new Coverage_MHB();
                Org = new Organization_MHB();

                //判斷組織是否存在
                int org_yn = 0; //0代表有  1代表沒有
                for (int o_num = 0; o_num < orglist.Count; o_num++)
                {
                    if (orglist[o_num].id == nodes[i].SelectSingleNode("r7.3").InnerText)
                    {
                        Org.id = orglist[o_num].id;
                        org_yn = 1;
                        break;
                    }
                }
                if (org_yn == 0)
                {
                    Org.id = nodes[i].SelectSingleNode("r7.3").InnerText;
                    Org.type = nodes[i].SelectSingleNode("r7.2").InnerText;
                    Org.name = nodes[i].SelectSingleNode("r7.4").InnerText;
                    orglist.Add(Org);
                }

                //時間
                time_start = nodes[i].SelectSingleNode("r7.5").InnerText;
                time_end = nodes[i].SelectSingleNode("r7.6").InnerText;
                time_start = time_start.Substring(0, 4) + "-" + time_start.Substring(4, 2) + "-" + time_start.Substring(6, 2);
                time_end = time_end.Substring(0, 4) + "-" + time_end.Substring(4, 2) + "-" + time_end.Substring(6, 2);

                //Composition 資料
                Comp.id = num_id.ToString();
                num_id++;
                Comp.title = "r7";
                Comp.patient = "Patient/" + Pat.id;

                Comp.section_entry = new string[0];

                //Encounter 資料
                Enc.id = num_id.ToString();
                num_id++;
                Enc.en_class = "ambulatory"; //門診 ambulatory  住院 inpatient encounter
                Enc.subject = "Patient/" + Pat.id;
                Enc.period_start = time_start;
                Enc.period_end = time_end;
                Enc.Organization = "Organization/" + Org.id;
                Enc.reasonReference = new string[0];


                //陣列變數初始化
                com_num = 0;
                en_reason_num = 0;
                obs_comp_num = 0;

                //Composition 資料
                Comp.encounter = "Encounter/" + Enc.id;
                Comp.date = time_end;
                Comp.author = "Organization/" + Org.id;

                //Observation 資料
                Obs.com_code = new string[0];
                Obs.com_code_display = new string[0];
                Obs.valueQuantity = new string[0];
                Obs.referenceRange_text = new string[0];
                Obs.interpretation = new string[0];

                Obs.id = num_id.ToString();
                num_id++;
                Obs.category = nodes[i].SelectSingleNode("r7.9").InnerText;
                Obs.code_code = nodes[i].SelectSingleNode("r7.8").InnerText;
                Obs.code_diplay = nodes[i].SelectSingleNode("r7.9").InnerText;
                Obs.subject = "Patient/" + Pat.id;
                Obs.encounter = "Encounter/" + Enc.id;
                Obs.performer = "Organization/" + Org.id;
                Obs.effectiveDateTime = time_end;
                obs_comp_num++;

                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;

                Obs.com_code[obs_comp_num - 1] = Obs.code_code;
                Obs.com_code_display[obs_comp_num - 1] = Obs.code_diplay;
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r7.11").InnerText;
                Obs.note = nodes[i].SelectSingleNode("r7.10").InnerText;
                Obs.referenceRange_text[obs_comp_num - 1] = nodes[i].SelectSingleNode("r7.12").InnerText;
                obslist.Add(Obs);

                //Encounter reference Observation
                en_reason_num++;
                Enc_reasonReference = Enc.reasonReference;
                Array.Resize(ref Enc_reasonReference, en_reason_num);
                Enc.reasonReference = Enc_reasonReference;
                Enc.reasonReference[en_reason_num - 1] = "Observation/" + Obs.id;

                //Encounter
                enclist.Add(Enc);

                //Composition reference Observation
                com_num++;
                Comp_section_entry = Comp.section_entry;
                Array.Resize(ref Comp_section_entry, com_num);
                Comp.section_entry = Comp_section_entry;
                Comp.section_entry[com_num - 1] = "Observation/" + Obs.id;
                complist.Add(Comp);
            }

            //取得r8資料
            nodes = doc.DocumentElement.SelectNodes("/myhealthbank/bdata/r8");

            //變數
            //時間變數
            time_start = "";
            time_end = "";

            //encounter 變數
            com_num = 0;
            en_reason_num = 0;
            obs_comp_num = 0;

            //r8 資料擷取
            //foreach (XmlNode node in nodes)
            for (int i = 0; i < nodes.Count; i++)
            {
                //判斷r8是否有資料
                if (nodes[i].SelectSingleNode("r8.1") == null)
                {
                    break;
                }

                //時間重製
                time_start = "";
                time_end = "";

                //重製struct 資料
                Comp = new Composition_MHB();
                Enc = new Encounter_MHB();
                //Con = new Condition_MHB();
                Obs = new Observation_MHB();
                //Pro = new Procedure_MHB();
                //Med = new MedicationRequest_MHB();
                //Allergy = new AllergyIntolerance_MHB();
                //Immun = new Immunization_MHB();
                //Cov = new Coverage_MHB();
                Org = new Organization_MHB();

                //判斷組織是否存在
                int org_yn = 0; //0代表有  1代表沒有
                for (int o_num = 0; o_num < orglist.Count; o_num++)
                {
                    if (orglist[o_num].id == nodes[i].SelectSingleNode("r8.3").InnerText)
                    {
                        Org.id = orglist[o_num].id;
                        org_yn = 1;
                        break;
                    }
                }
                if (org_yn == 0)
                {
                    Org.id = nodes[i].SelectSingleNode("r8.3").InnerText;
                    Org.type = nodes[i].SelectSingleNode("r8.2").InnerText;
                    Org.name = nodes[i].SelectSingleNode("r8.4").InnerText;
                    orglist.Add(Org);
                }

                //時間
                time_start = nodes[i].SelectSingleNode("r8.5").InnerText;
                time_end = nodes[i].SelectSingleNode("r8.6").InnerText;
                time_start = time_start.Substring(0, 4) + "-" + time_start.Substring(4, 2) + "-" + time_start.Substring(6, 2);
                time_end = time_end.Substring(0, 4) + "-" + time_end.Substring(4, 2) + "-" + time_end.Substring(6, 2);

                //Composition 資料
                Comp.id = num_id.ToString();
                num_id++;
                Comp.title = "r8";
                Comp.patient = "Patient/" + Pat.id;
                Comp.section_entry = new string[0];

                //Encounter 資料
                Enc.id = num_id.ToString();
                num_id++;
                Enc.en_class = "ambulatory"; //門診 ambulatory  住院 inpatient encounter
                Enc.subject = "Patient/" + Pat.id;
                Enc.period_start = time_start;
                Enc.period_end = time_end;
                Enc.Organization = "Organization/" + Org.id;
                Enc.reasonReference = new string[0];


                //陣列變數初始化
                com_num = 0;
                en_reason_num = 0;
                obs_comp_num = 0;

                //Composition 資料
                Comp.encounter = "Encounter/" + Enc.id;
                Comp.date = time_end;
                Comp.author = "Organization/" + Org.id;

                //Observation 資料
                Obs.com_code = new string[0];
                Obs.com_code_display = new string[0];
                Obs.interpretation = new string[0];
                Obs.valueQuantity = new string[0];
                Obs.referenceRange_text = new string[0];
                Obs.id = num_id.ToString();
                num_id++;
                Obs.category = nodes[i].SelectSingleNode("r8.9").InnerText;
                Obs.code_code = nodes[i].SelectSingleNode("r8.8").InnerText;
                Obs.code_diplay = nodes[i].SelectSingleNode("r8.9").InnerText;
                Obs.subject = "Patient/" + Pat.id;
                Obs.encounter = "Encounter/" + Enc.id;
                Obs.performer = "Organization/" + Org.id;
                Obs.effectiveDateTime = time_end;
                Obs.note = nodes[i].SelectSingleNode("r8.10").InnerText;
                obslist.Add(Obs);

                //Encounter reference Observation
                en_reason_num++;
                Enc_reasonReference = Enc.reasonReference;
                Array.Resize(ref Enc_reasonReference, en_reason_num);
                Enc.reasonReference = Enc_reasonReference;
                Enc.reasonReference[en_reason_num - 1] = "Observation/" + Obs.id;

                //Encounter
                enclist.Add(Enc);

                //Composition reference Observation
                com_num++;
                Comp_section_entry = Comp.section_entry;
                Array.Resize(ref Comp_section_entry, com_num);
                Comp.section_entry = Comp_section_entry;
                Comp.section_entry[com_num - 1] = "Observation/" + Obs.id;
                complist.Add(Comp);
            }

            //取得r9資料
            nodes = doc.DocumentElement.SelectNodes("/myhealthbank/bdata/r9");

            //變數
            //時間變數
            time_start = "";
            time_end = "";

            //encounter 變數
            com_num = 0;
            en_reason_num = 0;
            en_diag_num = 0;

            //r9 資料擷取
            //foreach (XmlNode node in nodes)
            for (int i = 0; i < nodes.Count; i++)
            {
                //判斷r9是否有資料
                if (nodes[i].SelectSingleNode("r9.1") == null)
                {
                    break;
                }

                //時間重製
                time_start = "";
                time_end = "";

                //重製struct 資料
                Comp = new Composition_MHB();
                Enc = new Encounter_MHB();
                Con = new Condition_MHB();
                //Obs = new Observation_MHB();
                Pro = new Procedure_MHB();
                Med = new MedicationRequest_MHB();
                //Allergy = new AllergyIntolerance_MHB();
                //Immun = new Immunization_MHB();
                Cov = new Coverage_MHB();
                Org = new Organization_MHB();


                //判斷組織是否存在
                int org_yn = 0; //0代表有  1代表沒有
                for (int o_num = 0; o_num < orglist.Count; o_num++)
                {
                    if (orglist[o_num].id == nodes[i].SelectSingleNode("r9.3").InnerText)
                    {
                        Org.id = orglist[o_num].id;
                        org_yn = 1;
                        break;
                    }
                }
                if (org_yn == 0)
                {
                    Org.id = nodes[i].SelectSingleNode("r9.3").InnerText;
                    Org.type = nodes[i].SelectSingleNode("r9.2").InnerText;
                    Org.name = nodes[i].SelectSingleNode("r9.4").InnerText;
                    orglist.Add(Org);
                }

                //時間
                time_start = nodes[i].SelectSingleNode("r9.5").InnerText;
                time_end = nodes[i].SelectSingleNode("r9.5").InnerText;
                time_start = time_start.Substring(0, 4) + "-" + time_start.Substring(4, 2) + "-" + time_start.Substring(6, 2);
                time_end = time_end.Substring(0, 4) + "-" + time_end.Substring(4, 2) + "-" + time_end.Substring(6, 2);

                //Composition 資料
                Comp.id = num_id.ToString();
                num_id++;
                Comp.title = "r9";
                Comp.patient = "Patient/" + Pat.id;
                Comp.section_entry = new string[0];

                //Encounter 資料
                Enc.id = num_id.ToString();
                num_id++;
                Enc.en_class = "ambulatory"; //門診 ambulatory  住院 inpatient encounter
                Enc.subject = "Patient/" + Pat.id;
                Enc.period_start = time_start;
                Enc.period_end = time_end;
                Enc.Organization = "Organization/" + Org.id;
                Enc.reasonReference = new string[0];
                Enc.diagnosis = new string[0];

                //陣列變數初始化
                com_num = 0;
                en_reason_num = 0;
                en_diag_num = 0;

                //Composition 資料
                Comp.encounter = "Encounter/" + Enc.id;
                Comp.date = time_end;
                Comp.author = "Organization/" + Org.id;


                //Conditions 資料
                if (nodes[i].SelectSingleNode("r9.7").InnerText != "")
                {
                    Con.id = num_id.ToString();
                    num_id++;
                    Con.code_code = nodes[i].SelectSingleNode("r9.7").InnerText;
                    Con.code_diplay = nodes[i].SelectSingleNode("r9.8").InnerText;
                    Con.subject = "Patient/" + Pat.id;
                    Con.encounter = "Encounter/" + Enc.id;
                    Con.recoredDate = time_end;
                    conlist.Add(Con);

                    //Encounter reference condition
                    en_reason_num++;
                    en_diag_num++;
                    Enc_reasonReference = Enc.reasonReference;
                    Enc_diagnosis = Enc.diagnosis;
                    Array.Resize(ref Enc_reasonReference, en_reason_num);
                    Array.Resize(ref Enc_diagnosis, en_diag_num);
                    Enc.reasonReference = Enc_reasonReference;
                    Enc.diagnosis = Enc_diagnosis;
                    Enc.reasonReference[en_reason_num - 1] = "Condition/" + Con.id;
                    Enc.diagnosis[en_diag_num - 1] = "Condition/" + Con.id;

                    //Composition reference condition
                    com_num++;
                    Comp_section_entry = Comp.section_entry;
                    Array.Resize(ref Comp_section_entry, com_num);
                    Comp.section_entry = Comp_section_entry;
                    Comp.section_entry[com_num - 1] = "Condition/" + Con.id;
                }

                //Procedure 資料
                if (nodes[i].SelectSingleNode("r9.9").InnerText != "")
                {
                    Pro.id = num_id.ToString();
                    num_id++;
                    Pro.code_code = nodes[i].SelectSingleNode("r9.9").InnerText;
                    Pro.code_diplay = nodes[i].SelectSingleNode("r9.10").InnerText;
                    Pro.subject = "Patient/" + Pat.id;
                    Pro.encounter = "Encounter/" + Enc.id;
                    Pro.performedPeriod_start = time_start;
                    Pro.performedPeriod_end = time_end;
                    Pro.actor = "Organization/" + Org.id;
                    Pro.reasonReference = "Condition/" + Con.id;
                    prolist.Add(Pro);

                    //Encounter reference Procedure
                    en_reason_num++;
                    en_diag_num++;
                    Enc_reasonReference = Enc.reasonReference;
                    Enc_diagnosis = Enc.diagnosis;
                    Array.Resize(ref Enc_reasonReference, en_reason_num);
                    Array.Resize(ref Enc_diagnosis, en_diag_num);
                    Enc.reasonReference = Enc_reasonReference;
                    Enc.diagnosis = Enc_diagnosis;
                    Enc.reasonReference[en_reason_num - 1] = "Procedure/" + Pro.id;
                    Enc.diagnosis[en_diag_num - 1] = "Procedure/" + Pro.id;

                    //Composition reference Procedure
                    com_num++;
                    Comp_section_entry = Comp.section_entry;
                    Array.Resize(ref Comp_section_entry, com_num);
                    Comp.section_entry = Comp_section_entry;
                    Comp.section_entry[com_num - 1] = "Procedure/" + Pro.id;
                }
                XmlNodeList r9_1_nodes = nodes[i].SelectNodes("r9_1");
                //藥物與處置
                for (int j = 0; j < r9_1_nodes.Count; j++)
                {
                    //數字開頭 處置 
                    if (System.Text.RegularExpressions.Regex.IsMatch(r9_1_nodes[j].SelectSingleNode("r9_1.1").InnerText, @"^[0-9]"))
                    {
                        Pro = new Procedure_MHB();
                        Pro.id = num_id.ToString();
                        num_id++;
                        Pro.code_code = r9_1_nodes[j].SelectSingleNode("r9_1.1").InnerText;
                        Pro.code_diplay = r9_1_nodes[j].SelectSingleNode("r9_1.2").InnerText;
                        Pro.subject = "Patient/" + Pat.id;
                        Pro.encounter = "Encounter/" + Enc.id;
                        Pro.performedPeriod_start = time_start;
                        Pro.performedPeriod_end = time_end;
                        Pro.actor = "Organization/" + Org.id;
                        Pro.reasonReference = "Condition/" + Con.id;
                        prolist.Add(Pro);

                        //Encounter reference Procedure
                        en_reason_num++;
                        en_diag_num++;
                        Enc_reasonReference = Enc.reasonReference;
                        Enc_diagnosis = Enc.diagnosis;
                        Array.Resize(ref Enc_reasonReference, en_reason_num);
                        Array.Resize(ref Enc_diagnosis, en_diag_num);
                        Enc.reasonReference = Enc_reasonReference;
                        Enc.diagnosis = Enc_diagnosis;
                        Enc.reasonReference[en_reason_num - 1] = "Procedure/" + Pro.id;
                        Enc.diagnosis[en_diag_num - 1] = "Procedure/" + Pro.id;

                        //Composition reference Procedure
                        com_num++;
                        Comp_section_entry = Comp.section_entry;
                        Array.Resize(ref Comp_section_entry, com_num);
                        Comp.section_entry = Comp_section_entry;
                        Comp.section_entry[com_num - 1] = "Procedure/" + Pro.id;
                    }
                    //字母開頭 藥物
                    else if (System.Text.RegularExpressions.Regex.IsMatch(r9_1_nodes[j].SelectSingleNode("r9_1.1").InnerText, @"^[a-zA-Z]"))
                    {
                        Med = new MedicationRequest_MHB();
                        //Medecation 藥品名 
                        Med.Medication_id = r9_1_nodes[j].SelectSingleNode("r9_1.1").InnerText;
                        Med.Medication_code = r9_1_nodes[j].SelectSingleNode("r9_1.1").InnerText;
                        Med.Medication_display = r9_1_nodes[j].SelectSingleNode("r9_1.2").InnerText;

                        //Request 藥囑處方
                        Med.id = num_id.ToString();
                        num_id++;
                        Med.medicationReference = "#" + Med.Medication_id;
                        Med.subject = "Patient/" + Pat.id;
                        Med.encounter = "Encounter/" + Enc.id;
                        Med.authoredOn = time_start;
                        Med.quantity = r9_1_nodes[j].SelectSingleNode("r9_1.3").InnerText;
                        Med.expectedSupplyDuration = r9_1_nodes[j].SelectSingleNode("r9_1.4").InnerText;
                        medlist.Add(Med);

                        //Composition reference Procedure
                        com_num++;
                        Comp_section_entry = Comp.section_entry;
                        Array.Resize(ref Comp_section_entry, com_num);
                        Comp.section_entry = Comp_section_entry;
                        Comp.section_entry[com_num - 1] = "MedicationRequest/" + Med.id;
                    }
                }

                //Coverage 資料
                //部分負擔(自付)
                Cov.id = num_id.ToString();
                num_id++;
                Cov.subscriber = "Patient/" + Pat.id;
                Cov.beneficiary = "Patient/" + Pat.id;
                Cov.period_start = time_start;
                Cov.period_end = time_end;
                Cov.payor = "Patient/" + Pat.id;
                Cov.valueMoney = nodes[i].SelectSingleNode("r9.11").InnerText;
                covlist.Add(Cov);

                //Composition reference Procedure
                com_num++;
                Comp_section_entry = Comp.section_entry;
                Array.Resize(ref Comp_section_entry, com_num);
                Comp.section_entry = Comp_section_entry;
                Comp.section_entry[com_num - 1] = "Coverage/" + Cov.id;

                //健保給付
                Cov = new Coverage_MHB();
                Cov.id = num_id.ToString();
                num_id++;
                Cov.subscriber = "Patient/" + Pat.id;
                Cov.beneficiary = "Patient/" + Pat.id;
                Cov.period_start = time_start;
                Cov.period_end = time_end;
                Cov.payor = "Organization/MOHW";
                Cov.valueMoney = nodes[i].SelectSingleNode("r9.12").InnerText;
                covlist.Add(Cov);

                //Encounter
                enclist.Add(Enc);

                //Composition reference Coverage
                com_num++;
                Comp_section_entry = Comp.section_entry;
                Array.Resize(ref Comp_section_entry, com_num);
                Comp.section_entry = Comp_section_entry;
                Comp.section_entry[com_num - 1] = "Coverage/" + Cov.id;
                complist.Add(Comp);
            }

            //取得r10資料
            nodes = doc.DocumentElement.SelectNodes("/myhealthbank/bdata/r10");

            //變數
            //時間變數
            time_start = "";
            time_end = "";

            //encounter 變數
            com_num = 0;
            en_reason_num = 0;
            obs_comp_num = 0;

            //r10 資料擷取
            //foreach (XmlNode node in nodes)
            for (int i = 0; i < nodes.Count; i++)
            {
                //判斷R10是否有資料
                if (nodes[i].SelectSingleNode("r10.1") == null)
                {
                    break;
                }

                //時間重製
                time_start = "";
                time_end = "";

                //重製struct 資料
                Comp = new Composition_MHB();
                Enc = new Encounter_MHB();
                //Con = new Condition();
                Obs = new Observation_MHB();
                //Pro = new Procedure();
                //Med = new MedicationRequest();
                //Allergy = new AllergyIntolerance();
                //Immun = new Immunization();
                //Cov = new Coverage();
                Org = new Organization_MHB();

                //判斷組織是否存在
                int org_yn = 0; //0代表有  1代表沒有
                for (int o_num = 0; o_num < orglist.Count; o_num++)
                {
                    if (orglist[o_num].id == nodes[i].SelectSingleNode("r10.1").InnerText)
                    {
                        Org.id = orglist[o_num].id;
                        org_yn = 1;
                        break;
                    }
                }
                if (org_yn == 0)
                {
                    Org.id = nodes[i].SelectSingleNode("r10.1").InnerText;
                    Org.name = nodes[i].SelectSingleNode("r10.2").InnerText;
                    Org.telecom = nodes[i].SelectSingleNode("r10.3").InnerText + " " + nodes[i].SelectSingleNode("r10.4").InnerText;
                    orglist.Add(Org);
                }

                //時間
                time_start = nodes[i].SelectSingleNode("r10.5").InnerText;
                time_end = nodes[i].SelectSingleNode("r10.5").InnerText;
                time_start = time_start.Substring(0, 4) + "-" + time_start.Substring(4, 2) + "-" + time_start.Substring(6, 2);
                time_end = time_end.Substring(0, 4) + "-" + time_end.Substring(4, 2) + "-" + time_end.Substring(6, 2);

                //Composition 資料
                Comp.id = num_id.ToString();
                num_id++;
                Comp.title = "r10";
                Comp.patient = "Patient/" + Pat.id;
                Comp.section_entry = new string[0];

                //Encounter 資料
                Enc.id = num_id.ToString();
                num_id++;
                Enc.en_class = "ambulatory"; //門診 ambulatory  住院 inpatient encounter
                Enc.subject = "Patient/" + Pat.id;
                Enc.period_start = time_start;
                Enc.period_end = time_end;
                Enc.Organization = "Organization/" + Org.id;
                Enc.reasonReference = new string[0];


                //陣列變數初始化
                com_num = 0;
                en_reason_num = 0;
                obs_comp_num = 0;

                //Composition 資料
                Comp.encounter = "Encounter/" + Enc.id;
                Comp.date = time_end;
                Comp.author = "Organization/" + Org.id;

                //Observation 資料
                Obs.com_code = new string[0];
                Obs.com_code_display = new string[0];
                Obs.valueQuantity = new string[0];
                Obs.interpretation = new string[0];
                Obs.referenceRange_text = new string[0];
                Obs.id = num_id.ToString();
                num_id++;
                Obs.category = "成人預防保健存摺";
                Obs.code_diplay = Obs.category;
                Obs.subject = "Patient/" + Pat.id;
                Obs.encounter = "Encounter/" + Enc.id;
                Obs.performer = "Organization/" + Org.id;
                Obs.effectiveDateTime = time_end;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "一般檢查_身高";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.6").InnerText;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "一般檢查_體重";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.7").InnerText;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "一般檢查_BMI(身體質量指數)";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.8").InnerText;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "一般檢查_腰圍";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.9").InnerText;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "一般檢查_收縮壓";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.10").InnerText;
                string inter = "";
                switch (nodes[i].SelectSingleNode("r10.12").InnerText)
                {
                    case "0":
                        inter = "無";
                        break;
                    case "1":
                        inter = "正常";
                        break;
                    case "2":
                        inter = "異常,建議:請洽詢醫師";
                        break;
                    case "3":
                        inter = "異常,建議:進一步檢查";
                        break;
                    case "4":
                        inter = "異常,建議:接受治療";
                        break;
                }
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "一般檢查_舒張壓";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.11").InnerText;
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "血脂肪檢查_總膽固醇(TC)";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.14").InnerText;
                inter = "";
                switch (nodes[i].SelectSingleNode("r10.18").InnerText)
                {
                    case "0":
                        inter = "無";
                        break;
                    case "1":
                        inter = "正常";
                        break;
                    case "2":
                        inter = "異常,建議:請洽詢醫師";
                        break;
                    case "3":
                        inter = "異常,建議:進一步檢查";
                        break;
                    case "4":
                        inter = "異常,建議:接受治療";
                        break;
                }
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "血脂肪檢查_三酸甘酯(TG)";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.15").InnerText;
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "血脂肪檢查_高密度脂蛋白膽固醇(HDL)";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.16").InnerText;
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "血脂肪檢查_低密度脂蛋白膽固醇(LDL)";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.17").InnerText;
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "血糖檢查_飯前血糖";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.20").InnerText;
                inter = "";
                switch (nodes[i].SelectSingleNode("r10.21").InnerText)
                {
                    case "0":
                        inter = "無";
                        break;
                    case "1":
                        inter = "正常";
                        break;
                    case "2":
                        inter = "異常,建議:請洽詢醫師";
                        break;
                    case "3":
                        inter = "異常,建議:進一步檢查";
                        break;
                    case "4":
                        inter = "異常,建議:接受治療";
                        break;
                }
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "腎功能檢查_尿素氮";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.23").InnerText;
                inter = "";
                switch (nodes[i].SelectSingleNode("r10.26").InnerText)
                {
                    case "0":
                        inter = "無";
                        break;
                    case "1":
                        inter = "正常";
                        break;
                    case "2":
                        inter = "異常,建議:請洽詢醫師";
                        break;
                    case "3":
                        inter = "異常,建議:進一步檢查";
                        break;
                    case "4":
                        inter = "異常,建議:接受治療";
                        break;
                }
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "腎功能檢查_肌酸酐";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.24").InnerText;
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "腎功能檢查_腎絲球過濾率";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.25").InnerText;
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "尿液檢查_尿液蛋白質";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.28").InnerText;
                inter = "";
                switch (nodes[i].SelectSingleNode("r10.29").InnerText)
                {
                    case "0":
                        inter = "無";
                        break;
                    case "1":
                        inter = "正常";
                        break;
                    case "2":
                        inter = "異常,建議:請洽詢醫師";
                        break;
                    case "3":
                        inter = "異常,建議:進一步檢查";
                        break;
                    case "4":
                        inter = "異常,建議:接受治療";
                        break;
                }
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "肝功能檢查_SGOT(AST或GOT)";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.31").InnerText;
                inter = "";
                switch (nodes[i].SelectSingleNode("r10.33").InnerText)
                {
                    case "0":
                        inter = "無";
                        break;
                    case "1":
                        inter = "正常";
                        break;
                    case "2":
                        inter = "異常,建議:請洽詢醫師";
                        break;
                    case "3":
                        inter = "異常,建議:進一步檢查";
                        break;
                    case "4":
                        inter = "異常,建議:接受治療";
                        break;
                }
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "肝功能檢查_SGPT(ALT或GPT)";
                Obs.valueQuantity[obs_comp_num - 1] = nodes[i].SelectSingleNode("r10.32").InnerText;
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "B 型肝炎檢查_B 型肝炎表面抗原(HBsAg)";
                inter = "";
                switch (nodes[i].SelectSingleNode("r10.35").InnerText)
                {
                    case "1":
                        inter = "陰性";
                        break;
                    case "2":
                        inter = "陽性";
                        break;
                    case "3":
                        inter = "無";
                        break;
                }
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "B 型肝炎檢查結果";
                inter = "";
                switch (nodes[i].SelectSingleNode("r10.37").InnerText)
                {
                    case "0":
                        inter = "無";
                        break;
                    case "1":
                        inter = "陰性";
                        break;
                    case "2":
                        inter = "陽性,建議進一步檢查";
                        break;
                }
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "C 型肝炎檢查_C 型肝炎抗體(Anti-HCV)";
                inter = "";
                switch (nodes[i].SelectSingleNode("r10.39").InnerText)
                {
                    case "1":
                        inter = "陰性";
                        break;
                    case "2":
                        inter = "陽性";
                        break;
                    case "3":
                        inter = "無";
                        break;
                }
                Obs.interpretation[obs_comp_num - 1] = inter;

                obs_comp_num++;
                Obs_com_code = Obs.com_code;
                Obs_com_code_display = Obs.com_code_display;
                Obs_valueQuantity = Obs.valueQuantity;
                Obs_referenceRange_text = Obs.referenceRange_text;
                Obs_interpretation = Obs.interpretation;
                Array.Resize(ref Obs_com_code, obs_comp_num);
                Array.Resize(ref Obs_com_code_display, obs_comp_num);
                Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                Array.Resize(ref Obs_interpretation, obs_comp_num);
                Obs.com_code = Obs_com_code;
                Obs.com_code_display = Obs_com_code_display;
                Obs.valueQuantity = Obs_valueQuantity;
                Obs.referenceRange_text = Obs_referenceRange_text;
                Obs.interpretation = Obs_interpretation;
                Obs.com_code_display[obs_comp_num - 1] = "C 型肝炎檢查結果";
                inter = "";
                switch (nodes[i].SelectSingleNode("r10.41").InnerText)
                {
                    case "0":
                        inter = "無";
                        break;
                    case "1":
                        inter = "陰性";
                        break;
                    case "":
                        inter = "陽性,建議進一步檢查";
                        break;
                }
                Obs.interpretation[obs_comp_num - 1] = inter;
                obslist.Add(Obs);


                //Encounter reference Observation
                en_reason_num++;
                Enc_reasonReference = Enc.reasonReference;
                Array.Resize(ref Enc_reasonReference, en_reason_num);
                Enc.reasonReference = Enc_reasonReference;
                Enc.reasonReference[en_reason_num - 1] = "Observation/" + Obs.id;

                //Encounter
                enclist.Add(Enc);

                //Composition reference Observation
                com_num++;
                Comp_section_entry = Comp.section_entry;
                Array.Resize(ref Comp_section_entry, com_num);
                Comp.section_entry = Comp_section_entry;
                Comp.section_entry[com_num - 1] = "Observation/" + Obs.id;
                complist.Add(Comp);
            }

            //取得r11資料
            nodes = doc.DocumentElement.SelectNodes("/myhealthbank/bdata/r11");

            //變數
            //時間變數
            time_start = "";
            time_end = "";

            //encounter 變數
            com_num = 0;
            en_reason_num = 0;
            obs_comp_num = 0;

            //r11 資料擷取
            //foreach (XmlNode node in nodes)
            for (int i = 0; i < nodes.Count; i++)
            {
                //判斷r7是否有資料
                if (nodes[i].SelectSingleNode("r11.1") == null)
                {
                    break;
                }

                XmlNodeList r11_1_nodes = nodes[i].SelectNodes("r11_1");
                for (int j = 0; j < r11_1_nodes.Count; j++)
                {
                    //時間重製
                    time_start = "";
                    time_end = "";

                    //重製struct 資料
                    Comp = new Composition_MHB();
                    Enc = new Encounter_MHB();
                    //Con = new Condition();
                    Obs = new Observation_MHB();
                    //Pro = new Procedure();
                    //Med = new MedicationRequest();
                    //Allergy = new AllergyIntolerance();
                    //Immun = new Immunization();
                    //Cov = new Coverage();
                    Org = new Organization_MHB();

                    //判斷組織是否存在
                    int org_yn = 0; //0代表有  1代表沒有
                    for (int o_num = 0; o_num < orglist.Count; o_num++)
                    {
                        if (orglist[o_num].name == r11_1_nodes[j].SelectSingleNode("r11_1.2").InnerText)
                        {
                            Org.id = orglist[o_num].id;
                            org_yn = 1;
                            break;
                        }
                    }
                    if (org_yn == 0)
                    {
                        Org.id = num_id.ToString();
                        num_id++;
                        Org.name = r11_1_nodes[j].SelectSingleNode("r11_1.2").InnerText;
                        orglist.Add(Org);
                    }

                    //時間
                    time_start = r11_1_nodes[j].SelectSingleNode("r11_1.1").InnerText;
                    time_end = r11_1_nodes[j].SelectSingleNode("r11_1.1").InnerText;
                    time_start = time_start.Substring(0, 4) + "-" + time_start.Substring(4, 2) + "-" + time_start.Substring(6, 2);
                    time_end = time_end.Substring(0, 4) + "-" + time_end.Substring(4, 2) + "-" + time_end.Substring(6, 2);

                    //Composition 資料
                    Comp.id = num_id.ToString();
                    num_id++;
                    Comp.title = "r11";
                    Comp.patient = "Patient/" + Pat.id;
                    Comp.section_entry = new string[0];

                    //Encounter 資料
                    Enc.id = num_id.ToString();
                    num_id++;
                    Enc.en_class = "ambulatory"; //門診 ambulatory  住院 inpatient encounter
                    Enc.subject = "Patient/" + Pat.id;
                    Enc.period_start = time_start;
                    Enc.period_end = time_end;
                    Enc.Organization = "Organization/" + Org.id;
                    Enc.reasonReference = new string[0];

                    //陣列變數初始化
                    com_num = 0;
                    en_reason_num = 0;
                    obs_comp_num = 0;

                    //Composition 資料
                    Comp.encounter = "Encounter/" + Enc.id;
                    Comp.date = time_end;
                    Comp.author = "Organization/" + Org.id;

                    //Observation 資料
                    Obs.com_code = new string[0];
                    Obs.com_code_display = new string[0];
                    Obs.interpretation = new string[0];
                    Obs.valueQuantity = new string[0];
                    Obs.referenceRange_text = new string[0];
                    Obs.id = num_id.ToString();
                    num_id++;
                    Obs.category = nodes[i].SelectSingleNode("r11.1").InnerText;
                    Obs.code_diplay = nodes[i].SelectSingleNode("r11.2").InnerText;
                    Obs.subject = "Patient/" + Pat.id;
                    Obs.encounter = "Encounter/" + Enc.id;
                    Obs.effectiveDateTime = time_end;
                    obs_comp_num++;
                    Obs_com_code = Obs.com_code;
                    Obs_com_code_display = Obs.com_code_display;
                    Obs_valueQuantity = Obs.valueQuantity;
                    Obs_referenceRange_text = Obs.referenceRange_text;
                    Obs_interpretation = Obs.interpretation;
                    Array.Resize(ref Obs_com_code, obs_comp_num);
                    Array.Resize(ref Obs_com_code_display, obs_comp_num);
                    Array.Resize(ref Obs_valueQuantity, obs_comp_num);
                    Array.Resize(ref Obs_referenceRange_text, obs_comp_num);
                    Array.Resize(ref Obs_interpretation, obs_comp_num);
                    Obs.com_code = Obs_com_code;
                    Obs.com_code_display = Obs_com_code_display;
                    Obs.valueQuantity = Obs_valueQuantity;
                    Obs.referenceRange_text = Obs_referenceRange_text;
                    Obs.interpretation = Obs_interpretation;
                    Obs.interpretation[obs_comp_num - 1] = r11_1_nodes[j].SelectSingleNode("r11_1.3").InnerText;
                    Obs.note = r11_1_nodes[j].SelectSingleNode("r11_1.4").InnerText;
                    obslist.Add(Obs);

                    //Encounter reference Observation
                    en_reason_num++;
                    Enc_reasonReference = Enc.reasonReference;
                    Array.Resize(ref Enc_reasonReference, en_reason_num);
                    Enc.reasonReference = Enc_reasonReference;
                    Enc.reasonReference[en_reason_num - 1] = "Observation/" + Obs.id;

                    //Encounter
                    enclist.Add(Enc);

                    //Composition reference Observation
                    com_num++;
                    Comp_section_entry = Comp.section_entry;
                    Array.Resize(ref Comp_section_entry, com_num);
                    Comp.section_entry = Comp_section_entry;
                    Comp.section_entry[com_num - 1] = "Observation/" + Obs.id;
                    complist.Add(Comp);
                }
            }
        }

        public string TransfertoJSON_API(string path_upload, string fileName)
        {
            XML_Path(path_upload);
            var path = Server.MapPath("~/FHIR/");
            //若該資料夾不存在，則新增一個
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            fhir = fileName + "_FHIR.json";
            //將文字寫成檔案，存放在系統中
            path = Path.Combine(path, fhir);
            System.IO.File.WriteAllText(path, TransfertoJSON(), Encoding.UTF8);
            return "轉檔成功";
        }

        public string TransfertoJSON()
        {
            string MHB_Json = "";

            //JSON寫入到檔案
            using (StringWriter sw = new StringWriter())
            {
                using (JsonTextWriter writer = new JsonTextWriter(sw))
                {
                    ////建立物件
                    //writer.WriteStartObject();  // 新增 "{"
                    ////物件名稱
                    //writer.WritePropertyName("Bundle");
                    ////設定值
                    //writer.WriteValue(index);
                    //writer.WriteEndObject();

                    //Bundle
                    writer.WriteStartObject();
                    writer.WritePropertyName("resourceType");
                    writer.WriteValue("Bundle");
                    writer.WritePropertyName("id");
                    writer.WriteValue(Bun.id);
                    //Bundle meta  更新版本時間
                    writer.WritePropertyName("meta");
                    writer.WriteStartObject();
                    writer.WritePropertyName("lastUpdated");
                    writer.WriteValue(Bun.meta);
                    writer.WriteEndObject();
                    writer.WritePropertyName("type");
                    writer.WriteValue("transaction");

                    //Bundle entry
                    writer.WritePropertyName("entry");
                    ////建立陣列
                    writer.WriteStartArray();   //  新增  [

                    //entry resource start
                    writer.WriteStartObject();
                    writer.WritePropertyName("resource");

                    //Patient start
                    writer.WriteStartObject();
                    writer.WritePropertyName("resourceType");
                    writer.WriteValue("Patient");
                    writer.WritePropertyName("id");
                    writer.WriteValue(Pat.id);
                    //meta  更新版本時間
                    writer.WritePropertyName("meta");
                    writer.WriteStartObject();
                    writer.WritePropertyName("lastUpdated");
                    writer.WriteValue(Bun.meta);
                    writer.WriteEndObject();
                    if (Pat.valueDate.Length != 0)  //判斷是否有器官捐贈資料
                    {
                        writer.WritePropertyName("extension");
                        writer.WriteStartArray();
                        for (int i = 0; i < Pat.valueDate.Length; i++)
                        {
                            writer.WriteStartObject();
                            writer.WritePropertyName("valueDate");
                            writer.WriteValue(Pat.valueDate[i]);
                            writer.WritePropertyName("valuestring");
                            writer.WriteValue(Pat.valuestring[i]);
                            writer.WriteEndObject();
                        }
                        writer.WriteEndArray();
                    }
                    writer.WritePropertyName("name");
                    writer.WriteValue(Pat.name);
                    writer.WritePropertyName("gender");
                    writer.WriteValue(Pat.gender);
                    writer.WritePropertyName("birthDate");
                    writer.WriteValue(Pat.birthDate);
                    //Patient end              
                    writer.WriteEndObject();
                    //entry request
                    writer.WritePropertyName("request");
                    writer.WriteStartObject();
                    writer.WritePropertyName("method");
                    writer.WriteValue("POST");
                    writer.WritePropertyName("url");
                    writer.WriteValue("Patient/" + Pat.id);
                    writer.WriteEndObject();
                    //entry resource end
                    writer.WriteEndObject();


                    //Composition complist.Count
                    for (int i = 0; i < complist.Count; i++)
                    {
                        //entry resource start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resource");

                        //Composition start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resourceType");
                        writer.WriteValue("Composition");
                        writer.WritePropertyName("id");
                        writer.WriteValue(complist[i].id);
                        //meta  更新版本時間
                        writer.WritePropertyName("meta");
                        writer.WriteStartObject();
                        writer.WritePropertyName("lastUpdated");
                        writer.WriteValue(Bun.meta);
                        writer.WriteEndObject();
                        writer.WritePropertyName("status");
                        writer.WriteValue("final");

                        writer.WritePropertyName("type");
                        writer.WriteStartObject();  //type
                        writer.WritePropertyName("coding");
                        writer.WriteStartArray();
                        writer.WriteStartObject();
                        writer.WritePropertyName("system");
                        writer.WriteValue("http://loinc.or");
                        writer.WritePropertyName("code");
                        writer.WriteValue("11503-0");
                        writer.WriteEndObject();
                        writer.WriteEndArray();
                        writer.WriteEndObject();    //type
                        //subject
                        writer.WritePropertyName("subject");
                        writer.WriteStartObject();  //subject
                        writer.WritePropertyName("reference");
                        writer.WriteValue(complist[i].patient);
                        writer.WriteEndObject();    //subject
                        //encounter
                        if (complist[i].encounter != null)
                        {
                            writer.WritePropertyName("encounter");
                            writer.WriteStartObject();  //encounter
                            writer.WritePropertyName("reference");
                            writer.WriteValue(complist[i].encounter);
                            writer.WriteEndObject();    //encounter
                        }
                        //date
                        writer.WritePropertyName("date");
                        writer.WriteValue(complist[i].date);
                        //author
                        writer.WritePropertyName("author");
                        writer.WriteStartArray();
                        writer.WriteStartObject();  //author
                        writer.WritePropertyName("reference");
                        writer.WriteValue(complist[i].author);
                        writer.WriteEndObject();    //author
                        writer.WriteEndArray();
                        //title
                        writer.WritePropertyName("title");
                        writer.WriteValue(complist[i].title);
                        //section
                        writer.WritePropertyName("section");
                        writer.WriteStartArray();   //section array start
                        string com_title = "";
                        for (int j = 0; j < complist[i].section_entry.Length; j++)
                        {
                            if (com_title != complist[i].section_entry[j].Split('/')[0])
                            {
                                com_title = complist[i].section_entry[j].Split('/')[0];
                                writer.WriteStartObject();  //section start
                                //title
                                writer.WritePropertyName("title");
                                writer.WriteValue(com_title);
                                //entry
                                writer.WritePropertyName("entry");
                                writer.WriteStartArray();

                                while (com_title == complist[i].section_entry[j].Split('/')[0])
                                {
                                    writer.WriteStartObject();  //entry
                                    writer.WritePropertyName("reference");
                                    writer.WriteValue(complist[i].section_entry[j]);
                                    writer.WriteEndObject();    //entry
                                    j++;
                                    if (j == complist[i].section_entry.Length)
                                    {
                                        break;
                                    }
                                }
                                j--;

                                writer.WriteEndArray();
                                writer.WriteEndObject();    //sectionend
                            }

                        }
                        writer.WriteEndArray(); //section array end
                        //Composition end              
                        writer.WriteEndObject();
                        //entry request
                        writer.WritePropertyName("request");
                        writer.WriteStartObject();
                        writer.WritePropertyName("method");
                        writer.WriteValue("POST");
                        writer.WritePropertyName("url");
                        writer.WriteValue("Composition/" + complist[i].id);
                        writer.WriteEndObject();
                        //entry resource end
                        writer.WriteEndObject();
                    }

                    //Encounter
                    for (int i = 0; i < enclist.Count; i++)
                    {
                        //entry resource start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resource");

                        //Encounter start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resourceType");
                        writer.WriteValue("Encounter");
                        writer.WritePropertyName("id");
                        writer.WriteValue(enclist[i].id);
                        //meta  更新版本時間
                        writer.WritePropertyName("meta");
                        writer.WriteStartObject();
                        writer.WritePropertyName("lastUpdated");
                        writer.WriteValue(Bun.meta);
                        writer.WriteEndObject();
                        writer.WritePropertyName("status");
                        writer.WriteValue("finished");

                        //class  就診種類 AMB ambulatory or IMP inpatient encounter
                        writer.WritePropertyName("class");
                        writer.WriteStartObject();  //class
                        writer.WritePropertyName("system");
                        writer.WriteValue("https://www.hl7.org/fhir/v3/ActEncounterCode/vs.html");
                        writer.WritePropertyName("code");
                        int imp_ys = 0;  //判斷是否是住院資料  用於type
                        if (enclist[i].en_class == "ambulatory")
                        {
                            writer.WriteValue("AMB");
                            imp_ys = 0;
                        }
                        else
                        {
                            writer.WriteValue("IMP");
                            imp_ys = 1;
                        }
                        writer.WritePropertyName("display");
                        writer.WriteValue(enclist[i].en_class);
                        writer.WriteEndObject();    //class
                        if (imp_ys == 1)
                        {
                            //type for r2_1.4/r2_1.5
                            writer.WritePropertyName("type");
                            writer.WriteStartObject();  //type
                            writer.WritePropertyName("coding");
                            writer.WriteStartArray();
                            writer.WriteStartObject();
                            writer.WritePropertyName("system");
                            writer.WriteValue("http://loinc.or");
                            writer.WritePropertyName("code");
                            writer.WriteValue("11503-0");
                            writer.WriteEndObject();
                            writer.WriteEndArray();
                            writer.WriteEndObject();    //type
                        }
                        //subject
                        writer.WritePropertyName("subject");
                        writer.WriteStartObject();  //subject
                        writer.WritePropertyName("reference");
                        writer.WriteValue(enclist[i].subject);
                        writer.WriteEndObject();    //subject
                        //period
                        writer.WritePropertyName("period");
                        writer.WriteStartObject();  //period
                        writer.WritePropertyName("start");
                        writer.WriteValue(enclist[i].period_start);
                        writer.WritePropertyName("end");
                        writer.WriteValue(enclist[i].period_end);
                        writer.WriteEndObject();    //period
                        //reasonReference
                        writer.WritePropertyName("reasonReference");
                        writer.WriteStartArray();
                        for (int j = 0; j < enclist[i].reasonReference.Length; j++)
                        {
                            writer.WriteStartObject();
                            writer.WritePropertyName("reference");
                            writer.WriteValue(enclist[i].reasonReference[j]);
                            writer.WriteEndObject();
                        }
                        writer.WriteEndArray();
                        //diagnosis
                        if (enclist[i].diagnosis != null)
                        {
                            writer.WritePropertyName("diagnosis");
                            writer.WriteStartArray();
                            for (int j = 0; j < enclist[i].diagnosis.Length; j++)
                            {
                                writer.WriteStartObject();
                                writer.WritePropertyName("condition");
                                writer.WriteStartObject();
                                writer.WritePropertyName("reference");
                                writer.WriteValue(enclist[i].diagnosis[j]);
                                writer.WriteEndObject();
                                writer.WriteEndObject();
                            }
                            writer.WriteEndArray();
                        }
                        if (enclist[i].Organization != null)
                        {
                            //serviceProvider
                            writer.WritePropertyName("serviceProvider");
                            writer.WriteStartObject();
                            writer.WritePropertyName("reference");
                            writer.WriteValue(enclist[i].Organization);
                            writer.WriteEndObject();
                        }


                        //Encounter end              
                        writer.WriteEndObject();
                        //entry request
                        writer.WritePropertyName("request");
                        writer.WriteStartObject();
                        writer.WritePropertyName("method");
                        writer.WriteValue("POST");
                        writer.WritePropertyName("url");
                        writer.WriteValue("Encounter/" + enclist[i].id);
                        writer.WriteEndObject();
                        //entry resource end
                        writer.WriteEndObject();

                    }

                    //Condition
                    for (int i = 0; i < conlist.Count; i++)
                    {
                        //entry resource start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resource");

                        //Condition start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resourceType");
                        writer.WriteValue("Condition");
                        writer.WritePropertyName("id");
                        writer.WriteValue(conlist[i].id);
                        //meta  更新版本時間
                        writer.WritePropertyName("meta");
                        writer.WriteStartObject();
                        writer.WritePropertyName("lastUpdated");
                        writer.WriteValue(Bun.meta);
                        writer.WriteEndObject();
                        //code
                        writer.WritePropertyName("code");
                        writer.WriteStartObject();
                        writer.WritePropertyName("coding");
                        writer.WriteStartArray();
                        writer.WriteStartObject();
                        writer.WritePropertyName("code");
                        writer.WriteValue(conlist[i].code_code);
                        writer.WritePropertyName("display");
                        writer.WriteValue(conlist[i].code_diplay);
                        writer.WriteEndObject();
                        writer.WriteEndArray();
                        writer.WriteEndObject();    //code
                        //subject
                        writer.WritePropertyName("subject");
                        writer.WriteStartObject();  //subject
                        writer.WritePropertyName("reference");
                        writer.WriteValue(conlist[i].subject);
                        writer.WriteEndObject();    //subject
                        //encounter
                        writer.WritePropertyName("encounter");
                        writer.WriteStartObject();  //encounter
                        writer.WritePropertyName("reference");
                        writer.WriteValue(conlist[i].encounter);
                        writer.WriteEndObject();    //encounter
                        //recordedDate
                        writer.WritePropertyName("recordedDate");
                        writer.WriteValue(conlist[i].recoredDate);

                        //Condition end              
                        writer.WriteEndObject();
                        //entry request
                        writer.WritePropertyName("request");
                        writer.WriteStartObject();
                        writer.WritePropertyName("method");
                        writer.WriteValue("POST");
                        writer.WritePropertyName("url");
                        writer.WriteValue("Condition/" + conlist[i].id);
                        writer.WriteEndObject();
                        //entry resource end
                        writer.WriteEndObject();

                    }

                    //Observation
                    for (int i = 0; i < obslist.Count; i++)
                    {
                        //entry resource start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resource");

                        //Observation start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resourceType");
                        writer.WriteValue("Observation");
                        writer.WritePropertyName("id");
                        writer.WriteValue(obslist[i].id);
                        //meta  更新版本時間
                        writer.WritePropertyName("meta");
                        writer.WriteStartObject();
                        writer.WritePropertyName("lastUpdated");
                        writer.WriteValue(Bun.meta);
                        writer.WriteEndObject();
                        writer.WritePropertyName("status");
                        writer.WriteValue("final");
                        //category
                        writer.WritePropertyName("category");
                        writer.WriteStartArray();
                        writer.WriteStartObject();
                        writer.WritePropertyName("text");
                        writer.WriteValue(obslist[i].category);
                        writer.WriteEndObject();
                        writer.WriteEndArray();
                        //code
                        writer.WritePropertyName("code");
                        writer.WriteStartObject();
                        if (obslist[i].code_code != null)
                        {
                            writer.WritePropertyName("coding");
                            writer.WriteStartArray();
                            writer.WriteStartObject();
                            writer.WritePropertyName("code");
                            writer.WriteValue(obslist[i].code_code);
                            writer.WritePropertyName("display");
                            writer.WriteValue(obslist[i].code_diplay);
                            writer.WriteEndObject();
                            writer.WriteEndArray();
                        }
                        else if (obslist[i].code_diplay != null)
                        {
                            writer.WritePropertyName("text");
                            writer.WriteValue(obslist[i].code_diplay);
                        }
                        writer.WriteEndObject();
                        //subject
                        writer.WritePropertyName("subject");
                        writer.WriteStartObject();  //subject
                        writer.WritePropertyName("reference");
                        writer.WriteValue(obslist[i].subject);
                        writer.WriteEndObject();    //subject
                        //encounter
                        writer.WritePropertyName("encounter");
                        writer.WriteStartObject();  //encounter
                        writer.WritePropertyName("reference");
                        writer.WriteValue(obslist[i].encounter);
                        writer.WriteEndObject();    //encounter
                        //effectiveDateTime
                        writer.WritePropertyName("effectiveDateTime");
                        writer.WriteValue(obslist[i].effectiveDateTime);
                        //performer
                        if (obslist[i].performer != null)
                        {
                            writer.WritePropertyName("performer");
                            writer.WriteStartArray();
                            writer.WriteStartObject();
                            writer.WritePropertyName("reference");
                            writer.WriteValue(obslist[i].performer);
                            writer.WriteEndObject();
                            writer.WriteEndArray();
                        }
                        //note
                        if (obslist[i].note != null)
                        {
                            writer.WritePropertyName("note");
                            writer.WriteStartArray();
                            writer.WriteStartObject();
                            writer.WritePropertyName("text");
                            writer.WriteValue(obslist[i].note);
                            writer.WriteEndObject();
                            writer.WriteEndArray();
                        }
                        //component
                        if (obslist[i].com_code.Length != 0)
                        {
                            writer.WritePropertyName("component");
                            writer.WriteStartArray();
                            for (int j = 0; j < obslist[i].com_code.Length; j++)
                            {
                                writer.WriteStartObject();
                                //code
                                if (obslist[i].com_code[j] != null || obslist[i].com_code_display[j] != null)
                                {
                                    writer.WritePropertyName("code");
                                    writer.WriteStartObject();
                                    if (obslist[i].com_code[j] != null)
                                    {
                                        writer.WritePropertyName("coding");
                                        writer.WriteStartArray();
                                        writer.WriteStartObject();
                                        writer.WritePropertyName("code");
                                        writer.WriteValue(obslist[i].com_code[j]);
                                        writer.WritePropertyName("display");
                                        writer.WriteValue(obslist[i].com_code_display[j]);
                                        writer.WriteEndObject();
                                        writer.WriteEndArray();
                                    }
                                    else if (obslist[i].com_code_display[j] != null)
                                    {
                                        writer.WritePropertyName("text");
                                        writer.WriteValue(obslist[i].com_code_display[j]);
                                    }
                                    writer.WriteEndObject();
                                }
                                //valueQuantity
                                if (obslist[i].valueQuantity[j] != null)
                                {
                                    writer.WritePropertyName("valueQuantity");
                                    writer.WriteStartObject();
                                    writer.WritePropertyName("text");
                                    writer.WriteValue(obslist[i].valueQuantity[j]);
                                    writer.WriteEndObject();
                                }
                                //interpretation
                                if (obslist[i].interpretation[j] != null)
                                {
                                    writer.WritePropertyName("interpretation");
                                    writer.WriteStartArray();
                                    writer.WriteStartObject();
                                    writer.WritePropertyName("text");
                                    writer.WriteValue(obslist[i].interpretation[j]);
                                    writer.WriteEndObject();
                                    writer.WriteEndArray();
                                }
                                //referenceRange
                                if (obslist[i].referenceRange_text[j] != null)
                                {
                                    writer.WritePropertyName("referenceRange");
                                    writer.WriteStartArray();
                                    writer.WriteStartObject();
                                    writer.WritePropertyName("text");
                                    writer.WriteValue(obslist[i].referenceRange_text[j]);
                                    writer.WriteEndObject();
                                    writer.WriteEndArray();
                                }
                                writer.WriteEndObject();
                            }
                            writer.WriteEndArray();
                        }
                        //Observation end              
                        writer.WriteEndObject();
                        //entry request
                        writer.WritePropertyName("request");
                        writer.WriteStartObject();
                        writer.WritePropertyName("method");
                        writer.WriteValue("POST");
                        writer.WritePropertyName("url");
                        writer.WriteValue("Observation/" + obslist[i].id);
                        writer.WriteEndObject();
                        //entry resource end
                        writer.WriteEndObject();
                    }

                    //Procedure
                    for (int i = 0; i < prolist.Count; i++)
                    {
                        //entry resource start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resource");

                        //Procedure start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resourceType");
                        writer.WriteValue("Procedure");
                        writer.WritePropertyName("id");
                        writer.WriteValue(prolist[i].id);
                        //meta  更新版本時間
                        writer.WritePropertyName("meta");
                        writer.WriteStartObject();
                        writer.WritePropertyName("lastUpdated");
                        writer.WriteValue(Bun.meta);
                        writer.WriteEndObject();
                        writer.WritePropertyName("status");
                        writer.WriteValue("completed");
                        //code
                        writer.WritePropertyName("code");
                        writer.WriteStartObject();
                        writer.WritePropertyName("coding");
                        writer.WriteStartArray();
                        writer.WriteStartObject();
                        writer.WritePropertyName("code");
                        writer.WriteValue(prolist[i].code_code);
                        writer.WritePropertyName("display");
                        writer.WriteValue(prolist[i].code_diplay);
                        writer.WriteEndObject();
                        writer.WriteEndArray();
                        writer.WriteEndObject();    //code
                        //subject
                        writer.WritePropertyName("subject");
                        writer.WriteStartObject();  //subject
                        writer.WritePropertyName("reference");
                        writer.WriteValue(prolist[i].subject);
                        writer.WriteEndObject();    //subject
                        //encounter
                        writer.WritePropertyName("encounter");
                        writer.WriteStartObject();  //encounter
                        writer.WritePropertyName("reference");
                        writer.WriteValue(prolist[i].encounter);
                        writer.WriteEndObject();    //encounter
                        //performedPeriod
                        writer.WritePropertyName("performedPeriod");
                        writer.WriteStartObject();  //period
                        writer.WritePropertyName("start");
                        writer.WriteValue(prolist[i].performedPeriod_start);
                        writer.WritePropertyName("end");
                        writer.WriteValue(prolist[i].performedPeriod_end);
                        writer.WriteEndObject();    //period
                        //performer
                        writer.WritePropertyName("performer");
                        writer.WriteStartArray();
                        writer.WriteStartObject();
                        writer.WritePropertyName("actor");
                        writer.WriteStartObject();
                        writer.WritePropertyName("reference");
                        writer.WriteValue(prolist[i].actor);
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                        writer.WriteEndArray();
                        //reasonReference
                        writer.WritePropertyName("reasonReference");
                        writer.WriteStartObject();
                        writer.WritePropertyName("reference");
                        writer.WriteValue(prolist[i].reasonReference);
                        writer.WriteEndObject();
                        //bodySite
                        if (prolist[i].bodySite_code != null)
                        {
                            writer.WritePropertyName("bodySite");
                            writer.WriteStartArray();
                            writer.WriteStartObject();
                            writer.WritePropertyName("coding");
                            writer.WriteStartArray();
                            writer.WriteStartObject();
                            writer.WritePropertyName("code");
                            writer.WriteValue(prolist[i].bodySite_code);
                            writer.WritePropertyName("display");
                            writer.WriteValue(prolist[i].bodySite_display);
                            writer.WriteEndObject();
                            writer.WriteEndArray();
                            writer.WriteEndObject();
                            writer.WriteEndArray();
                        }


                        //Procedure end              
                        writer.WriteEndObject();
                        //entry request
                        writer.WritePropertyName("request");
                        writer.WriteStartObject();
                        writer.WritePropertyName("method");
                        writer.WriteValue("POST");
                        writer.WritePropertyName("url");
                        writer.WriteValue("Procedure/" + prolist[i].id);
                        writer.WriteEndObject();
                        //entry resource end
                        writer.WriteEndObject();

                    }

                    //MedicationRequest
                    for (int i = 0; i < medlist.Count; i++)
                    {
                        //entry resource start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resource");

                        //MedicationRequest start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resourceType");
                        writer.WriteValue("MedicationRequest");
                        writer.WritePropertyName("id");
                        writer.WriteValue(medlist[i].id);
                        //meta  更新版本時間
                        writer.WritePropertyName("meta");
                        writer.WriteStartObject();
                        writer.WritePropertyName("lastUpdated");
                        writer.WriteValue(Bun.meta);
                        writer.WriteEndObject();
                        //contained
                        writer.WritePropertyName("contained");
                        writer.WriteStartArray();
                        writer.WriteStartObject();
                        //Medication
                        writer.WritePropertyName("resourceType");
                        writer.WriteValue("Medication");
                        writer.WritePropertyName("id");
                        writer.WriteValue(medlist[i].Medication_id);
                        writer.WritePropertyName("code");
                        writer.WriteStartObject();
                        writer.WritePropertyName("coding");
                        writer.WriteStartArray();
                        writer.WriteStartObject();
                        writer.WritePropertyName("code");
                        writer.WriteValue(medlist[i].Medication_code);
                        writer.WritePropertyName("display");
                        writer.WriteValue(medlist[i].Medication_display);
                        writer.WriteEndObject();
                        writer.WriteEndArray();
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                        writer.WriteEndArray();

                        writer.WritePropertyName("status");
                        writer.WriteValue("completed");
                        //intent
                        writer.WritePropertyName("intent");
                        writer.WriteValue("order");
                        //code
                        writer.WritePropertyName("medicationReference");
                        writer.WriteStartObject();
                        writer.WritePropertyName("reference");
                        writer.WriteValue(medlist[i].medicationReference);
                        writer.WriteEndObject();
                        //subject
                        writer.WritePropertyName("subject");
                        writer.WriteStartObject();  //subject
                        writer.WritePropertyName("reference");
                        writer.WriteValue(medlist[i].subject);
                        writer.WriteEndObject();    //subject
                        //encounter
                        writer.WritePropertyName("encounter");
                        writer.WriteStartObject();  //encounter
                        writer.WritePropertyName("reference");
                        writer.WriteValue(medlist[i].encounter);
                        writer.WriteEndObject();    //encounter
                        //authoredOn
                        writer.WritePropertyName("authoredOn");
                        writer.WriteValue(medlist[i].authoredOn);
                        //dispenseRequest
                        writer.WritePropertyName("dispenseRequest");
                        writer.WriteStartObject();
                        writer.WritePropertyName("quantity");
                        writer.WriteStartObject();
                        writer.WritePropertyName("value");
                        writer.WriteValue(medlist[i].quantity);
                        writer.WriteEndObject();
                        writer.WritePropertyName("expectedSupplyDuration");
                        writer.WriteStartObject();
                        writer.WritePropertyName("value");
                        writer.WriteValue(medlist[i].expectedSupplyDuration);
                        writer.WriteEndObject();
                        writer.WriteEndObject();


                        //MedicationRequest end              
                        writer.WriteEndObject();
                        //entry request
                        writer.WritePropertyName("request");
                        writer.WriteStartObject();
                        writer.WritePropertyName("method");
                        writer.WriteValue("POST");
                        writer.WritePropertyName("url");
                        writer.WriteValue("MedicationRequest/" + medlist[i].id);
                        writer.WriteEndObject();
                        //entry resource end
                        writer.WriteEndObject();

                    }

                    //AllergyIntolerance
                    for (int i = 0; i < allergylist.Count; i++)
                    {
                        //entry resource start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resource");

                        //AllergyIntolerance start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resourceType");
                        writer.WriteValue("AllergyIntolerance");
                        writer.WritePropertyName("id");
                        writer.WriteValue(allergylist[i].id);
                        //meta  更新版本時間
                        writer.WritePropertyName("meta");
                        writer.WriteStartObject();
                        writer.WritePropertyName("lastUpdated");
                        writer.WriteValue(Bun.meta);
                        writer.WriteEndObject();
                        //code
                        writer.WritePropertyName("code");
                        writer.WriteStartObject();
                        writer.WritePropertyName("text");
                        writer.WriteValue(allergylist[i].code);
                        writer.WriteEndObject();    //code
                        //patient
                        writer.WritePropertyName("patient");
                        writer.WriteStartObject();  //patient
                        writer.WritePropertyName("reference");
                        writer.WriteValue(allergylist[i].patient);
                        writer.WriteEndObject();    //patient
                        //recordedDate
                        writer.WritePropertyName("recordedDate");
                        writer.WriteValue(allergylist[i].recordedDate);
                        //recorder
                        writer.WritePropertyName("recorder");
                        writer.WriteValue(allergylist[i].recorder);

                        //AllergyIntolerance end              
                        writer.WriteEndObject();
                        //entry request
                        writer.WritePropertyName("request");
                        writer.WriteStartObject();
                        writer.WritePropertyName("method");
                        writer.WriteValue("POST");
                        writer.WritePropertyName("url");
                        writer.WriteValue("AllergyIntolerance" + allergylist[i].id);
                        writer.WriteEndObject();
                        //entry resource end
                        writer.WriteEndObject();

                    }

                    //ImmunizationRecommendation
                    for (int i = 0; i < immlist.Count; i++)
                    {
                        //entry resource start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resource");

                        //ImmunizationRecommendation start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resourceType");
                        writer.WriteValue("ImmunizationRecommendation");
                        writer.WritePropertyName("id");
                        writer.WriteValue(immlist[i].id);
                        //meta  更新版本時間
                        writer.WritePropertyName("meta");
                        writer.WriteStartObject();
                        writer.WritePropertyName("lastUpdated");
                        writer.WriteValue(Bun.meta);
                        writer.WriteEndObject();
                        //patient
                        writer.WritePropertyName("patient");
                        writer.WriteStartObject();  //patient
                        writer.WritePropertyName("reference");
                        writer.WriteValue(immlist[i].patient);
                        writer.WriteEndObject();    //patient
                        //occurrenceDateTime
                        writer.WritePropertyName("date");
                        writer.WriteValue(immlist[i].occurrenceDateTime);
                        //recommendation
                        writer.WritePropertyName("recommendation");
                        writer.WriteStartArray();
                        writer.WriteStartObject();
                        //vaccineCode
                        writer.WritePropertyName("vaccineCode");
                        writer.WriteStartArray();
                        writer.WriteStartObject();
                        writer.WritePropertyName("text");
                        writer.WriteValue(immlist[i].vaccineCode);
                        writer.WriteEndObject();
                        writer.WriteEndArray();
                        //end vaccineCode
                        writer.WriteEndObject();
                        writer.WriteEndArray();

                        //ImmunizationRecommendation end              
                        writer.WriteEndObject();
                        //entry request
                        writer.WritePropertyName("request");
                        writer.WriteStartObject();
                        writer.WritePropertyName("method");
                        writer.WriteValue("POST");
                        writer.WritePropertyName("url");
                        writer.WriteValue("ImmunizationRecommendation/" + immlist[i].id);
                        writer.WriteEndObject();
                        //entry resource end
                        writer.WriteEndObject();

                    }

                    //Coverage
                    for (int i = 0; i < covlist.Count; i++)
                    {
                        //entry resource start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resource");

                        //Coverage start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resourceType");
                        writer.WriteValue("Coverage");
                        writer.WritePropertyName("id");
                        writer.WriteValue(covlist[i].id);
                        //meta  更新版本時間
                        writer.WritePropertyName("meta");
                        writer.WriteStartObject();
                        writer.WritePropertyName("lastUpdated");
                        writer.WriteValue(Bun.meta);
                        writer.WriteEndObject();
                        writer.WritePropertyName("status");
                        writer.WriteValue("active");
                        //subscriber
                        writer.WritePropertyName("subscriber");
                        writer.WriteStartObject();
                        writer.WritePropertyName("reference");
                        writer.WriteValue(covlist[i].subscriber);
                        writer.WriteEndObject();
                        //beneficiary
                        writer.WritePropertyName("beneficiary");
                        writer.WriteStartObject();
                        writer.WritePropertyName("reference");
                        writer.WriteValue(covlist[i].beneficiary);
                        writer.WriteEndObject();
                        //period
                        writer.WritePropertyName("period");
                        writer.WriteStartObject();  //period
                        writer.WritePropertyName("start");
                        writer.WriteValue(covlist[i].period_start);
                        writer.WritePropertyName("end");
                        writer.WriteValue(covlist[i].period_end);
                        writer.WriteEndObject();    //period
                        //payor
                        writer.WritePropertyName("payor");
                        writer.WriteStartObject();
                        writer.WritePropertyName("reference");
                        writer.WriteValue(covlist[i].payor);
                        writer.WriteEndObject();
                        //costToBeneficiary
                        writer.WritePropertyName("costToBeneficiary");
                        writer.WriteStartArray();
                        writer.WriteStartObject();
                        writer.WritePropertyName("valueMoney");
                        writer.WriteStartObject();
                        writer.WritePropertyName("value");
                        writer.WriteValue(covlist[i].valueMoney);
                        writer.WritePropertyName("currency");
                        if (covlist[i].payor.Split('/')[0] == "Organization")
                        {
                            writer.WriteValue("Taiwan Insurence's Point");
                        }
                        else
                        {
                            writer.WriteValue("NT");
                        }
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                        writer.WriteEndArray();


                        //Coverage end              
                        writer.WriteEndObject();
                        //entry request
                        writer.WritePropertyName("request");
                        writer.WriteStartObject();
                        writer.WritePropertyName("method");
                        writer.WriteValue("POST");
                        writer.WritePropertyName("url");
                        writer.WriteValue("Coverage/" + covlist[i].id);
                        writer.WriteEndObject();
                        //entry resource end
                        writer.WriteEndObject();

                    }

                    //Organization
                    for (int i = 0; i < orglist.Count; i++)
                    {
                        //entry resource start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resource");

                        //Organization start
                        writer.WriteStartObject();
                        writer.WritePropertyName("resourceType");
                        writer.WriteValue("Organization");
                        writer.WritePropertyName("id");
                        writer.WriteValue(orglist[i].id);
                        //meta  更新版本時間
                        writer.WritePropertyName("meta");
                        writer.WriteStartObject();
                        writer.WritePropertyName("lastUpdated");
                        writer.WriteValue(Bun.meta);
                        writer.WriteEndObject();
                        //type
                        if (orglist[i].type != null)
                        {
                            writer.WritePropertyName("type");
                            writer.WriteStartArray();
                            writer.WriteStartObject();
                            writer.WritePropertyName("text");
                            writer.WriteValue(orglist[i].type);
                            writer.WriteEndObject();
                            writer.WriteEndArray();
                        }
                        //name
                        writer.WritePropertyName("name");
                        writer.WriteValue(orglist[i].name);
                        //telecom
                        if (orglist[i].telecom != null)
                        {
                            writer.WritePropertyName("telecom");
                            writer.WriteStartArray();
                            writer.WriteStartObject();
                            writer.WritePropertyName("system");
                            writer.WriteValue("phone");
                            writer.WritePropertyName("value");
                            writer.WriteValue(orglist[i].telecom);
                            writer.WriteEndObject();
                            writer.WriteEndArray();
                        }
                        //address
                        if (orglist[i].address != null)
                        {
                            writer.WritePropertyName("address");
                            writer.WriteStartArray();
                            writer.WriteStartObject();
                            writer.WritePropertyName("line");
                            writer.WriteStartArray();
                            writer.WriteValue(orglist[i].address);
                            writer.WriteEndArray();
                            writer.WriteEndObject();
                            writer.WriteEndArray();
                        }


                        //Organization end              
                        writer.WriteEndObject();
                        //entry request
                        writer.WritePropertyName("request");
                        writer.WriteStartObject();
                        writer.WritePropertyName("method");
                        writer.WriteValue("POST");
                        writer.WritePropertyName("url");
                        writer.WriteValue("Organization/" + orglist[i].id);
                        writer.WriteEndObject();
                        //entry resource end
                        writer.WriteEndObject();

                    }


                    //Bundle entry array end
                    writer.WriteEndArray();
                    //Bundle end
                    writer.WriteEndObject();


                    writer.Flush();
                    writer.Close();
                    sw.Flush();
                    sw.Close();

                    MHB_Json = sw.ToString();
                }
            }
            return MHB_Json;
        }

        //讀取FHIR的Json檔案
        public void ReadJson(string UploadData_path)
        {
            string PHRPath = "";
            //var db = new ApplicationDbContext();


            PHRPath = Server.MapPath("~/FHIR/" + UploadData_path);
            //PHRPath = Server.MapPath("~/FHIR/"+PHRPath);
            //讀取JSON檔案
            using (StreamReader reader = new StreamReader(PHRPath, Encoding.UTF8))
            {
                //將 Json 文檔解析為 JObject
                JObject fhir = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

                //LINQ語法
                //var com = from task in fhir["entry"].Children()
                //          where task.SelectToken("resource").SelectToken("resourceType").ToString() == "Composition"
                //          select task.SelectToken("resource").ToString();
                //foreach (var value in com)
                //{
                //    test = test + value;
                //}

                //擷取JSON裡面檔案
                //Bundle
                Bun.id = fhir["id"].ToString();
                Bun.meta = fhir["meta"]["lastUpdated"].ToString();


                //Bundle裡所有entry
                foreach (JToken entry in fhir["entry"])
                {
                    //Patient Data
                    if (entry["resource"]["resourceType"].ToString() == "Patient")
                    {
                        //Patient 變數
                        int pat_ex = 0;
                        Pat.valueDate = new string[pat_ex];
                        Pat.valuestring = new string[pat_ex];
                        Pat.id = entry["resource"]["id"].ToString();
                        if (entry["resource"]["extension"] != null)
                        {
                            foreach (JToken extension in entry["resource"]["extension"])
                            {
                                pat_ex++;
                                var pat_date = Pat.valueDate;
                                var pat_string = Pat.valuestring;
                                Array.Resize(ref pat_date, pat_ex);
                                Array.Resize(ref pat_string, pat_ex);
                                Pat.valueDate = pat_date;
                                Pat.valuestring = pat_string;
                                Pat.valueDate[pat_ex - 1] = extension["valueDate"].ToString();
                                Pat.valuestring[pat_ex - 1] = extension["valuestring"].ToString();
                            }
                        }
                        Pat.name = entry["resource"]["name"].ToString();
                        Pat.gender = entry["resource"]["gender"].ToString();
                        Pat.birthDate = entry["resource"]["birthDate"].ToString();
                    }
                    //Composition
                    else if (entry["resource"]["resourceType"].ToString() == "Composition")
                    {
                        Comp = new Composition_MHB();
                        //陣列變數初始化
                        int com_num = 0;
                        Comp.section_entry = new string[com_num];

                        Comp.id = entry["resource"]["id"].ToString();
                        Comp.patient = entry["resource"]["subject"]["reference"].ToString();
                        Comp.encounter = entry["resource"]["encounter"] != null ? entry["resource"]["encounter"]["reference"].ToString() : null;
                        Comp.date = entry["resource"]["date"].ToString();
                        Comp.author = entry["resource"]["author"][0]["reference"].ToString();
                        Comp.title = entry["resource"]["title"].ToString();
                        foreach (JToken section in entry["resource"]["section"])
                        {
                            foreach (JToken section_entry in section["entry"])
                            {
                                com_num++;
                                var com_sec_entry = Comp.section_entry;
                                Array.Resize(ref com_sec_entry, com_num);
                                Comp.section_entry = com_sec_entry;
                                Comp.section_entry[com_num - 1] = section_entry["reference"].ToString();
                            }
                        }
                        complist.Add(Comp);
                    }
                    ////Encounter
                    //else if (entry["resource"]["resourceType"].ToString() == "Encounter")
                    //{

                    //}
                    //Condition
                    else if (entry["resource"]["resourceType"].ToString() == "Condition")
                    {
                        Con = new Condition_MHB();
                        Con.id = entry["resource"]["id"].ToString();
                        Con.code_code = entry["resource"]["code"]["coding"][0]["code"].ToString();
                        Con.code_diplay = entry["resource"]["code"]["coding"][0]["display"].ToString();
                        Con.subject = entry["resource"]["subject"]["reference"].ToString();
                        Con.encounter = entry["resource"]["encounter"]["reference"].ToString();
                        Con.recoredDate = entry["resource"]["recordedDate"].ToString();
                        conlist.Add(Con);
                    }
                    //Observation
                    else if (entry["resource"]["resourceType"].ToString() == "Observation")
                    {
                        Obs = new Observation_MHB();
                        //陣列變數初始化
                        int obs_comp_num = 0;
                        Obs.com_code = new string[obs_comp_num];
                        Obs.com_code_display = new string[obs_comp_num];
                        Obs.valueQuantity = new string[obs_comp_num];
                        Obs.referenceRange_text = new string[obs_comp_num];
                        Obs.interpretation = new string[obs_comp_num];

                        Obs.id = entry["resource"]["id"].ToString();
                        Obs.category = entry["resource"]["category"][0]["text"].ToString();
                        Obs.code_code = entry["resource"]["code"]["coding"] != null ? entry["resource"]["code"]["coding"][0]["code"].ToString() : null;
                        Obs.code_diplay = entry["resource"]["code"]["coding"] != null ? entry["resource"]["code"]["coding"][0]["display"].ToString() : entry["resource"]["code"]["text"].ToString();
                        Obs.subject = entry["resource"]["subject"]["reference"].ToString();
                        Obs.encounter = entry["resource"]["encounter"]["reference"].ToString();
                        Obs.effectiveDateTime = entry["resource"]["effectiveDateTime"].ToString();
                        Obs.performer = entry["resource"]["performer"] != null ? entry["resource"]["performer"][0]["reference"].ToString() : null;
                        Obs.note = entry["resource"]["note"] != null ? entry["resource"]["note"][0]["text"].ToString() : null;
                        if (entry["resource"]["component"] != null)
                        {
                            foreach (JToken component in entry["resource"]["component"])
                            {
                                obs_comp_num++;
                                var obs_com_code = Obs.com_code;
                                var obs_com_dispaly = Obs.com_code_display;
                                var obs_valuequan = Obs.valueQuantity;
                                var obs_inter = Obs.interpretation;
                                var obs_refer = Obs.referenceRange_text;
                                Array.Resize(ref obs_com_code, obs_comp_num);
                                Array.Resize(ref obs_com_dispaly, obs_comp_num);
                                Array.Resize(ref obs_valuequan, obs_comp_num);
                                Array.Resize(ref obs_inter, obs_comp_num);
                                Array.Resize(ref obs_refer, obs_comp_num);
                                Obs.com_code = obs_com_code;
                                Obs.com_code_display = obs_com_dispaly;
                                Obs.valueQuantity = obs_valuequan;
                                Obs.interpretation = obs_inter;
                                Obs.referenceRange_text = obs_refer;
                                Obs.com_code[obs_comp_num - 1] = component["code"] != null && component["code"]["coding"] != null ? component["code"]["coding"][0]["code"].ToString() : null;
                                Obs.com_code_display[obs_comp_num - 1] = component["code"] != null ? (component["code"]["coding"] != null ? component["code"]["coding"][0]["display"].ToString() : component["code"]["text"].ToString()) : null;
                                Obs.valueQuantity[obs_comp_num - 1] = component["valueQuantity"] != null ? component["valueQuantity"]["text"].ToString() : null;
                                Obs.interpretation[obs_comp_num - 1] = component["interpretation"] != null ? component["interpretation"][0]["text"].ToString() : null;
                                Obs.referenceRange_text[obs_comp_num - 1] = component["referenceRange"] != null ? component["referenceRange"][0]["text"].ToString() : null;
                            }
                        }
                        obslist.Add(Obs);
                    }
                    //Procedure
                    else if (entry["resource"]["resourceType"].ToString() == "Procedure")
                    {
                        Pro = new Procedure_MHB();
                        Pro.id = entry["resource"]["id"].ToString();
                        Pro.code_code = entry["resource"]["code"]["coding"][0]["code"].ToString();
                        Pro.code_diplay = entry["resource"]["code"]["coding"][0]["display"].ToString();
                        Pro.subject = entry["resource"]["subject"]["reference"].ToString();
                        Pro.encounter = entry["resource"]["encounter"]["reference"].ToString();
                        Pro.performedPeriod_start = entry["resource"]["performedPeriod"]["start"].ToString();
                        Pro.performedPeriod_end = entry["resource"]["performedPeriod"]["end"].ToString();
                        Pro.actor = entry["resource"]["performer"][0]["actor"]["reference"].ToString();
                        Pro.reasonReference = entry["resource"]["reasonReference"]["reference"].ToString();
                        Pro.bodySite_code = entry["resource"]["bodySite"] != null ? entry["resource"]["bodySite"][0]["coding"][0]["code"].ToString() : null;
                        Pro.bodySite_display = entry["resource"]["bodySite"] != null ? entry["resource"]["bodySite"][0]["coding"][0]["display"].ToString() : null;
                        prolist.Add(Pro);
                    }
                    //MedicationRequest
                    else if (entry["resource"]["resourceType"].ToString() == "MedicationRequest")
                    {
                        Med = new MedicationRequest_MHB();
                        Med.id = entry["resource"]["id"].ToString();
                        Med.Medication_id = entry["resource"]["contained"][0]["id"].ToString();
                        Med.Medication_code = entry["resource"]["contained"][0]["code"]["coding"][0]["code"].ToString();
                        Med.Medication_display = entry["resource"]["contained"][0]["code"]["coding"][0]["display"].ToString();
                        Med.medicationReference = entry["resource"]["medicationReference"]["reference"].ToString();
                        Med.subject = entry["resource"]["subject"]["reference"].ToString();
                        Med.encounter = entry["resource"]["encounter"]["reference"].ToString();
                        Med.authoredOn = entry["resource"]["authoredOn"].ToString();
                        Med.quantity = entry["resource"]["dispenseRequest"]["quantity"]["value"].ToString();
                        Med.expectedSupplyDuration = entry["resource"]["dispenseRequest"]["expectedSupplyDuration"]["value"].ToString();
                        medlist.Add(Med);
                    }
                    //AllergyIntolerance
                    else if (entry["resource"]["resourceType"].ToString() == "AllergyIntolerance")
                    {
                        Allergy = new AllergyIntolerance_MHB();
                        Allergy.id = entry["resource"]["id"].ToString();
                        Allergy.code = entry["resource"]["code"]["text"].ToString();
                        Allergy.patient = entry["resource"]["patient"]["reference"].ToString();
                        Allergy.recordedDate = entry["resource"]["recordedDate"].ToString();
                        Allergy.recorder = entry["resource"]["recorder"].ToString();
                        allergylist.Add(Allergy);
                    }
                    //ImmunizationRecommendation
                    else if (entry["resource"]["resourceType"].ToString() == "ImmunizationRecommendation")
                    {
                        Immun = new Immunization_MHB();
                        Immun.id = entry["resource"]["id"].ToString();
                        Immun.vaccineCode = entry["resource"]["recommendation"][0]["vaccineCode"][0]["text"].ToString();
                        Immun.occurrenceDateTime = entry["resource"]["date"].ToString();
                        immlist.Add(Immun);
                    }
                    //Coverage
                    else if (entry["resource"]["resourceType"].ToString() == "Coverage")
                    {
                        Cov = new Coverage_MHB();
                        Cov.id = entry["resource"]["id"].ToString();
                        Cov.subscriber = entry["resource"]["subscriber"]["reference"].ToString();
                        Cov.beneficiary = entry["resource"]["beneficiary"]["reference"].ToString();
                        Cov.period_start = entry["resource"]["period"]["end"].ToString();
                        Cov.period_end = entry["resource"]["period"]["end"].ToString();
                        Cov.payor = entry["resource"]["payor"]["reference"].ToString();
                        Cov.valueMoney = entry["resource"]["costToBeneficiary"][0]["valueMoney"]["value"].ToString();
                        covlist.Add(Cov);
                    }
                    //Organization
                    else if (entry["resource"]["resourceType"].ToString() == "Organization")
                    {
                        Org = new Organization_MHB();
                        Org.id = entry["resource"]["id"].ToString();
                        Org.type = entry["resource"]["type"] != null ? entry["resource"]["type"][0]["text"].ToString() : null;
                        Org.name = entry["resource"]["name"].ToString();
                        Org.telecom = entry["resource"]["telecom"] != null ? entry["resource"]["telecom"][0]["value"].ToString() : null;
                        Org.address = entry["resource"]["address"] != null ? entry["resource"]["address"][0]["line"][0].ToString() : null;
                        orglist.Add(Org);
                    }
                }
            }
        }
    }
}