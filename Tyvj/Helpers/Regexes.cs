using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using Tyvj.DataModels;
using Tyvj.ViewModels;

namespace Tyvj.Helpers
{
    public static class Regexes
    {
        public static Regex Email = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
    }
}