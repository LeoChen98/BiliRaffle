using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;

namespace BiliRaffle
{
    class Raffle
    {
        /// <summary>
        /// 开始抽奖
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="num">中奖人数</param>
        /// <param name="OneChance">只有一次机会</param>
        public static void Start( string url,int num = 1, bool OneChance = false)
        {
            url = url.Split('?')[0];
            string[] tmp = url.Split('/');
            switch (tmp[2])
            {
                case "t.bilibili.com":
                    ViewModel.Main.PushMsg("抽奖地址：" + url + "\r\n抽奖类型：动态转发抽奖\r\n中奖人数：" + num + "\r\n不统计重复：" + OneChance.ToString());
                    T_Raffle(tmp[tmp.Length-1], num);
                    break;

                case "h.bilibili.com":
                    
                    break;

                case "www.bilibili.com":
                    
                    break;

                default:
                    break;
            }
        }

        static Task FansTask;
        
        /// <summary>
        /// 开始抽奖（异步）
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="num">中奖人数</param>
        /// <param name="OneChance">只有一次机会</param>
        public static async void StartAsync(string url, int num = 1, bool OneChance = false)
        {
            ViewModel.Main.PushMsg("---------抽奖开始---------");
            ViewModel.Main.PushMsg("获取粉丝列表...");
            //FansTask = GetFansAsync();

            url = url.Split('?')[0];
            string[] tmp = url.Split('/');
            switch (tmp[2])
            {
                case "t.bilibili.com":
                    ViewModel.Main.PushMsg("---------抽奖设置---------");
                    ViewModel.Main.PushMsg("抽奖地址：" + url + "\r\n抽奖类型：动态转发抽奖\r\n中奖人数：" + num + "\r\n不统计重复："+OneChance.ToString());
                    ViewModel.Main.PushMsg("---------抽奖信息---------");

                    int[] rs = await T_RaffleAsync(tmp[3],num,OneChance);

                    ViewModel.Main.PushMsg("---------中奖名单---------");
                    foreach (int i in rs)
                    {
                        ViewModel.Main.PushMsg(GetUName(i) + "(uid:" + i + ")");
                    }
                    ViewModel.Main.PushMsg("---------抽奖结束---------");
                    break;

                case "h.bilibili.com":

                    break;

                case "www.bilibili.com":

                    break;

                default:
                    break;
            }
        }

        private static Task GetFansAsync()
        {
            return Task.Run(() =>
            {
                Fans_Data data = new Fans_Data();
                List<int> uids = new List<int>();
                int pn = 1;
                while (pn * 50 < data.total)
                {
                    string str = Http.GetBody("https://api.bilibili.com/x/relation/followers?vmid=" + hid + "&pn=" + pn + "&ps=50&order=desc", GetCookies(cookies));
                    if (!string.IsNullOrEmpty(str))
                    {
                        JObject obj = JObject.Parse(str);
                        if ((int)obj["code"] == 0)
                        {
                            data = JsonConvert.DeserializeObject<Fans_Data>(obj["data"].ToString());
                            foreach (Fans_Data.Fans u in data.list)
                            {
                                uids.Add(u.uid);
                            }
                        }
                    }

                    pn++;
                }

                Fans = uids.ToArray();
                ViewModel.Main.PushMsg("共获取到" + data.total + "个粉丝uid。");
            });
        }

        private static void GetFans()
        {
            Fans_Data data = new Fans_Data();
            List<int> uids = new List<int>();
            int pn = 1;
            while (pn * 50 < data.total)
            {
                string str = Http.GetBody("https://api.bilibili.com/x/relation/followers?vmid=" + hid + "&pn=" + pn + "&ps=50&order=desc", GetCookies(cookies));
                if (!string.IsNullOrEmpty(str))
                {
                    JObject obj = JObject.Parse(str);
                    if ((int)obj["code"] == 0)
                    {
                        data = JsonConvert.DeserializeObject<Fans_Data>(obj["data"].ToString());
                        foreach (Fans_Data.Fans u in data.list)
                        {
                            uids.Add(u.uid);
                        }
                    }
                }

                pn++;
                Thread.Sleep(1000);
            }

            Fans = uids.ToArray();
            ViewModel.Main.PushMsg("共获取到" + data.total + "个粉丝uid。");
        }

        /// <summary>
        /// 获取cookies实例
        /// </summary>
        /// <param name="cookies">cookies文本</param>
        /// <returns>cookies实例</returns>
        private static CookieCollection GetCookies(string cookies)
        {
            try
            {
                CookieCollection public_cookie;
                Uri target = new Uri("https://api.bilibili.com/x/relation/followers");
                public_cookie = new CookieCollection();
                cookies = cookies.Replace(",", "%2C");//转义“，”
                string[] cookiestrs = Regex.Split(cookies, "; ");
                foreach (string i in cookiestrs)
                {
                    string[] cookie = Regex.Split(i, "=");
                    public_cookie.Add(new Cookie(cookie[0], cookie[1]) { Domain = target.Host });
                }
                return public_cookie;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private class Fans_Data
        {
            public int total = 51;
            public Fans[] list;

            public class Fans
            {
                public int uid;
            }
        }

        /// <summary>
        /// 动态抽奖
        /// </summary>
        /// <param name="id">动态id(c_id)</param>
        /// <param name="num">中奖人数</param>
        /// <param name="OneChance">只有一次机会</param>
        /// <returns>抽奖结果</returns>
        private static int[] T_Raffle(string id, int num,bool OneChance = false)
        {
            T_Repost_Data Data = new T_Repost_Data();
            List<int> uids = new List<int>();
            int[] rs = new int[num];
            int i = 0;
            while (Data.has_more)
            {
                string str = Http.GetBody("https://api.vc.bilibili.com/dynamic_repost/v1/dynamic_repost/view_repost?dynamic_id=" + id + "&offset=" + i * 20);
                if (!string.IsNullOrEmpty(str))
                {
                    JObject obj = JObject.Parse(str);
                    if((int)obj["code"] == 0)
                    {
                        Data = JsonConvert.DeserializeObject<T_Repost_Data>(obj["data"].ToString());

                        if (i == 0) ViewModel.Main.PushMsg("共有【" + Data.total_count + "】条转发。");

                        foreach (T_Repost_Data.comment comment in Data.comments)
                        {
                            if(!uids.Contains(comment.uid) || !OneChance) uids.Add(comment.uid);
                        }
                    }
                }
                i++;
            }

            ViewModel.Main.PushMsg("共统计到" + uids.Count + "个（次）uid");

            Random random = new Random((int)DateTime.Now.ToUniversalTime().Ticks);
            for(int n = 0; n < num; n++)
            {
            re:
                int uid = uids[random.Next(0, uids.Count - 1)];
                bool IRId = IsRaffleId(uid);
                bool Repeated = rs.Contains(uid);
                if (!IRId && !Repeated)
                {
                    rs[n] = uid;
                    ViewModel.Main.PushMsg("抽到【" + GetUName(uid) + "（uid:" + uid + "）】中奖，有效。");
                }
                else
                {
                    goto re;
                }
            }
            return rs;
        }
        /// <summary>
        /// 动态抽奖(异步)
        /// </summary>
        /// <param name="id">动态id(c_id)</param>
        /// <param name="num">中奖人数</param>
        /// <param name="OneChance">只有一次机会</param>
        /// <returns>抽奖结果</returns>
        private static Task<int[]> T_RaffleAsync(string id, int num, bool OneChance = false)
        {
            return Task.Run(() =>
            {
                T_Repost_Data Data = new T_Repost_Data();
                List<int> uids = new List<int>();
                int[] rs = new int[num];
                int i = 0;
                while (Data.has_more)
                {
                    string str = Http.GetBody("https://api.vc.bilibili.com/dynamic_repost/v1/dynamic_repost/view_repost?dynamic_id=" + id + "&offset=" + i * 20);
                    if (!string.IsNullOrEmpty(str))
                    {
                        JObject obj = JObject.Parse(str);
                        if ((int)obj["code"] == 0)
                        {
                            Data = JsonConvert.DeserializeObject<T_Repost_Data>(obj["data"].ToString());

                            if (i == 0) ViewModel.Main.PushMsg("共有" + Data.total_count + "条转发。");

                            if(Data.comments.Length != 0)
                            {
                                foreach (T_Repost_Data.comment comment in Data.comments)
                                {
                                    if (!uids.Contains(comment.uid) || !OneChance) uids.Add(comment.uid);
                                }
                            }
                        }
                    }
                    i++;
                }

                ViewModel.Main.PushMsg("共统计到" + uids.Count + "个（次）uid");

                //FansTask.Wait();

                Random random = new Random((int)DateTime.Now.ToUniversalTime().Ticks);
                for (int n = 0; n < num; n++)
                {
                re:
                    int uid = uids[random.Next(0, uids.Count - 1)];
                    //if (IsFollowing(uid) && !IsRaffleId(uid) && !rs.Contains(uid))
                    if (!IsRaffleId(uid) && !rs.Contains(uid))
                    {
                        rs[n] = uid;
                        ViewModel.Main.PushMsg("抽到【" + GetUName(uid) + "（uid:" + uid + "）】中奖，有效。");
                    }
                    else
                    {
                        goto re;
                    }
                }
                return rs;
            });
        }

        /// <summary>
        /// 判断是否为粉丝
        /// </summary>
        /// <param name="uid">uid</param>
        /// <returns>是否</returns>
        private static bool IsFollowing(int uid)
        {
            if (Fans.Contains(uid)) return true;
            else
            {
                ViewModel.Main.PushMsg("抽到【" + GetUName(uid) + "（uid:" + uid + "）】中奖，但未关注，结果无效。");
                return false;
            }
        }

        private static int[] Fans;

        /// <summary>
        /// 检查是否抽奖号
        /// </summary>
        /// <param name="uid">账号uid</param>
        /// <returns>是否</returns>
        private static bool IsRaffleId(int uid)
        {
            int raffle_count = 0;
            Regex reg = new Regex("抽奖");
            string str = Http.GetBody("https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/space_history?visitor_uid=0&host_uid=" + uid + "&offset_dynamic_id=0");
            if (!string.IsNullOrEmpty(str))
            {
                JObject obj = JObject.Parse(str);
                if((int)obj["code"] == 0)
                {
                    Dynamic_Data data = JsonConvert.DeserializeObject<Dynamic_Data>(obj["data"].ToString());
                    int check_count = data.cards.Length >= 10 ? 10 : data.cards.Length;
                    for (int i = 0; i < check_count; i++)
                    {
                        if (reg.IsMatch(data.cards[i].card))
                        {
                            raffle_count++;
                        }
                    }
                }
            }

            if (raffle_count > 5) {
                ViewModel.Main.PushMsg("抽到【" + GetUName(uid) + "（uid:" + uid + "）】中奖，但判定为抽奖号，结果无效。（指数：" + raffle_count + "/10）");
                return true;
            }
            else return false;

        }

        /// <summary>
        /// 通过Uid获取UName
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        private static string GetUName(int uid)
        {
            string str = Http.GetBody("https://api.bilibili.com/x/space/acc/info?mid=" + uid);
            if (!string.IsNullOrEmpty(str))
            {
                JObject obj = JObject.Parse(str);
                if((int)obj["code"] == 0)
                {
                    return obj["data"]["name"].ToString();
                }
            }
            return "";
        }

        /// <summary>
        /// 动态数据模板
        /// </summary>
        private class Dynamic_Data
        {
                public Card[] cards;

                public class Card
                {
                    public string card;
                }

        }

        /// <summary>
        /// 动态转发数据模板
        /// </summary>
        private class T_Repost_Data
        {
            public bool has_more = true;
            public comment[] comments;
            public int total_count;

            public class comment
            {
                public int uid;
            }
        }

        const int hid = 27234245;
        const string cookies = "buvid3=F90D9172-0C4C-45E7-B0F7-80ADFC53B139110266infoc; sid=k5d873ft; DedeUserID=27234245; DedeUserID__ckMd5=ad72f46c8576a206; SESSDATA=fcb0ac86%2C1565683207%2C84d4d071; bili_jct=1d467dcb42e99e61feceb14ce5b6cd1c; CURRENT_FNVAL=16; UM_distinctid=16bef8c929b5-032de28c0709d4-e343166-13c680-16bef8c92a087f; finger=b3372c5f; im_notify_type_27234245=0; rpdid=|(J~RYuJRu|Y0J'ulYJ|mY~)m; fts=1563179122; stardustvideo=1; LIVE_PLAYER_TYPE=1; _uuid=5C473D22-208E-ABFD-D8D3-35345A78CD7A95435infoc; LIVE_BUVID=6d833b10ee9f3d1342fb225b93c990a8; LIVE_BUVID__ckMd5=4b3a14596968d841; CURRENT_QUALITY=112; im_local_unread_27234245=0; bp_t_offset_27234245=282676751514399091; im_seqno_27234245=46027";
    }
}
