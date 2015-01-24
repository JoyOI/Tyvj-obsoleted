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
            Nickname = HttpUtility.HtmlEncode(user.Username);
            Credit = user.Ratings.Sum(x => x.Credit) + 1500;
            Rank = rank;
            Gravatar = Helpers.Gravatar.GetAvatarURL(user.Gravatar, 200);
            Motto = HttpUtility.HtmlEncode(user.Motto);
            if (Motto == null)
                Motto = "";
            var DbContext = new DB();
            var ACCount = user.AcceptedCount;
            var HideProblemIDs = (from p in Controllers.ProblemController.ProblemListCache
                                      where p.Hide == true
                                      select p.ID).ToList();
            var HideCount = (from s in DbContext.Statuses
                             where s.UserID == user.ID
                             && s.ResultAsInt == 0
                             && HideProblemIDs.Contains(s.ProblemID)
                             select s.ProblemID).Distinct().Count();
            ACCount -= HideCount;
            var TotalCount = user.SubmitCount;
            AC = ACCount;
            Total = TotalCount;
            if(Total == 0)
            {
                ACRate = "0.00%";
            }
            else
            {
                ACRate = (Convert.ToDouble(AC) * 100 / Convert.ToDouble(Total)).ToString("0.00") + "%";
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