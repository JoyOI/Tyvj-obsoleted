using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;
using Tyvj.ViewModels;

namespace Tyvj.Helpers
{
    public static class ColorName
    {
        public static string GetColor(UserRole Role)
        {
            if (Role >= UserRole.Master)
                return "#000";
            else if (Role == UserRole.VIP)
                return "red";
            return "inherit";
        }
        public static string GetLevel(int Ratings)
        {
            if (Ratings < 1500) return "R";
            if (Ratings < 1700) return "L3";
            if (Ratings < 2000) return "L2";
            if (Ratings < 2400) return "L1";
            return "S";
        }

        public static string GetNicknameHtml(string Nickname, UserRole UserRole)
        {
            return string.Format("<span style='color:{0}'>{1}</span>", GetColor(UserRole), HttpUtility.HtmlEncode(Nickname));
        }

        public static string GetNicknameHtml(string Nickname, UserRole UserRole, string @class)
        {
            return string.Format("<span style='color:{0}' class='{1}'>{2}</span>", GetColor(UserRole), @class, HttpUtility.HtmlEncode(Nickname));
        }
    }
}