﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using WeChatTools.Core;

namespace WeChatTools.Web.dev
{
    /// <summary>
    /// 微信域名自动切换--中转站
    /// </summary>
    public class HandlerSply3 : IHttpHandler
    {
       

        private string gotoRedirectUrl = "http://www.rrbay.com/";//最终用户访问的网址

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            string getJump = QueryString("key");//参数1：授权key
            string getUrl = QueryString("url");//参数1：用户传当前推广的网址,url需要编码 
            if (string.IsNullOrEmpty(getJump) || string.IsNullOrEmpty(getUrl))
            {

                context.Response.Redirect(gotoRedirectUrl);
            }
            else
            {

                getJump = getJump + "!" + getUrl;
              
                string domainLeft = "http://";

                string isHttps = ConfigTool.ReadVerifyConfig("IsHttps", "HostUrl");//这些域名都需要指向用户最终要访问的站点

                if (isHttps.ToLower() == "true")
                {
                    string agent = context.Request.UserAgent;
                    if (!agent.Contains("Macintosh") && !agent.Contains("iPhone") && !agent.Contains("iPod") && !agent.Contains("iPad") && !agent.Contains("Windows Phone") && !agent.Contains("Windows NT"))
                    {
                        domainLeft = "https://";
                    }
                }

                string domainCenter = GetRandHostUrl();
                gotoRedirectUrl = domainLeft + domainCenter + "/home/GoToHost";
                // string xxx =PostHtml(gotoRedirectUrl, getJump);

                string html = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "dev/sply.html");
                html = html.Replace("$actionUrl", gotoRedirectUrl).Replace("$jumpValue", getJump);


                context.Response.Write(html);
            }
            context.Response.End();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }





        public string PostHtml(string URL, string postData)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(postData);//Encoding.UTF8.GetBytes(postData);

            HttpWebRequest httpWebRequest;
            HttpWebResponse webResponse;
            Stream getStream;
            StreamReader streamReader;

            httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(URL);
            httpWebRequest.Accept = "application/json, text/javascript, */*; q=0.01";
            httpWebRequest.Referer = URL;
            // httpWebRequest.TransferEncoding = "gzip, deflate";

            httpWebRequest.ContentLength = data.Length;
            httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            //  httpWebRequest.Host = "183.6.175.51:8000";
            //  httpWebRequest.UserAgent = GetUserAgent();
            httpWebRequest.Method = "POST";
            // httpWebRequest.AllowAutoRedirect = true;
            Stream pReqStream = httpWebRequest.GetRequestStream();
            // Send the data.
            pReqStream.Write(data, 0, data.Length);
            pReqStream.Close();

            webResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            getStream = webResponse.GetResponseStream();
            streamReader = new StreamReader(getStream, Encoding.UTF8);
            string getString = "";
            getString = streamReader.ReadToEnd();

            streamReader.Close();
            getStream.Close();
            webResponse.Close();

            return getString;
        }


        private string GetRandHostUrl()
        {
            Random ran = new Random();

            string randUrl = "";
            /*
            bool isBlacklist = true;
            // int xx = 0;//没有做剔除操作，暂时限制循环次数。
            while (isBlacklist)
            {
                try
                {
                    */
            string hosturl = ConfigTool.ReadVerifyConfig("HostHttp", "HostUrl");//这些域名都需要指向用户最终要访问的站点
            string[] sArray = hosturl.Split(',');

            int RandKey1 = ran.Next(0, sArray.Length);//随机选中域名
            randUrl = sArray[RandKey1];
            /*

                                wxCheckApi = ConfigTool.ReadVerifyConfig("wxCheckApi", "WeiXin"); ;
                                wxCheckApiKey = ConfigTool.ReadVerifyConfig("wxCheckApiKey", "WeiXin");

                                WebRequest wr = (HttpWebRequest)WebRequest.Create(wxCheckApi + "?key=" + wxCheckApiKey + "&url=http://" + randUrl);
                                var stream = wr.GetResponse().GetResponseStream();
                                var sr = new StreamReader(stream, Encoding.GetEncoding("UTF-8"));
                                var all = sr.ReadToEnd();
                                sr.Close();
                                stream.Close();
                                //读取网站的数据
                                if (all.Contains("屏蔽"))
                                {
                                    //剔除域名
                                    if (hosturl.Contains(sArray[RandKey1] + ","))
                                    {
                                        hosturl = hosturl.Replace(sArray[RandKey1] + ",", "");
                                    }
                                    if (hosturl.Contains("," + sArray[RandKey1]))
                                    {
                                        hosturl = hosturl.Replace("," + sArray[RandKey1], "");
                                    }
                                    ConfigTool.WriteVerifyConfig("Host", hosturl, "HostUrl");//剔除黑名单域名
                                }
                                else
                                {
                                    isBlacklist = false;
                                    return randUrl;
                                }




                            }
                            catch (Exception ex)
                            {
                                randUrl = "";
                                //randUrl = ex.Message.ToString();
                                return randUrl;
                            }
                        }
            */
            return randUrl;
        }


        private static char[] constant =   
         {   
            '0','1','2','3','4','5','6','7','8','9',  
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'   
         };

        private static string GenerateRandomNumber(int Length)
        {
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(36);
            Random rd = new Random();
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(36)]);
            }
            return newRandom.ToString();
        }

        /// <summary>
        /// url请求里的参数
        /// 过滤非法字符
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string QueryString(string param)
        {
            if (HttpContext.Current.Request[param] == null || HttpContext.Current.Request[param].ToString().Trim() == "")
            {
                return "";
            }
            string ret = HttpContext.Current.Request[param].ToString().Trim();
            ret = ret.Replace(",", "");
            ret = ret.Replace("'", "");
            ret = ret.Replace(";", "");

            return ret;
        }

    }
}