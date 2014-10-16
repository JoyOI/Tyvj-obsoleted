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
            return (from cr in DbContext.ContestRegisters
                    where cr.UserID == UserID
                    && cr.ContestID == ContestID
                    select cr).Count() > 0;
        }
    }
}