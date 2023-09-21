using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;

namespace BestKeystoneAddonCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            headers.Add("X-Api-Token", "#######################################");
            var result = HttpsGet($@"http://localhost:50486/api/Lua/newest");

            var splitted = result.Split(new string[] { "#-#" }, StringSplitOptions.None);

            var Addondirectory = "Addon\\BestKeystone\\";
            var dbAddondirectory = "Addon\\BestKeystone\\db";
            if (!Directory.Exists(dbAddondirectory))
            {
                Directory.Delete(dbAddondirectory, true);
                Directory.CreateDirectory(dbAddondirectory);
            }


            foreach (var split in splitted)
            {
                var name = split.Split(' ')[0];
                var filename = "db_" + name + ".lua";

                var dblocation = dbAddondirectory + "\\" + filename;

                var data =
                $@"local addonname, ns = ..." + Environment.NewLine +
                $@"local data = {split.Split('=')[1]}" + Environment.NewLine +
                //$@"if not ns.methodes then" + Environment.NewLine +
                //$@"    ns.methodes = {{}}" + Environment.NewLine +
                //$@"end" + Environment.NewLine +
                $@"ns.methodes.get_{name.Replace("runs", "run")} = function (index)" + Environment.NewLine +
                $@"    return data[index]" + Environment.NewLine +
                $@"end" + Environment.NewLine;

                if (name.Contains("level"))
                {
                    data += $@"ns.runs = #data";

                }

                File.WriteAllText(dblocation, data);
            }


            if (File.Exists("Bestkeystone.zip"))
            {
                Thread.Sleep(1000);
                File.Delete("Bestkeystone.zip");
            }

            ZipFile.CreateFromDirectory("Addon", "Bestkeystone.zip");
            var timestamp = DateTime.Now.ToString("HHmmddMMyyyy");
            /*
            var versions = GetVersions().OrderByDescending(x=> x.id);
            UploadMultipart($@"Bestkeystone.zip", "Bestkeystone.zip", @"{  displayName: ""v"+timestamp+@""",changelog: ""Database updated"", gameVersions: ["+versions.FirstOrDefault().id+@"], releaseType: ""beta""}", "https://wow.curseforge.com/api/projects/313543/upload-file",headers);
            */
        }
        static Dictionary<string, string> headers = new Dictionary<string, string>();

        private static List<VersionInfo> GetVersions()
        {
            var result = HttpsGet(@"https://wow.curseforge.com/api/game/versions", headers);

            return JsonConvert.DeserializeObject<List<VersionInfo>>(result);
        }

        public static string HttpsGet(string request, [Optional] Dictionary<string, string> headers)
        {
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                HttpResponseMessage response = client.GetAsync(request).Result;
                response.EnsureSuccessStatusCode();
                string result = response.Content.ReadAsStringAsync().Result;
                return result;
            };
        }
        public static void UploadMultipart(string filelocation, string filename, string content, string url, [Optional] Dictionary<string, string> headers)
        {
            byte[] file = File.ReadAllBytes(filelocation);
            var client = new HttpClient();
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            var requestContent = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(file);
            requestContent.Add(fileContent, "file", filelocation);
            StringContent jsonContent = new StringContent(content);
            requestContent.Add(jsonContent, "metadata");
            var result = client.PostAsync(url, requestContent).Result;

        }

        public class VersionInfo
        {
            public int id { get; set; }
            public int gameVersionTypeID { get; set; }
            public string name { get; set; }
            public string slug { get; set; }
        }

    }

}
