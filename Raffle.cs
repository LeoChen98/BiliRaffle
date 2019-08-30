using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;

namespace BiliRaffle
{
    class Raffle
    {
        private static string _Cookies;
        public static string Cookies {
            get
            {
                if (string.IsNullOrEmpty(_Cookies))
                {
                re:
                    if ((bool)App.Current.Dispatcher.Invoke(() => {
                        return (new LoginWindow()).ShowDialog();
                    }))
                        return _Cookies;
                    else
                        goto re;
                }
                else
                {
                    return _Cookies;
                }
            }
            set
            {
                _Cookies = value;
            }
        }

        /// <summary>
        /// 开始抽奖
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="num">中奖人数</param>
        /// <param name="OneChance">只有一次机会</param>
        public static void Start( string url,int num = 1, bool OneChance = false,bool CheckFollow = false)
        {
            ViewModel.Main.PushMsg("---------抽奖开始---------");

            url = url.Split('?')[0];
            string[] tmp = url.Split('/');
            switch (tmp[2])
            {
                case "t.bilibili.com":
                    ViewModel.Main.PushMsg("---------抽奖设置---------");
                    ViewModel.Main.PushMsg("抽奖地址：" + url + "\r\n抽奖类型：动态转发抽奖\r\n中奖人数：" + num + "\r\n不统计重复：" + OneChance.ToString());
                    ViewModel.Main.PushMsg("---------抽奖信息---------");

                    int[] rs = T_Raffle(tmp[3], num, OneChance);

                    ViewModel.Main.PushMsg("---------中奖名单---------");
                    foreach (int i in rs)
                    {
                        ViewModel.Main.PushMsg(GetUName(i) + "(uid:" + i + ")");
                    }
                    ViewModel.Main.PushMsg("---------抽奖结束---------");
                    break;

                case "h.bilibili.com":
                    ViewModel.Main.PushMsg("提示：目前只支持动态详情页（域名为t.bilibili.com）的抽奖哦~");
                    break;

                case "www.bilibili.com":
                    ViewModel.Main.PushMsg("提示：目前只支持动态详情页（域名为t.bilibili.com）的抽奖哦~");
                    break;

                default:
                    ViewModel.Main.PushMsg("提示：目前只支持动态详情页（域名为t.bilibili.com）的抽奖哦~");
                    break;
            }
        }
        
        /// <summary>
        /// 开始抽奖（异步）
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="num">中奖人数</param>
        /// <param name="OneChance">只有一次机会</param>
        public static async void StartAsync(string url, int num = 1, bool OneChance = false, bool CheckFollow = false)
        {
            ViewModel.Main.PushMsg("---------抽奖开始---------");

            url = url.Split('?')[0];
            string[] tmp = url.Split('/');
            switch (tmp[2])
            {
                case "t.bilibili.com":
                    ViewModel.Main.PushMsg("---------抽奖设置---------");
                    ViewModel.Main.PushMsg("抽奖地址：" + url + "\r\n抽奖类型：动态转发抽奖\r\n中奖人数：" + num + "\r\n不统计重复："+OneChance.ToString());
                    ViewModel.Main.PushMsg("---------抽奖信息---------");

                    int[] rs = await T_RaffleAsync(tmp[3],num,OneChance,CheckFollow);

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

        /// <summary>
        /// 动态抽奖
        /// </summary>
        /// <param name="id">动态id(c_id)</param>
        /// <param name="num">中奖人数</param>
        /// <param name="OneChance">只有一次机会</param>
        /// <returns>抽奖结果</returns>
        private static int[] T_Raffle(string id, int num,bool OneChance = false, bool CheckFollow = false)
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

                        if (Data.comments.Length != 0)
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

            Random random = new Random((int)DateTime.Now.ToUniversalTime().Ticks);
            for (int n = 0; n < num; n++)
            {
            re:
                int uid = uids[random.Next(0, uids.Count - 1)];
                if (!IsRaffleId(uid) && !rs.Contains(uid))
                {
                    if (CheckFollow)
                    {
                        if (IsFollowing(uid))
                        {
                            rs[n] = uid;
                            ViewModel.Main.PushMsg("抽到【" + GetUName(uid) + "（uid:" + uid + "）】中奖，有效。");
                        }
                        else
                        {
                            goto re;
                        }
                    }
                    else
                    {
                        rs[n] = uid;
                        ViewModel.Main.PushMsg("抽到【" + GetUName(uid) + "（uid:" + uid + "）】中奖，有效。");
                    }
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
        private static Task<int[]> T_RaffleAsync(string id, int num, bool OneChance = false, bool CheckFollow = false)
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

                Random random = new Random((int)DateTime.Now.ToUniversalTime().Ticks);
                for (int n = 0; n < num; n++)
                {
                re:
                    int uid = uids[random.Next(0, uids.Count - 1)];
                    if (!IsRaffleId(uid) && !rs.Contains(uid))
                    {
                        if (CheckFollow)
                        {
                            if (IsFollowing(uid))
                            {
                                rs[n] = uid;
                                ViewModel.Main.PushMsg("抽到【" + GetUName(uid) + "（uid:" + uid + "）】中奖，有效。");
                            }
                            else
                            {
                                goto re;
                            }
                        }
                        else
                        {
                            rs[n] = uid;
                            ViewModel.Main.PushMsg("抽到【" + GetUName(uid) + "（uid:" + uid + "）】中奖，有效。");
                        }
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
            if (!string.IsNullOrEmpty(Cookies))
            {
                string str = Http.GetBody("https://api.bilibili.com/x/space/acc/relation?mid=" + uid, GetCookies(Cookies));
                if (!string.IsNullOrEmpty(str))
                {
                    JObject obj = JObject.Parse(str);
                    if((int)obj["code"] == 0)
                    {
                        switch ((int)obj["data"]["be_relation"]["attribute"])
                        {
                            case 1://悄悄关注
                            case 2://关注
                            case 6://互关
                                return true;
                            default:
                                ViewModel.Main.PushMsg("抽到【" + GetUName(uid) + "（uid:" + uid + "）】中奖，但未关注，结果无效。(relation:" + obj["data"]["be_relation"]["attribute"].ToString() + ")");
                                return false;
                        }
                        
                    }
                }
            }
            ViewModel.Main.PushMsg("抽到【" + GetUName(uid) + "（uid:" + uid + "）】中奖，但未关注，结果无效。");
            return false;
        }

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
    }
}
