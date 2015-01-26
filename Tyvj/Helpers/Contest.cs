using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;

namespace Tyvj.Helpers
{
    public static class Contest
    {
        public static bool UserInContest(int UserID, int ContestID)
        { 
            var DbContext = new DB();
            var contest = DbContext.Contests.Find(ContestID);
            switch (contest.JoinMethod)
            {
                case ContestJoinMethod.Everyone:
                    return true;
                case ContestJoinMethod.Group:
                    if ((from gm in DbContext.GroupMembers where gm.UserID == UserID select gm).Count() > 0)
                        return true;
                    else
                        return false;
                case ContestJoinMethod.Appoint:
                case ContestJoinMethod.Password:
                    if ((from cr in DbContext.ContestRegisters where cr.UserID == UserID && ContestID == cr.ContestID select cr).Count() > 0)
                        return true;
                    else
                        return false;
                default: return false;
            }
        }
        public static IQueryable<Status> GetStatuses(int ProblemID, int ContestID)
        { 
            var DbContext = new DB();
            return (from s in DbContext.Statuses
                    where s.ContestID == ContestID
                    && s.ProblemID == ProblemID
                    orderby s.ID ascending
                    select s);
        }

        public static int GetStatusesCount(int ProblemID, int ContestID)
        {
            var DbContext = new DB();
            return (from s in DbContext.Statuses
                    where s.ContestID == ContestID
                    && s.ProblemID == ProblemID
                    orderby s.ID ascending
                    select s).Count();
        }
    }
}