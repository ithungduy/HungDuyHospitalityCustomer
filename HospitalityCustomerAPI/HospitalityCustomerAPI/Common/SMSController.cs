using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace HospitalityCustomerAPI.Common
{
    public class SMSController
    {
        private static string smsURL = "http://123.30.227.9/ws_stttt/ws.asmx";
        private static string smsACTION = "http://sms.giaoductayninh.vn/sendBrandname";
        private static string smsSENDER = "BV_HONGHUNG";
        private static string smsUR = "vnpt_tayninh@123";
        private static string smsPW = "jUzdc%#tvp";
        private static string smsURLPublic = "http://113.185.0.35:8888/smsmarketing/api";

        static public async Task<bool> sendSMS(string phone, string message)
        {
            //return false; // khóa SMS ở đê
            if (phone.Length != 11)
            {
                return false;
            }
            JObject obj = (JObject)JsonConvert.DeserializeObject(await sms_response(phone, message));
            return (string)obj["RPLY"]["ERROR"] == "0";
        }

        static public async Task<string> sms_response(string phone, string message)
        {
            return await sms_responses(new List<string>() { phone }, message);
        }

        static public async Task<string> sms_responses(List<string> phone, string message)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                    RQST = new
                    {
                        name = "send_sms_list",
                        REQID = Utility.RandomString(5),
                        LABELID = "140464",
                        CONTRACTTYPEID = "1",
                        CONTRACTID = "12963",
                        TEMPLATEID = "1055152",
                        PARAMS = new[] {
                            new {  NUM = "1",  CONTENT = Utility.BoDauTiengViet( message) },
                        },
                        SCHEDULETIME = "",
                        MOBILELIST = phone.Count == 1 ? phone.FirstOrDefault() : phone.Aggregate((a, b) => a + "," + b),
                        ISTELCOSUB = "0",
                        AGENTID = "181",
                        APIUSER = "BV_HONGHUNG",
                        APIPASS = "Abc@123",
                        USERNAME = "BV_HONGHUNG",
                        //DATACODING = "8"
                    }
                }), Encoding.UTF8, "application/json"),
            };
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using (HttpClient client = new HttpClient() { BaseAddress = new Uri(smsURLPublic) })
            {
                var response = await client.SendAsync(httpRequestMessage);
                return await response.Content.ReadAsStringAsync();
            }
        }

        static public string sendSMSPrivateAPI_VNPT(string phone, string message)
        {
            string result = "";
            XmlDocument soapEnvelopeXml = CreateSoapEnvelope(smsSENDER, phone, message, smsUR, smsPW);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(smsURL);
            httpWebRequest.Headers.Add("SOAPAction", smsACTION);
            httpWebRequest.ContentType = "text/xml;charset=\"utf-8\"";
            httpWebRequest.Accept = "text/xml";
            httpWebRequest.Method = "POST";
            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, httpWebRequest);
            IAsyncResult asyncResult = httpWebRequest.BeginGetResponse(null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            using (WebResponse webResponse = httpWebRequest.EndGetResponse(asyncResult))
            {
                string text;
                using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream()))
                {
                    text = streamReader.ReadToEnd();
                }
                Console.Write(text);
                XDocument xdocument = XDocument.Parse(text);
                XmlReader xmlReader = xdocument.CreateReader();
                xmlReader.ReadToFollowing("sendBrandnameResult");
                result = xmlReader.ReadElementString();
            }
            return result;
        }
        static public string sendSMSLocal(string phone, string message)
        {
            string result = "";
            string requestUriString = "http://172.16.17.236:8088/xtest.asmx";
            XmlDocument soapEnvelopeXml = CreateSoapEnvelopeLocal(phone, message);
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
            httpWebRequest.ContentType = "text/xml;charset=\"utf-8\"";
            httpWebRequest.Accept = "text/xml";
            httpWebRequest.Method = "POST";
            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, httpWebRequest);
            IAsyncResult asyncResult = httpWebRequest.BeginGetResponse(null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            try
            {
                using (WebResponse webResponse = httpWebRequest.EndGetResponse(asyncResult))
                {
                    string text;
                    using (StreamReader streamReader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        text = streamReader.ReadToEnd();
                    }
                    XDocument xdocument = XDocument.Parse(text);
                    XmlReader xmlReader = xdocument.CreateReader();
                    xmlReader.ReadToFollowing("SEND_SMSResult");
                    result = xmlReader.ReadElementString();
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        static private HttpWebRequest CreateWebRequest(string url, string action)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Headers.Add("SOAPAction", action);
            httpWebRequest.ContentType = "text/xml;charset=\"utf-8\"";
            httpWebRequest.Accept = "text/xml";
            httpWebRequest.Method = "POST";
            return httpWebRequest;
        }
        static private XmlDocument CreateSoapEnvelope(string sender, string phone, string message, string username, string password)
        {
            XmlDocument xmlDocument = new XmlDocument();
            string xml = string.Concat(new string[]
            {
                "<?xml version=\"1.0\" encoding=\"utf-8\"?> <SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" \r\n               xmlns:xsi=\"http://www.w3.org/1999/XMLSchema-instance\" \r\n               xmlns:xsd=\"http://www.w3.org/1999/XMLSchema\">\r\n                    <SOAP-ENV:Body>\r\n                    <sendBrandname  xmlns=\"http://sms.giaoductayninh.vn/\"> \r\n                        <sender>",
                sender,
                "</sender>\r\n                      <phone>",
                phone,
                "</phone>\r\n                      <message> ",
                message,
                "</message>\r\n                      <username>",
                username,
                "</username>\r\n                      <password>",
                password,
                "</password>\r\n                    </sendBrandname>\r\n                    </SOAP-ENV:Body>\r\n                </SOAP-ENV:Envelope>"
            });
            xmlDocument.LoadXml(xml);
            return xmlDocument;
        }

        static private XmlDocument CreateSoapEnvelopeLocal(string phone, string message)
        {
            XmlDocument xmlDocument = new XmlDocument();
            string xml = string.Concat(new string[]
            {
                "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" \r\n               xmlns:xsi=\"http://www.w3.org/1999/XMLSchema-instance\" \r\n               xmlns:xsd=\"http://www.w3.org/1999/XMLSchema\">\r\n                    <SOAP-ENV:Body>\r\n                    <SEND_SMS xmlns = \"http://tempuri.org/\"> \r\n                      <sdt>",
                phone,
                "</sdt>\r\n                      <noidung>",
                message,
                "</noidung> \r\n                    </SEND_SMS>\r\n                    </SOAP-ENV:Body>\r\n                </SOAP-ENV:Envelope>"
            });
            xmlDocument.LoadXml(xml);
            return xmlDocument;
        }

        static private void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream requestStream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(requestStream);
            }
        }
    }
}
