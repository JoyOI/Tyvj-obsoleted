using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;

namespace Tyvj.ViewModels
{
    public class vRank
    {
        public vRank() { }
        public vRank(User user, int rank)
        {
            UserID = user.ID;
            Nickname = user.Username;
            Credit = user.Ratings.Sum(x => x.Credit) + 1500;
            Rank = rank;
            Gravatar = Helpers.Gravatar.GetAvatarURL(user.Gravatar, 200);
            Motto = user.Motto;
            if (Motto == null)
                Motto = "";
            var DbContext = new DB();
            var ACCount = (from s in DbContext.Statuses where s.ResultAsInt == (int)JudgeResult.Accepted && s.UserID == UserID select s.ProblemID).Distinct().Count();
            var TotalCount = (from s in DbContext.Statuses where s.UserID == UserID select s.ProblemID).Count();
            AC = ACCount;
            Total = TotalCount;
            if(Total == 0)
            {
                ACRate = "0.00%";
            }
            else
            {
                ACRate = (Convert.ToDouble(AC) / Convert.ToDouble(Total)).ToString("0.00") + "%";
            }
        }
        public int UserID { get; set; }
        public string Nickname { get; set; }
        public int Credit { get; set; }
        public int Rank { get; set; }
        public string Gravatar { get; set; }
        public string Motto { get; set; }
        public int AC { get; set; }
        public string ACRate { get; set; }
        public int Total { get; set; }
    }
}