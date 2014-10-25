using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tyvj.Helpers
{
    public static class AcList
    {
        public static List<int> GetList(string src)
        {
            try
            {
                var tmp = src.Trim('|').Split('|').ToList();
                var ret = new List<int>();
                foreach (var str in tmp)
                    ret.Add(Convert.ToInt32(str));
                return ret;
            }
            catch 
            {
                return new List<int>();
            }
        }
        public static bool Existed(List<int> src,int ProblemID)
        {
            return src.Where(x => x == ProblemID).Count() > 0;
        }
        public static string ToString(List<int> src)
        {
            var ret = "";
            foreach (var id in src.Distinct())
                ret += id + "|";
            return ret.Trim('|');
        }
    }
}