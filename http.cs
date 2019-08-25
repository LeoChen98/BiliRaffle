using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BiliRaffle
{
    /// <summary>
    /// HTTP封装类
    /// </summary>
    internal class Http
    {
        #region Public Methods

        /// <summary>
        /// Get方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="cookie">cookies集合实例</param>
        /// <param name="referer">Referer</param>
        /// <param name="user_agent">User-agent</param>
        /// <param name="specialheaders">除前面之外的Headers</param>
        /// <returns>请求返回体</returns>
        public static string GetBody(string url, CookieCollection cookie = null,
            string referer = "", string user_agent = "", WebHeaderCollection specialheaders = null)
        {
            string result = "";
            HttpWebRequest req = null;
            HttpWebResponse rep = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);

                if (specialheaders != null) req.Headers = specialheaders;

                if (cookie != null)
                {
                    req.CookieContainer = new CookieContainer(cookie.Count)
                    {
                        PerDomainCapacity = cookie.Count
                    };
                    req.CookieContainer.Add(cookie);
                }

                if (!string.IsNullOrEmpty(referer)) req.Referer = referer;
                if (!string.IsNullOrEmpty(user_agent)) req.UserAgent = user_agent;

                rep = (HttpWebResponse)req.GetResponse();
                using (StreamReader reader = new StreamReader(rep.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }
            }
            finally
            {
                if (rep != null) rep.Close();
                if (req != null) req.Abort();
            }
            return result;
        }

        /// <summary>
        /// POST方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="data">要POST发送的文本</param>
        /// <param name="cookie">cookies集合实例</param>
        /// <param name="contenttype">数据类型</param>
        /// <param name="referer">Referer</param>
        /// <param name="user_agent">User-agent</param>
        /// <param name="specialheaders">除前面之外的Headers</param>
        /// <returns>请求返回体</returns>
        public static string PostBody(string url, string data = "", CookieCollection cookie = null,
            string contenttype = "application/x-www-form-urlencoded;charset=utf-8", string referer = "", string user_agent = "",
            WebHeaderCollection specialheaders = null)
        {
            string result = "";
            HttpWebRequest req = null;
            HttpWebResponse rep = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);

                if (specialheaders != null) req.Headers = specialheaders;

                req.Method = "POST";

                if (cookie != null)
                {
                    req.CookieContainer = new CookieContainer(cookie.Count)
                    {
                        PerDomainCapacity = cookie.Count
                    };
                    req.CookieContainer.Add(cookie);
                }

                req.ContentType = contenttype;

                byte[] bdata = Encoding.UTF8.GetBytes(data);
                Stream sdata = req.GetRequestStream();
                sdata.Write(bdata, 0, bdata.Length);
                sdata.Close();

                if (!string.IsNullOrEmpty(referer)) req.Referer = referer;
                if (!string.IsNullOrEmpty(user_agent)) req.UserAgent = user_agent;

                rep = (HttpWebResponse)req.GetResponse();
                using (StreamReader reader = new StreamReader(rep.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }
            }
            finally
            {
                if (rep != null) rep.Close();
                if (req != null) req.Abort();
            }
            return result;
        }

        /// <summary>
        /// OPTIONS方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="cookie">cookies集合实例</param>
        /// <param name="referer">Referer</param>
        /// <param name="user_agent">User-agent</param>
        /// <param name="specialheaders">除前面之外的Headers</param>
        /// <returns>请求返回体</returns>
        public static bool Options(string url, CookieCollection cookie = null,
            string referer = "", string user_agent = "", WebHeaderCollection specialheaders = null)
        {
            bool result = false;
            HttpWebRequest req = null;
            HttpWebResponse rep = null;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);

                if (specialheaders != null) req.Headers = specialheaders;

                req.Method = "OPTIONS";

                if (cookie != null)
                {
                    req.CookieContainer = new CookieContainer(cookie.Count)
                    {
                        PerDomainCapacity = cookie.Count
                    };
                    req.CookieContainer.Add(cookie);
                }

                if (!string.IsNullOrEmpty(referer)) req.Referer = referer;
                if (!string.IsNullOrEmpty(user_agent)) req.UserAgent = user_agent;

                rep = (HttpWebResponse)req.GetResponse();

                if (rep.StatusCode == HttpStatusCode.OK) result = true;
            }
            finally
            {
                if (rep != null) rep.Close();
                if (req != null) req.Abort();
            }
            return result;
        }

        ///// <summary>
        ///// Put文件方法
        ///// </summary>
        ///// <param name="url">URL</param>
        ///// <param name="filename">要PUT的文件路径</param>
        ///// <param name="start">开始字节位</param>
        ///// <param name="length">长度</param>
        ///// <param name="cookie">cookies集合实例</param>
        ///// <param name="referer">Referer</param>
        ///// <param name="user_agent">User-agent</param>
        ///// <param name="specialheaders">除前面之外的Headers</param>
        ///// <returns>请求返回体</returns>
        //public static string PutFile(string url, string filename, int start = 0, int length = -1, CookieCollection cookie = null,
        //    string referer = "", string user_agent = "", WebHeaderCollection specialheaders = null)
        //{
        //    string result = "";
        //    HttpWebRequest req = null;
        //    HttpWebResponse rep = null;
        //    try
        //    {
        //        req = (HttpWebRequest)WebRequest.Create(url);

        //        if (specialheaders != null) req.Headers = specialheaders;

        //        req.Method = "PUT";

        //        req.Timeout = Timeout.Infinite;

        //        FileInfo fi = new FileInfo(filename);

        //        if (length == -1) length = (int)fi.Length;
        //        if (start + length > fi.Length) length = (int)(fi.Length - start);

        //        byte[] bdata = new byte[length];

        //        using (FileStream fs = File.OpenRead(filename))
        //        {
        //            fs.Read(bdata, start, length);
        //        }

        //        using (Stream sdata = req.GetRequestStream())
        //        {
        //            sdata.Write(bdata, 0, bdata.Length);
        //        }

        //        if (cookie != null)
        //        {
        //            req.CookieContainer = new CookieContainer(cookie.Count)
        //            {
        //                PerDomainCapacity = cookie.Count
        //            };
        //            req.CookieContainer.Add(cookie);
        //        }

        //        if (!string.IsNullOrEmpty(referer)) req.Referer = referer;
        //        if (!string.IsNullOrEmpty(user_agent)) req.UserAgent = user_agent;

        //        rep = (HttpWebResponse)req.GetResponse();
        //        using (StreamReader reader = new StreamReader(rep.GetResponseStream()))
        //        {
        //            result = reader.ReadToEnd();
        //        }
        //    }
        //    finally
        //    {
        //        if (rep != null) rep.Close();
        //        if (req != null) req.Abort();
        //    }
        //    return result;
        //}

        /// <summary>
        /// Put文件方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="filename">要PUT的文件路径</param>
        /// <param name="start">开始字节位</param>
        /// <param name="length">长度</param>
        /// <param name="cookie">cookies集合实例</param>
        /// <param name="referer">Referer</param>
        /// <param name="user_agent">User-agent</param>
        /// <param name="specialheaders">除前面之外的Headers</param>
        /// <returns>请求返回体</returns>
        public static async Task<string> PutFile(string url, string filename, int start = 0, int length = -1, CookieCollection cookie = null,
            string referer = "", string user_agent = "", WebHeaderCollection specialheaders = null)
        {
            string result = "";
            HttpWebRequest req = null;
            HttpWebResponse rep = null;
            try
            {
                await Task.Run(() =>
                {
                    req = (HttpWebRequest)WebRequest.Create(url);

                    if (specialheaders != null) req.Headers = specialheaders;

                    req.Method = "PUT";

                    req.Timeout = Timeout.Infinite;

                    FileInfo fi = new FileInfo(filename);

                    if (length == -1) length = (int)fi.Length;
                    if (start + length > fi.Length) length = (int)(fi.Length - start);

                    byte[] bdata = new byte[length];

                    using (FileStream fs = File.OpenRead(filename))
                    {
                        fs.Position = start;
                        fs.Read(bdata, 0, length);
                    }

                    using (Stream sdata = req.GetRequestStream())
                    {
                        sdata.Write(bdata, 0, bdata.Length);
                    }

                    if (cookie != null)
                    {
                        req.CookieContainer = new CookieContainer(cookie.Count)
                        {
                            PerDomainCapacity = cookie.Count
                        };
                        req.CookieContainer.Add(cookie);
                    }

                    if (!string.IsNullOrEmpty(referer)) req.Referer = referer;
                    if (!string.IsNullOrEmpty(user_agent)) req.UserAgent = user_agent;

                    rep = (HttpWebResponse)req.GetResponse();
                    using (StreamReader reader = new StreamReader(rep.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                    }
                });
            }
            finally
            {
                if (rep != null) rep.Close();
                if (req != null) req.Abort();
            }
            return result;
        }
    }

    #endregion Public Methods
}