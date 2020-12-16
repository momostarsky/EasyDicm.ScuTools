using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using easyrsa;
using Newtonsoft.Json;

namespace easyscu
{
    public class KeyGen : ScuProc<RsaOptions>
    {
         class JsonContent : StringContent
        {
            public JsonContent(object obj) :
                base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
            {
            }
        }

        public KeyGen(RsaOptions option) : base(option)
        {
        }

        private static string HttpClientPost(string url, object datajson)
        {
            HttpClient httpClient = new HttpClient(); //http对象
            //表头参数
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //转为链接需要的格式
            HttpContent httpContent = new JsonContent(datajson);
            //请求
            HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;

            if (response.IsSuccessStatusCode)
            {
                Task<string> t = response.Content.ReadAsStringAsync();
                t.Wait(); 
                return t.Result;
            }
            else
            {
                return "";
            }
        }

        public override Task Start()
        {
            var t = Task.Factory.StartNew(() =>
            {
               
                try
                {
                    var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                    string json = File.ReadAllText(filePath);
                    dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    string url = jsonObj["RegistUrl"];
                    object regObj = new
                    {
                        clientId = Opt.AppId,
                        clientName = Opt.AppName,
                        days = Opt.Days,
                        // pubkey = km.PublicKey
                    };

                    var res = HttpClientPost(url, regObj);
                    // "RsaKey": {
                    //     "MyKey": "", 
                    // }

                    if (String.IsNullOrEmpty(res))
                        return;
                    dynamic regIfno = Newtonsoft.Json.JsonConvert.DeserializeObject(res);
                    // jsonObj["RsaKey"]["PublicKey"] = km.PublicKey;
                    // jsonObj["RsaKey"]["PrivateKey"] = km.PrivateKey;
                    jsonObj["RsaKey"]["ApplicationID"] = regIfno["appId"];
                    jsonObj["RsaKey"]["ApplicationKey"] = regIfno["appSeckey"];
                    jsonObj["RsaKey"]["AppID"] = Opt.AppId;
                    jsonObj["RsaKey"]["AppName"] = Opt.AppName;
                    jsonObj["RsaKey"]["KeySize"] = Opt.KeySize;

                    string output =
                        Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(filePath, output);
                }
                catch (ConfigurationErrorsException)
                {
                    Console.WriteLine("Error writing app settings");
                }
            });
            return t;
        }
    }
}