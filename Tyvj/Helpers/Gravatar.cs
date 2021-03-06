﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using Tyvj.DataModels;
using Tyvj.ViewModels;
using System.Net;

namespace Tyvj.Helpers
{
    public static class Gravatar
    {
        public static void RefreshGravatar(int UserID)
        {
            try
            {
                using (var db = new DB())
                {
                    var user = db.Users.Find(UserID);
                    if (user.Gravatar != null)
                    {
                        if (DateTime.Now >= user.LastLoginTime.AddDays(3) || user.Avatar.Count() == 0)
                        {
                            var wc = new WebClient();
                            user.Avatar = wc.DownloadData(GetAvatarURL(user.Gravatar, 180));
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch { }
        }

        public static string GetAvatarURL(string email, int size)
        {
            try
            {
                string md5 = ToHexString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(email)));
                string url = string.Format("http://gravatar.duoshuo.com/avatar/{0}?s={1}&d=mm", md5, size);
                return url;
            }
            catch { return "https://avatars3.githubusercontent.com/u/28256913?v=3&s=200"; }
        }

        static string ToHexString(byte[] bytes)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    string tmp = Convert.ToString(b, 16);
                    if (tmp.Length == 1) sb.Append('0').Append(tmp);
                    else sb.Append(tmp);
                }
                return sb.ToString();
            } catch { return string.Empty; }
        }
    }
}