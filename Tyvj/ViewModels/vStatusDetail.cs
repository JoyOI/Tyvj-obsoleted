using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;

namespace Tyvj.ViewModels
{
    public class vStatusDetail
    {
        public vStatusDetail() { }
        public vStatusDetail(JudgeTask judgetask, int index) 
        {
            ID = index;
            MemoryUsage = judgetask.MemoryUsage;
            TimeUsage = judgetask.TimeUsage;
            Result = judgetask.ResultAsInt;
            Hint = Helpers.HtmlFilter.Instance.SanitizeHtml(judgetask.Hint);
        }
        public int ID { get; set; }
        public int Result { get; set; }
        public int TimeUsage { get; set; }
        public int MemoryUsage { get; set; }
        public string Hint { get; set; }
    }
}