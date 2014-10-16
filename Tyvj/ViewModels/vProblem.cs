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
            Title = Problem.Title;
            Accepted = (from s in DbContext.Statuses
                        where s.ResultAsInt == (int)JudgeResult.Accepted
                        && s.ProblemID == ID
                        select s).Count();
            Submitted = (from s in DbContext.Statuses
                         where s.ProblemID == ID
                         select s).Count();
        }
        public vProblem(Problem Problem, string Username) : this(Problem)
        {
            var DbContext = new DB();
            if ((from s in DbContext.Statuses where s.User.Username == Username && s.ProblemID == Problem.ID select s).Count() == 0)
                Flag = 0;
            else if ((from s in DbContext.Statuses where s.User.Username == Username && s.ProblemID == Problem.ID && s.ResultAsInt == (int)JudgeResult.Accepted select s).Count() == 0)
                Flag = 1;
            else 
                Flag = 2;
        }
        public int ID { get; set; }
        public bool Official { get; set; }
        public string Title { get; set; }
        public int Accepted { get; set; }
        public int Submitted { get; set; }
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