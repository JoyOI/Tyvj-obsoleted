using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using Tyvj.DataModels;
using Tyvj.ViewModels;

namespace Tyvj.Helpers
{
    public static class Gravatar
    {
        public static string GetAvatarURL(string email, int size)
        {
            string md5 = ToHexString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(email)));
            string url = string.Format("http://www.gravatar.com/avatar/{0}?s={1}&d=mm", md5, size);
            return url;
        }

        static string ToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                string tmp = Convert.ToString(b, 16);
                if (tmp.Length == 1) sb.Append('0').Append(tmp);
                else sb.Append(tmp);
            }
            return sb.ToString();
        }
    }
}