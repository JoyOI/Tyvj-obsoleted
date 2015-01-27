using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;

namespace Tyvj.ViewModels
{
    public class vProblem
    {
        public vProblem() { }
        public vProblem(Problem Problem)
        {
            var DbContext = new DB();
            ID = Problem.ID;
            Official = Problem.Official;
            Hide = Problem.Hide;
            Title = HttpUtility.HtmlEncode(Problem.Title);
            Accepted = Problem.AcceptedCount;
            Submitted = Problem.SubmitCount;
            UserID = Problem.UserID;
            Username = HttpUtility.HtmlEncode(Problem.User.Username);
            VIP = Problem.VIP;
            Series = Problem.Series;
        }
        public vProblem(Problem Problem, List<int> ac, List<int> submit)
            : this(Problem)
        {
            Flag = 0;
            if (Helpers.AcList.Existed(submit, Problem.ID))
                Flag = 1;
            if (Helpers.AcList.Existed(ac, Problem.ID))
                Flag = 2;
        }
        public int ID { get; set; }
        public bool Official { get; set; }
        public string Title { get; set; }
        public int Accepted { get; set; }
        public int Submitted { get; set; }
        public bool VIP { get; set; }
        public bool Hide { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Series { get; set; }
        public int Ratio
        {
            get 
            {
                var ret = 0;
                try
                {
                    ret = Accepted * 100 / Submitted;
                }
                catch { }
                return ret;
            }
        }
        public int Flag { get; set; }//0-null 1-ac 2-noac
    }
}