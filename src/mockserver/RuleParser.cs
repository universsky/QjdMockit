using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using MockServer.Model;

namespace MockServer
{
    internal class RuleParser
    {
        static object rulesFileLock = new object();
        static Dictionary<RuleKey, ExpectHttpResponseMap> _responseMap = new Dictionary<RuleKey, ExpectHttpResponseMap>();
        string _rulesFile = string.Empty;
        public RuleParser(string rulesFile)
        {
            ParseRules(rulesFile);
            _rulesFile = rulesFile;
            FileInfo f = new FileInfo(rulesFile);
            var wacher = new FileSystemWatcher(f.Directory.FullName, f.Name);
            wacher.Changed += new FileSystemEventHandler(wacher_Changed);
            wacher.EnableRaisingEvents = true;
        }

        void wacher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!String.IsNullOrEmpty(_rulesFile))
            {
                try
                {
                    ParseRules(_rulesFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        private ExpectHttpResponse GetStringResp(FileInfo responseFile)
        {
            int statusCode = -1;
            WebHeaderCollection header = new WebHeaderCollection();
            var body = string.Empty;
            // Try to get expected HTTP Response from response file
            using (StreamReader bodySr = new StreamReader(new FileStream(responseFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                // Parse HTTP Status Code
                var thisRow = bodySr.ReadLine();


                if (thisRow != null)
                {
                    var firstRow = thisRow.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (firstRow.Length > 1)
                    {
                        if (int.TryParse(firstRow[1], out statusCode))
                        {
                            // Continue read until reach end of file or meet first line break
                            while (!bodySr.EndOfStream)
                            {
                                var currentRow = bodySr.ReadLine();
                                if (currentRow == null || currentRow == String.Empty)
                                {
                                    break;
                                }
                                else
                                {
                                    // Parse HTTP Header
                                    var h = currentRow.Split(new char[] { ' ' });
                                    if (h.Length > 1)
                                    {
                                        var headerKey = h[0].Substring(0, h[0].Length - 1);
                                        string s = string.Empty;
                                        for (int i = 1; i < h.Length; i++)
                                        {
                                            s += h[i];
                                            s += ' ';
                                        }

                                        var headerValue = s.TrimStart().TrimEnd(); ;
                                        header.Add(headerKey, headerValue);
                                    }
                                }
                            }
                            // The content remained is considered as the HTTP body
                            body = bodySr.ReadToEnd();
                            return new ExpectHttpResponse()
                            {
                                Body = System.Text.Encoding.UTF8.GetBytes(body),
                                StatusCode = statusCode,
                                Header = header
                            };

                        }
                    }

                }
            }
            return null;
        }
        private ExpectHttpResponse GetBytesResp(FileInfo headerFile, FileInfo bodyFile)
        {
            int statusCode = -1;
            WebHeaderCollection header = new WebHeaderCollection();
            byte[] body = null;
            string errMsg = string.Empty;
            // Try to get HTTP header
            if (File.Exists(headerFile.FullName))
            {
                using (StreamReader hSr = new StreamReader(new FileStream(headerFile.FullName, FileMode.Open)))
                {
                    // Parse HTTP Status Code
                    var thisRow = hSr.ReadLine();

                    if (thisRow != null)
                    {
                        var firstRow = thisRow.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (firstRow.Length > 1)
                        {
                            if (int.TryParse(firstRow[1], out statusCode))
                            {
                                // Continue read until reach end of file or meet first line break
                                while (!hSr.EndOfStream)
                                {
                                    var currentRow = hSr.ReadLine();
                                    if (currentRow == null || currentRow == String.Empty)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        // Parse HTTP Header
                                        var h = currentRow.Split(new char[] { ' ' });
                                        if (h.Length > 1)
                                        {
                                            var headerKey = h[0].Substring(0, h[0].Length - 1);
                                            var headerValue = h[1].TrimStart();
                                            header.Add(headerKey, headerValue);
                                        }
                                    }
                                }

                            }
                        }

                    }

                }
            }
            else
            {
                errMsg += "Mapped Header File Does Not Exist!";
            }
            if (File.Exists(bodyFile.FullName))
            {
                body = File.ReadAllBytes(bodyFile.FullName);
            }
            else
            {
                errMsg += " Mapped Body File Does Not Exist!";
            }
            if (errMsg != string.Empty)
            {
                body = System.Text.Encoding.UTF8.GetBytes(errMsg);
                header = new WebHeaderCollection();
            }
            return new ExpectHttpResponse()
            {
                Body = body,
                StatusCode = statusCode,
                Header = header
            };
        }
        static string RESPONSE_DIR = AppDomain.CurrentDomain.BaseDirectory + @"mocked-data\";
        const string URL_MAP_TYPE_STARTWITH = "startwith";
        const string URL_MAP_TYPE_CONTAINS = "contains";
        const string CONVERT_TAG = "[convert]";
        const string BODY_CONTAINS_TAG = "[contains]";
        private static void ParseRules(string rulesFile)
        {
            _responseMap.Clear();
            //var rulesFile = AppDomain.CurrentDomain.BaseDirectory + "Rules.txt";
            if (File.Exists(rulesFile))
            {
                lock (rulesFileLock)
                {
                    using (StreamReader sr = new StreamReader(new FileStream(rulesFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        while (!sr.EndOfStream)
                        {
                            var rule = sr.ReadLine().Split(new char[] { '\t' });
                            if (rule.Length > 0 && !rule[0].TrimStart().StartsWith("#")) // # comment flag
                            {
                                #region URL AND MAP FILE
                                if (rule.Length > 1)
                                {

                                    ExpectHttpResponseMap ehr = null;
                                    // If it's single file mapping
                                    if (!rule[1].Contains(","))
                                    {
                                        var responseFile = RESPONSE_DIR + rule[1];
                                        if (File.Exists(responseFile))
                                        {
                                            ehr = new ExpectHttpResponseMap()
                                            {
                                                Type = MapFileType.One,
                                                FileUri = new FileInfo[] { new FileInfo(responseFile) }
                                            };
                                        }
                                        else
                                        {
                                            ehr = new ExpectHttpResponseMap()
                                            {
                                                Type = MapFileType.None,
                                                FileUri = null
                                            };
                                        }
                                    }
                                    // If it's two file mapping
                                    else
                                    {
                                        var temp = rule[1].Split(',');
                                        var hFile = RESPONSE_DIR + temp[0];
                                        var bFile = RESPONSE_DIR + temp[1];
                                        ehr = new ExpectHttpResponseMap()
                                        {
                                            Type = MapFileType.Two,
                                            FileUri = new FileInfo[] { new FileInfo(hFile), new FileInfo(bFile) }
                                        };
                                    }
                                    // Get map url and map url type (exact or startwith)
                                    var mockUrl = rule[0].TrimStart();
                                    var mapBodySubStr = string.Empty;

                                    URLMapType urlMapType = URLMapType.Exact;
                                    // Parse verb
                                    string verb = string.Empty;
                                    if (rule.Length > 2 && IsVerb(rule[2]))
                                    {
                                        verb = rule[2];
                                    }
                                    // Parse delay
                                    TimeSpan delay = TimeSpan.FromSeconds(0);
                                    if ((rule.Length - 1) >= 1 && GetDelay(rule[rule.Length - 1], out delay))
                                    {
                                        ehr.AdditionalDelay = delay;
                                    }

                                    if (mockUrl.StartsWith("["))
                                    {
                                        // Check map type
                                        var strUrlMapType = mockUrl.Substring(1, mockUrl.IndexOf("]") - 1);
                                        if (strUrlMapType.ToLower() == URL_MAP_TYPE_STARTWITH)
                                        {
                                            urlMapType = URLMapType.StartWith;
                                            mockUrl = mockUrl.Substring(mockUrl.IndexOf("]") + 1);
                                        }
                                        else if (strUrlMapType.ToLower() == URL_MAP_TYPE_CONTAINS)
                                        {
                                            urlMapType = URLMapType.Contains;
                                            mockUrl = mockUrl.Substring(mockUrl.IndexOf("]") + 1);
                                        }


                                        var splitTag = BODY_CONTAINS_TAG;
                                        var allStr = mockUrl;
                                        //If there is a body contains rule
                                        if (allStr.Contains(splitTag))
                                        {
                                            var index = allStr.IndexOf(splitTag);
                                            mockUrl = allStr.Substring(0, index > 1 ? index : 1);
                                            mapBodySubStr = allStr.Substring(index + splitTag.Length);
                                            //Parse rule and set enableConverter's value for ehr

                                        }
                                        // check if should enable converter
                                        if (ShouldEnableConverter(rule))
                                        {
                                            ehr.EnableConverter = true;
                                        }
                                        //  mockUrl = mockUrl.Substring(mockUrl.IndexOf("]") + 1);
                                    }
                                    // if (Uri.TryCreate(mockUrl, UriKind.Absolute, out url))
                                    {
                                        _responseMap.Add(new RuleKey()
                                        {
                                            MapBodySubStr = mapBodySubStr,
                                            MapUri = mockUrl,
                                            MapType = urlMapType,
                                            Verb = verb
                                        },
                                                           ehr);

                                    }


                                }
                                #endregion
                            }
                        }
                    }

                }
            }
        }
        private static bool ShouldEnableConverter(string[] rule)
        {
            bool result = false;
            // This tag may only appear at the last two slots
            for (int i = rule.Length - 1; i >= 2; i--)
            {
                if (rule[i].Trim().ToLower() == CONVERT_TAG)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        private static bool IsVerb(string input)
        {
            var i = input.ToLower();
            return i == "get" || i == "post";
        }
        private static bool GetDelay(string input, out TimeSpan delay)
        {
            TimeSpan R = TimeSpan.FromSeconds(0);
            var result = false;
            if (input.Length < 2) { delay = R; return result; }
            if (input.StartsWith("+"))
            {
                if (input.ToLower().EndsWith("ms"))
                {
                    var intRangeStr = input.Substring(1, input.ToLower().LastIndexOf("ms") - 1);
                    int delayMS = 0;
                    var r = int.TryParse(intRangeStr, out delayMS);
                    if (r)
                    {
                        R = TimeSpan.FromMilliseconds(delayMS);
                        result = true;
                    }
                }
                else if (input.ToLower().EndsWith("s"))
                {
                    var intRangeStr = input.Substring(1, input.ToLower().LastIndexOf("s") - 1);
                    int delayS = 0;
                    var r = int.TryParse(intRangeStr, out delayS);
                    if (r)
                    {
                        R = TimeSpan.FromSeconds(delayS);
                        result = true;
                    }
                }
            }
            delay = R;
            return result;
        }
        private ExpectHttpResponse GetExpectHttpResponseFinal(ExpectHttpResponseMap map)
        {
            ExpectHttpResponse finalResp = null;
            if (map.Type == MapFileType.One)
            {
                finalResp = GetStringResp(map.FileUri[0]);
            }
            else if (map.Type == MapFileType.Two)
            {
                finalResp = GetBytesResp(map.FileUri[0], map.FileUri[1]);
            }
            else if (map.Type == MapFileType.None)
            {
                finalResp = new ExpectHttpResponse();

            }
            finalResp.AdditionalDelay = map.AdditionalDelay;
            return finalResp;

        }
        private ExpectHttpResponseMap GetExpectHttpResponseMap(Uri input, string method, string reqBody, bool needCheckBody)
        {

            foreach (RuleKey key in _responseMap.Keys)
            {
                var needCheckVerb = false;
                if (key.Verb != string.Empty) { needCheckVerb = true; }
                // If Verb is specified in the rules file, check the verb
                // If does not match, skip further check
                if (needCheckVerb && key.Verb.ToLower() != method.ToLower())
                {
                    continue;
                }
                if (key.MapType == URLMapType.Exact)
                {
                    if (input.AbsoluteUri == key.MapUri)
                    {
                        if (needCheckBody)
                        {
                            if (reqBody.Contains(key.MapBodySubStr))
                            {
                                return _responseMap[key];
                            }
                        }
                        else return _responseMap[key];
                    }
                }
                else if (key.MapType == URLMapType.StartWith)
                {
                    if (input.AbsoluteUri.StartsWith(key.MapUri))
                    {
                        if (needCheckBody)
                        {
                            if (reqBody.Contains(key.MapBodySubStr))
                            {
                                return _responseMap[key];
                            }
                        }
                        else return _responseMap[key];
                    }
                }
                else if (key.MapType == URLMapType.Contains)
                {
                    if (input.AbsoluteUri.Contains(key.MapUri))
                    {
                        if (needCheckBody)
                        {
                            if (reqBody.Contains(key.MapBodySubStr))
                            {
                                return _responseMap[key];
                            }
                        }
                        else return _responseMap[key];
                    }
                }


            }

            return null;
        }
        internal ExpectHttpResponse GetExpectHttpResponse(Uri input, string method, string reqBody)
        {
            ExpectHttpResponseMap finalRespMap = GetExpectHttpResponseMap(input, method, reqBody, true);
            if (finalRespMap != null)
            {
                return GetExpectHttpResponseFinal(finalRespMap);
            }
            else
            {
                return null;
            }
        }
        internal bool NeedCallConvert(Uri input, string method, string reqBody)
        {
            var result = false;
            ExpectHttpResponseMap map = GetExpectHttpResponseMap(input, method, reqBody, false);
            if (map != null && map.EnableConverter)
            {
                result = true;
            }
            return result;

        }
    }


}
