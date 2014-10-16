using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;

namespace Tyvj.ViewModels
{
    public class vContest
    {
        public vContest() { }
        public vContest(Contest Contest) 
        {
            DB DbContext = new DB();
            ID = Contest.ID;
            Format = Contest.Format.ToString();
            Join = (from s in DbContext.Statuses
                    where s.ContestID == Contest.ID
                    select s.UserID).Distinct().ToList().Count;
            Title = Contest.Title;
            Begin = Contest.Begin.ToString("yyyy-MM-dd HH:mm");
            Duration = Helpers.Time.ToTimeLength(Contest.Begin, Contest.End);
            if (DateTime.Now < Contest.Begin) StatusAsInt = 0;
            else if (DateTime.Now < Contest.End) StatusAsInt = 1;
            else StatusAsInt = 2;
        }
        public int ID { get; set; }
        public string Format { get; set; }
        public int Join { get; set; }
        public string Title { get; set; }
        public string Begin { get; set; }
        public string Duration { get; set; }
        public string Status 
        { 
            get 
            {
                switch (StatusAsInt)
                {
                    case 0: return "Ready";
                    case 1:return "Live";
                    case 2: return "Done";
                    default:return "";
                }
            } 
        }
        public int StatusAsInt { get; set; }
    }
}