using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;

namespace Tyvj.ViewModels
{
    public class vStanding
    {
        public vStanding() { }
        public vStanding(User user, Contest contest)
        {
            UserID = user.ID;
            Details = new List<vStandingCol>();
            var problems = contest.ContestProblems.OrderBy(x => x.Point);
            foreach (var problem in problems)
            {
                Details.Add(new vStandingCol(user, problem.Problem, contest, problem.Point));
            }
            Key1 = Details.Sum(x => x.Key1);
            Key2 = Details.Sum(x => x.Key2);
            Key3 = Details.Sum(x => x.Key3);
            Gravatar = "/Avatar/" + user.ID;
            Nickname = Helpers.ColorName.GetNicknameHtml(user.Username, user.Ratings.Sum(x => x.Credit) + 1500);
            UserID = user.ID;
            Display1 = Key1.ToString();
            switch (contest.Format)
            { 
                case ContestFormat.OI:
                    Display2 = Key2 + " ms";
                    break;
                //case ContestFormat.OPJOI:
                //    Display2 = Key2.ToString();
                //    break;
                case ContestFormat.ACM:
                    Display2 = new TimeSpan(0, 0, Key2).ToString("c");
                    break;
                case ContestFormat.Codeforces:
                //case ContestFormat.TopCoder:
                    if (Key2 == 0 && Key3 == 0)
                        Display2 = "";
                    else if (Key2 != 0 && Key3 == 0)
                        Display2 = "+" + Key2;
                    else if (Key2 == 0 && Key3 != 0)
                        Display2 = "-" + Key3;
                    else
                        Display2 = "+" + Key2 + " : -" + Key3;
                    Key1 += Key2 * 100;
                    Key1 -= Key3 * 50;
                    Display1 = Key1.ToString();
                    break;
            }
        }
        public int Key1 { get; set; }
        public int Key2 { get; set; }
        public int Key3 { get; set; }
        public string Nickname { get; set; }
        public string Gravatar { get; set; }
        public List<vStandingCol> Details { get; set; }
        public int UserID { get; set; }
        public string Display1 { get; set; }
        public string Display2 { get; set; }
    }
    public class vStandingCol
    {
        public vStandingCol() { }
        public vStandingCol(User user, Problem problem, Contest contest, int credit) 
        {
            var statuses = Helpers.Contest.GetStatuses(problem.ID, contest.ID).Where(x=>x.UserID == user.ID).ToList();
            #region OI赛制逻辑
            if (contest.Format == ContestFormat.OI)
            {
                var status = statuses.LastOrDefault();
                if (status == null)
                {
                    StatusID = null;
                    Css = "";
                    Display = "";
                }
                else
                {
                    StatusID = status.ID;
                    int score;
                    try { score = status.JudgeTasks.Where(x => x.ResultAsInt == (int)JudgeResult.Accepted).Count() * 100 / status.JudgeTasks.Count; }
                    catch { score = 0; }
                    Display = score.ToString();
                    if (score == 0)
                        Css = "rank-red";
                    else if (score != 100)
                        Css = "rank-orange";
                    else
                        Css = "rank-green";
                    Key1 = score;
                    Key2 = status.JudgeTasks.Where(x=>x.Result == JudgeResult.Accepted).ToList().Sum(x => x.TimeUsage);
                    Key3 = status.JudgeTasks.Where(x => x.Result == JudgeResult.Accepted).ToList().Sum(x => x.MemoryUsage);
                }
            }
            #endregion
            #region OPJOI赛制逻辑
            //else if (contest.Format == Entity.ContestFormat.OPJOI)
            //{
            //    var status = statuses.LastOrDefault();
            //    if (status == null)
            //    {
            //        StatusID = null;
            //        Css = "";
            //        Display = "";
            //    }
            //    else
            //    {
            //        StatusID = status.ID;
            //        var score = status.JudgeTasks.Where(x => x.ResultAsInt == (int)Entity.JudgeResult.Accepted).Count() * 100 / status.JudgeTasks.Count;
            //        Display = score.ToString() + "(" + statuses.Count + ")";
            //        if (score == 0)
            //            Css = "rank-red";
            //        else if (score != 100)
            //            Css = "rank-orange";
            //        else
            //            Css = "rank-green";
            //        Key1 = score;
            //        Key2 = statuses.Count;
            //        Key3 = status.JudgeTasks.Sum(x => x.TimeUsage);
            //    }
            //}
            #endregion
            #region ACM赛制逻辑
            else if (contest.Format == ContestFormat.ACM)
            {
                if (statuses.Count == 0)
                {
                    StatusID = null;
                    Css = "";
                    Display = "";
                }
                else
                {
                    var status = statuses.Where(x => x.ResultAsInt == (int)JudgeResult.Accepted).FirstOrDefault();
                    var penalty_count = statuses.Where(x => !Status.FreeResults.Contains((JudgeResult)x.ResultAsInt)).Count();
                    if (status == null)
                    {
                        Display = "(-" + penalty_count + ")";
                        Css = "rank-red";
                        StatusID = null;
                        Key1 = 0;
                        Key2 = 0;
                        Key3 = penalty_count;
                    }
                    else
                    {
                        Css = "rank-green";
                        StatusID = status.ID;
                        penalty_count = statuses.Where(x => !Status.FreeResults.Contains((JudgeResult)x.ResultAsInt) && x.Time < status.Time).Count();
                        var penalty_time = (status.Time - status.Contest.Begin).Add(new TimeSpan(0, 20 * penalty_count, 0));
                        Display = penalty_time.ToString("c");
                        if (penalty_count > 0)
                            Display += "<br/>(-" + penalty_count + ")";
                        StatusID = status.ID;
                        Key1 = 1;
                        Key2 = (int)penalty_time.TotalSeconds;
                        Key3 = penalty_count;
                    }
                }
            }
            #endregion
            #region CF、TC赛制逻辑
            else if (contest.Format == ContestFormat.Codeforces)
            {
                DB db = new DB();
                var hack_success = (from h in db.Hacks
                                    where h.Status.ProblemID == problem.ID
                                    && h.HackerID == user.ID
                                    && h.ResultAsInt == (int)HackResult.Success
                                    select h).Count();
                var hack_failed = (from h in db.Hacks
                                   where h.Status.ProblemID == problem.ID
                                   && h.HackerID == user.ID
                                   && h.ResultAsInt == (int)HackResult.Failure
                                   select h).Count();
                Key2 = hack_success;
                Key3 = hack_failed;
                if (statuses.Count == 0)
                {
                    StatusID = null;
                    Css = "";
                    Display = "";
                }
                else
                {
                    var status = statuses.Last();
                    if (status.Result == JudgeResult.Accepted)
                    {
                        var max = credit;
                        var score = 0;
                        TimeSpan time;
                        //if (contest.Format == ContestFormat.Codeforces)
                        //{
                        score = Convert.ToInt32(max * (1 - 0.004 * (status.Time - status.Contest.Begin).TotalMinutes));
                        time = status.Time - contest.Begin;
                        //}
                        //else
                        //{
                        //    var glance = (from g in problem.Glances
                        //                  where g.UserID == user.ID
                        //                  select g).SingleOrDefault();
                        //    score = Convert.ToInt32(max * (1 * 0.004 * (status.Time - glance.Time).TotalMinutes));
                        //    time = status.Time - glance.Time;
                        //}
                        score -= 50 * statuses.Where(x => !Status._FreeResults.Contains((JudgeResult)x.ResultAsInt)).Count();
                        if (score < max * 0.3)
                            score = Convert.ToInt32(max * 0.3);
                        Key1 = score;
                        Css = "rank-green";
                        Display = Key1 + "<br/>(" + time.ToString("hh':'mm") + ")";
                        StatusID = status.ID;
                    }
                    else
                    {
                        Css = "rank-red";
                        Display = "(-" + statuses.Count + ")";
                    }
                }
            }
            #endregion
            #region CC赛制逻辑
            else//未实现
            {
                
            }
            #endregion
        }
        public int? StatusID { get; set; }
        public string Css { get; set; }
        public string Display { get; set; }
        public int Key1 { get; set; }
        public int Key2 { get; set; }
        public int Key3 { get; set; }
    } 
}