using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;
using Tyvj.ViewModels;

namespace Tyvj.Helpers
{
    public static class Standings
    {
        public static List<vStanding> Build(int id)//contest id
        {
            DB db = new DB();
            List<vStanding> standings = new List<vStanding>();
            var contest = db.Contests.Find(id);
            var users = (from cr in db.ContestRegisters
                         where cr.ContestID == id
                         select cr.User).Distinct().ToList();
            foreach (var user in users)
                standings.Add(new vStanding(user, contest));
            Sort(contest.Format, ref standings);
            return standings;
        }
        public static void Sort(ContestFormat format, ref List<vStanding> standings)
        {
            switch (format)
            {
                case ContestFormat.OI:
                    standings = standings.OrderByDescending(x => x.Key1).ThenBy(x => x.Key2).ThenBy(x => x.Key3).ToList();
                    break;
                case ContestFormat.Codeforces:
                    standings = standings.OrderByDescending(x => x.Key1).ThenByDescending(x => x.Key2).ThenBy(x => x.Key3).ToList();
                    break;
                case ContestFormat.ACM:
                    standings = standings.OrderByDescending(x => x.Key1).ThenBy(x => x.Key2).ThenBy(x => x.Key3).ToList();
                    break;
                default: break;
            }
        }
        public static void Update(int user_id, int contest_id, ref List<vStanding> standings)
        {
            DB db = new DB();
            var user = db.Users.Find(user_id);
            var contest = db.Contests.Find(contest_id);
            var index = standings.FindIndex(x => x.UserID == user_id);
            var new_standing = new vStanding(user, contest);
            if (index < 0)
            {
                standings.Add(new_standing);
            }
            else
            {
                standings[index] = new_standing;
            }
            Sort(contest.Format, ref standings);

            //推送排名变化
            SignalR.UserHub.context.Clients.All.onStandingChanged(contest_id, new_standing);
        }
    }
}