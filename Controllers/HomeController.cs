using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Trasy.Models;

namespace Trasy.Controllers
{
    public class HomeController : Controller
    {
        public DataContext db = new DataContext();

        public ActionResult Index()
        {

            var trasy = from a in db.trasa
                        select a;

            
            return View(trasy.ToList());
        }
        public ActionResult PridajTrasu()
        {

            
            return View();
        }

        [HttpPost]
        public ActionResult PridajTrasu(Trasy.Models.Trasa t)
        {
            
            db.trasa.Add(t);
            db.SaveChanges();
            return RedirectToAction("ZoznamBodov", "Home", new { id_tr = t.id } );
        }

        public ActionResult ZoznamBodov(int id_tr)
        {
            ZoznamBodovViewModel zb = new ZoznamBodovViewModel();
            var body = from a in db.bod
                       where a.id_trasy == id_tr
                       select a;
            zb.b = body.ToList();
            zb.id = id_tr;


            return View(zb);
        }

        public ActionResult PridajBod(int id)
        {
        

            PridajBodViewModel bod = new PridajBodViewModel();
            bod.id_trasy = id;
            return View(bod);
        }

        [HttpPost]
        public ActionResult PridajBod(Trasy.Models.PridajBodViewModel b)
        {


            return RedirectToAction("SpracujBod", "Home", new { id_tr = b.id_trasy, nazov=b.nazov });
            

        }

        public ActionResult SpracujBod(int id_tr, String nazov)
        {
            SpracujBodViewModel sbvm = new SpracujBodViewModel();
            sbvm.nazov = nazov;
            sbvm.id_trasy = id_tr;

            XDocument xml = XDocument.Parse(NajdiMesta(nazov));
            List<String> mesta = xml.Descendants().Where(x => x.Name.LocalName == "name").Select(x => x.Value.ToString()).ToList();
            sbvm.mesta = mesta;

            XDocument xml2 = XDocument.Parse(NajdiObce(nazov));
            List<String> obce = xml2.Descendants().Where(x => x.Name.LocalName == "name").Select(x => x.Value.ToString()).ToList();
            sbvm.obce = obce;

            if (mesta.Count()==0 && obce.Count()==0) return RedirectToAction("NenajdenyBod", "Home");
            else return View(sbvm);

        }

        public ActionResult NajdiBod(int id_tr, String nazov, Boolean typ)
        {
            Bod bod = new Bod();
            bod.id_trasy = id_tr;
            bod.nazov = nazov;
            bod.typ = typ;
            String isFree;
            String isWeekend;
            

            var pocetbodov = (from a in db.bod
                              where a.id_trasy == id_tr
                              select a).Count();
            bod.poradie = pocetbodov + 1;


            if (typ)
            {
                XDocument xmlMesto = XDocument.Parse(NajdiMesto(nazov));
                var soapResponseArea = xmlMesto.Descendants().Where(x => x.Name.LocalName == "area").FirstOrDefault().Value;
                String responseArea = soapResponseArea.ToString();
                bod.rozloha= float.Parse(responseArea, CultureInfo.InvariantCulture.NumberFormat);

                var soapResponseX = xmlMesto.Descendants().Where(x => x.Name.LocalName == "coord_lat").FirstOrDefault().Value;
                String responseX = soapResponseX.ToString();
                if (responseX == "") return RedirectToAction("NenajdenyBod", "Home");
                bod.suradnica_x = float.Parse(responseX, CultureInfo.InvariantCulture.NumberFormat);
                String surx = (bod.suradnica_x).ToString().Replace(",", ".");

                var soapResponseY = xmlMesto.Descendants().Where(x => x.Name.LocalName == "coord_lon").FirstOrDefault().Value;
                String responseY = soapResponseY.ToString();
                bod.suradnica_y = float.Parse(responseY, CultureInfo.InvariantCulture.NumberFormat);
                String sury = (bod.suradnica_y).ToString().Replace(",", ".");

                XDocument xml = XDocument.Parse(Execute(surx, sury, DateTime.Now.Year.ToString()));
                var soapResponse = xml.Descendants().Where(x => x.Name.LocalName == "average").FirstOrDefault().Value;
                String response = soapResponse.ToString();
                float poc = float.Parse(response, CultureInfo.InvariantCulture.NumberFormat);
                bod.pocasie = (float)Math.Round((double)poc, 1);

                DateTime today = DateTime.Today;
                XDocument xml3 = XDocument.Parse(JeVikend(today.ToString("yyyy-mm-dd")));
                var soapResponse3 = xml3.Descendants().Where(x => x.Name.LocalName == "je_vikend").FirstOrDefault().Value;
                String response3 = soapResponse3.ToString();
                isWeekend = response3;

                XDocument xml4 = XDocument.Parse(JeSviatok(today.Month, today.Day));
                var soapResponse4 = xml4.Descendants().Where(x => x.Name.LocalName == "is_free").FirstOrDefault().Value;
                String response4 = soapResponse4.ToString();
                isFree = response4;

                int area = int.Parse((bod.rozloha).ToString().Replace(",", "."));
                XDocument xml5 = XDocument.Parse(NajdiRestauraciu(area));
                var soapResponse5 = xml5.Descendants().Where(x => x.Name.LocalName == "FindRestaurantResult").FirstOrDefault().Value;
                String response5 = soapResponse5.ToString();
                bod.podnik = Convert.ToBoolean(response5);

                if (bod.podnik)
                {
                    XDocument xml6 = XDocument.Parse(JeOtvorena(isWeekend, isFree));
                    var soapResponse6 = xml6.Descendants().Where(x => x.Name.LocalName == "IsOpenResult").FirstOrDefault().Value;
                    String response6 = soapResponse6.ToString();
                    bod.otvrHodiny = response6;
                }
                else
                {
                    bod.otvrHodiny = "----------";
                }
            }
            else
            {
                XDocument xmlObec = XDocument.Parse(NajdiObec(nazov));
                var soapResponseArea2 = xmlObec.Descendants().Where(x => x.Name.LocalName == "area").FirstOrDefault().Value;
                String responseArea2 = soapResponseArea2.ToString();
                bod.rozloha = float.Parse(responseArea2, CultureInfo.InvariantCulture.NumberFormat);

                var soapResponseX2 = xmlObec.Descendants().Where(x => x.Name.LocalName == "coord_lat").FirstOrDefault().Value;
                String responseX2 = soapResponseX2.ToString();
                if (responseX2 == "") return RedirectToAction("NenajdenyBod", "Home");
                bod.suradnica_x = float.Parse(responseX2, CultureInfo.InvariantCulture.NumberFormat);
                String surx2 = (bod.suradnica_x).ToString().Replace(",", ".");

                var soapResponseY2 = xmlObec.Descendants().Where(x => x.Name.LocalName == "coord_lon").FirstOrDefault().Value;
                String responseY2 = soapResponseY2.ToString();
                bod.suradnica_y = float.Parse(responseY2, CultureInfo.InvariantCulture.NumberFormat);
                String sury2 = (bod.suradnica_y).ToString().Replace(",", ".");

                XDocument xml2 = XDocument.Parse(Execute(surx2, sury2, DateTime.Now.Year.ToString()));
                var soapResponse2 = xml2.Descendants().Where(x => x.Name.LocalName == "average").FirstOrDefault().Value;
                String response2 = soapResponse2.ToString();
                float poc = float.Parse(response2, CultureInfo.InvariantCulture.NumberFormat);
                bod.pocasie = (float)Math.Round((double)poc, 1);

                DateTime today = DateTime.Today;
                XDocument xml3 = XDocument.Parse(JeVikend(today.ToString("yyyy-mm-dd")));
                var soapResponse3 = xml3.Descendants().Where(x => x.Name.LocalName == "je_vikend").FirstOrDefault().Value;
                String response3 = soapResponse3.ToString();
                isWeekend = response3;

                XDocument xml4 = XDocument.Parse(JeSviatok(today.Month, today.Day));
                var soapResponse4 = xml4.Descendants().Where(x => x.Name.LocalName == "is_free").FirstOrDefault().Value;
                String response4 = soapResponse4.ToString();
                isFree = response4;

                int area = int.Parse((bod.rozloha).ToString().Replace(",", "."));
                XDocument xml5 = XDocument.Parse(NajdiRestauraciu(area));
                var soapResponse5 = xml5.Descendants().Where(x => x.Name.LocalName == "FindRestaurantResult").FirstOrDefault().Value;
                String response5 = soapResponse5.ToString();
                bod.podnik = Convert.ToBoolean(response5);

                if (bod.podnik)
                {
                    XDocument xml6 = XDocument.Parse(JeOtvorena(isWeekend, isFree));
                    var soapResponse6 = xml6.Descendants().Where(x => x.Name.LocalName == "IsOpenResult").FirstOrDefault().Value;
                    String response6 = soapResponse6.ToString();
                    bod.otvrHodiny = response6;
                }
                else
                {
                    bod.otvrHodiny = "----------";
                }
                
            }

            db.bod.Add(bod);
            db.SaveChanges();
            return RedirectToAction("ZoznamBodov", "Home", new { id_tr = bod.id_trasy });
        }

        public ActionResult NenajdenyBod()
        {
            

            return View();
        }

        public ActionResult MaloBodov()
        {


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

        public static String Execute(String x, String y, String rok )
        {
            HttpWebRequest request = CreateWebRequest();
            System.Xml.XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:typ=""http://labss2.fiit.stuba.sk/pis/weatherforecast/types"">
               <soap:Header/>
                  <soap:Body>
                    <typ:getAverageTemperature>
                    <year>" + rok + "</year>" +
                    "<coord_lat>" + x + "</coord_lat>" +
                    "<coord_lon>" + y + "</coord_lon>" +
                    "</typ:getAverageTemperature>" +
                  "</soap:Body>" +
                "</soap:Envelope>");

            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();
                    return soapResult;
                }
            }
        }

        public static String NajdiMesto(String nazov)
        {
            HttpWebRequest request = CreateWebRequestCity();
            System.Xml.XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:typ=""http://labss2.fiit.stuba.sk/pis/geoservices/cities/types"">
               <soap:Header/>
               <soap:Body>
                  <typ:getByName> 
                     <name>" + nazov + "</name>" +
                  "</typ:getByName>" +
               "</soap:Body>" +
            "</soap:Envelope> ");



            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();
                    return soapResult;
                }
            }
        }

        public static String NajdiMesta(String nazov)
        {
            HttpWebRequest request = CreateWebRequestCities();
            System.Xml.XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:typ=""http://labss2.fiit.stuba.sk/pis/geoservices/cities/types"">
               <soap:Header/>
               <soap:Body>
                  <typ:searchByName> 
                     <name>" + nazov + "</name>" +
                  "</typ:searchByName>" +
               "</soap:Body>" +
            "</soap:Envelope> ");

             

            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();
                    return soapResult;
                }
            }
        }

        public static String NajdiObec(String nazov)
        {
            HttpWebRequest request = CreateWebRequestMunicipality();
            System.Xml.XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:typ=""http://labss2.fiit.stuba.sk/pis/geoservices/municipalities/types"">
               <soap:Header/>
               <soap:Body>
                  <typ:searchByName>
                     <name>" + nazov + "</name>" +
                  "</typ:searchByName>" +
               "</soap:Body>" +
            "</soap:Envelope>");



            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();
                    return soapResult;
                }
            }
        }

        public static String NajdiObce(String nazov)
        {
            HttpWebRequest request = CreateWebRequestMunicipalities();
            System.Xml.XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:typ=""http://labss2.fiit.stuba.sk/pis/geoservices/municipalities/types"">
               <soap:Header/>
               <soap:Body>
                  <typ:searchByName> 
                     <name>" + nazov + "</name>" +
                  "</typ:searchByName>" +
               "</soap:Body>" +
            "</soap:Envelope>");



            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();
                    return soapResult;
                }
            }
        }

        public static String JeVikend(String datetime)
        {
            HttpWebRequest request = CreateWebRequestIsWeekend();
            System.Xml.XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:typ=""http://labss2.fiit.stuba.sk/pis/calendar/types"">
               <soap:Header/>
               <soap:Body>
                  <typ:isWeekend>
                     <datetime>" + datetime + "</datetime>" +
                  "</typ:isWeekend>" +
                "</soap:Body>" +
             "</soap:Envelope>");

            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();
                    return soapResult;
                }
            }
        }

        public static String JeSviatok(int month, int day)
        {
            HttpWebRequest request = CreateWebRequestIsWeekend();
            System.Xml.XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:typ=""http://labss2.fiit.stuba.sk/pis/calendar/types"">
               <soap:Header/>
               <soap:Body>
                  <typ:isFree>
                     <month>" + month + "</month>" +
                     "<day>" + day + "</day>" +
                  "</typ:isFree>" +
                "</soap:Body>" +
             "</soap:Envelope>");

            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();
                    return soapResult;
                }
            }
        }

        public static String NajdiRestauraciu(int area)
        {
            HttpWebRequest request = CreateWebRequestGetRestaurant();
            System.Xml.XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:xsi= ""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd = ""http://www.w3.org/2001/XMLSchema"" xmlns:soap = ""http://schemas.xmlsoap.org/soap/envelope/"">
               <soap:Body>
                  <FindRestaurant xmlns=""http://microsoft.com/webservices/"">
                      <area>"  + area + "</area>" +
                   "</FindRestaurant>" +
                 "</soap:Body>" +
                "</soap:Envelope>");

            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();
                    return soapResult;
                }
            }
        }

        public static String  JeOtvorena(String isWeekend, String isFree)
        {
            HttpWebRequest request = CreateWebRequestIsOpen();
            System.Xml.XmlDocument soapEnvelopeXml = new XmlDocument();
            soapEnvelopeXml.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                <soap:Body>
                    <IsOpen xmlns=""http://microsoft.com/webservices/"">
                        <isWeekend>" + isWeekend + "</isWeekend>" +
                        "<isFree>" + isFree + "</isFree>" +
                    "</IsOpen>" +
                "</soap:Body>" +
            "</soap:Envelope>");

            using (Stream stream = request.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();
                    return soapResult;
                }
            }
        }

        public static HttpWebRequest CreateWebRequest()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"http://labss2.fiit.stuba.sk/pis/ws/WeatherForecast?WSDL?op=getAverageTemperature");
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        public static HttpWebRequest CreateWebRequestCities()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"http://labss2.fiit.stuba.sk/pis/ws/GeoServices/Cities?WSDL?op=searchByName");
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        public static HttpWebRequest CreateWebRequestCity()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"http://labss2.fiit.stuba.sk/pis/ws/GeoServices/Cities?WSDL?op=getByName");
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        public static HttpWebRequest CreateWebRequestMunicipalities()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"http://labss2.fiit.stuba.sk/pis/ws/GeoServices/Municipalities?WSDL?op=searchByName");
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        public static HttpWebRequest CreateWebRequestMunicipality()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"http://labss2.fiit.stuba.sk/pis/ws/GeoServices/Municipalities?WSDL?op=getByName");
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        public static HttpWebRequest CreateWebRequestIsWeekend()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"http://labss2.fiit.stuba.sk/pis/ws/Calendar?WSDL?op=isWeekend");
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        public static HttpWebRequest CreateWebRequestIsFree()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"http://labss2.fiit.stuba.sk/pis/ws/Calendar?WSDL?op=isFree");
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        public static HttpWebRequest CreateWebRequestGetRestaurant()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"http://localhost:62509/WebService1.asmx?WSDL?op=FindRestaurant");
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        public static HttpWebRequest CreateWebRequestIsOpen()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@"http://localhost:62509/WebService1.asmx?WSDL?op=IsOpen");
            webRequest.Headers.Add(@"SOAP:Action");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }


    }
}