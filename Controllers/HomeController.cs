using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _211215_williamgaleanomedina.Models;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using System.Net;

namespace _211215_williamgaleanomedina.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// se obtiene desde la configuración de la aplicación, la URL de la API que se va a consumir
        /// </summary>
        string ApiURL = ConfigurationManager.AppSettings["ApiURL"].ToString();
        public ActionResult Index()
        {
            /// <summary>
            /// La vista usa una vista parcial que renderiza una tabla basado en el modelo de la vista, se pasa un listado con el modelo utilizado
            /// </summary>       
            return View(new List<Models.IMEIDataServices>());
        }

        /// <summary>
        /// se utiliza una peticion JSON para mostrar el uso de tablas, aunque el API no retorna un listado, se hace para demostrar el funcionamiento
        /// </summary>
        /// <returns>JSON con lista de IMEIs </returns>
        public ActionResult GetImeiList()
        {

            if (Request.IsAjaxRequest())
            {  
                //Parametros internos del DataTable
                int iDisplayLength = int.Parse(Request.Params["length"]);
                int iDisplayStart = int.Parse(Request.Params["start"]);
                int iSortCol_0 = int.Parse(Request.Params["order[0][column]"]);
                string sSortDir_0 = Request.Params["order[0][dir]"] ?? "asc";
                string sSortColName = Request.Params["columns[" + iSortCol_0 + "][name]"];
                string Search = Request.Params["search[value]"] ?? "";

                //Cargamos los valores de los filtros
                string _IMEI = Request.Params["IMEI"];
                string _CompanyID_p =  Request.Params["CompanyID"];

                int _CompanyID = 0;
                int.TryParse(_CompanyID_p, out _CompanyID);

                //Creamos una clase IMEIRequest que contine la información de la petición
                IMEIRequest ImeiRequest = new IMEIRequest() { IMEI = _IMEI, CompanyID = _CompanyID };

                Models.IMEIDataServices imei = null;

                //Esta sentencia para corregir un error de protocolos TSL (si sale error se comenta, el error tal vez es en mi PC)
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

                //Utilizamos un HttpClient para consumir el API
                using (var client = new HttpClient())
                {
                    string urlApiImei = ApiURL + "GetIMEIDataServicesByIMEIAndCompany";
                    HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(ImeiRequest), Encoding.UTF8);
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var responseTask = client.PostAsync(urlApiImei, httpContent);
                    responseTask.Wait();

                    var _result = responseTask.Result;
                    if (_result.IsSuccessStatusCode)
                    {
                        var readTask = _result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var res = readTask.Result;

                        imei = JsonConvert.DeserializeObject<Models.IMEIDataServices>(res);
                    }
                }

                //creamos el listado que vamos a cargar con los resultados del API
                var list = new List<Models.IMEIDataServices>();

                if(imei!=null)
                {
                    list.Add(imei);
                }

               var lista = list.AsEnumerable();

                var proper = typeof(Models.IMEIDataServices).GetProperty(sSortColName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);


                var Total = lista.Count();

                lista = lista.Skip((iDisplayStart / iDisplayLength) * iDisplayLength)
                             .Take(iDisplayLength);

                var query = (from it in lista
                             select it).AsEnumerable();                                            

                var Result = query;


                var Datos = (from it in Result
                             select new
                             {
                                 IMEI = it.IMEI,
                                 VehicleID = it.VehicleID,
                                 CompanyID = it.CompanyID,  
                                 Botones = ""
                             }).ToArray();



                var datos = new
                {
                    draw = Request.Params["draw"],
                    TotalRecords = Total,
                    recordsTotal = Total,
                    recordsFiltered = Total,
                    totalDisplayRecords = Datos.Length,
                    data = Datos
                };
                var serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = Int32.MaxValue;

                var result = new ContentResult
                {
                    Content = serializer.Serialize(datos),
                    ContentType = "application/json"
                };
                return result; // Json(datos, JsonRequestBehavior.AllowGet);
            }
            return View();

        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}