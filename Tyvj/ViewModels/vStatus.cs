using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;

namespace Tyvj.ViewModels
{
    public class vStatus
    {
        public vStatus() { }
        public vStatus(Status Status)
        {
            ID = Status.ID;
            UserID = Status.UserID;
            IsContestStatus = Status.ContestID == null ? false : true;
            ProblemID = Status.ProblemID;
            ProblemTitle = Status.Problem.Title;
            ResultAsInt = Status.ResultAsInt;
            Result = Status.Result.ToString();
            try
            {
                TimeUsage = Status.JudgeTasks.Sum(x => x.TimeUsage);
                MemoryUsage = Status.JudgeTasks.Max(x => x.MemoryUsage); 
                Score = Status.JudgeTasks.Where(x => x.Result == JudgeResult.Accepted).Count() * 100 / Status.JudgeTasks.Count;
            }
            catch 
            {
                TimeUsage = 0;
                MemoryUsage = 0;
                Score = 0;
            }
            Username = Status.User.Username;
            Language = CommonEnums.LanguageDisplay[Status.LanguageAsInt];
            Time = Status.Time.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public int ID { get; set; }
        public int UserID { get; set; }
        public bool IsContestStatus { get; set; }
        public int ProblemID { get; set; }
        public string ProblemTitle { get; set; }
        public int ResultAsInt { get; set; }
        public string Result { get; set; }
        public int TimeUsage { get; set; }
        public int MemoryUsage { get; set; }
        public int Score { get;set;}
        public string Username { get; set; }
        public string Language { get; set; }
        public string Time { get; set; }
    }
}