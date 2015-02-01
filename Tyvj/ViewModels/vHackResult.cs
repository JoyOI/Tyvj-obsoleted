using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;

namespace Tyvj.ViewModels
{
    public class vHackResult
    {
        public vHackResult() { }
        public vHackResult(Hack hack) 
        {
            ID = hack.ID;
            HackerID = hack.HackerID;
            HackerName = Helpers.ColorName.GetNicknameHtml(hack.Hacker.Username, hack.Hacker.Role);
            HackerGravatar = Helpers.Gravatar.GetAvatarURL(hack.Hacker.Gravatar, 180);
            DefenderID = hack.DefenderID;
            DefenderName = Helpers.ColorName.GetNicknameHtml(hack.Defender.Username, hack.Defender.Role);
            DefenderGravatar = Helpers.Gravatar.GetAvatarURL(hack.Defender.Gravatar, 180);
            Result = CommonEnums.HackResultDisplay[hack.ResultAsInt];
            if (hack.Result == HackResult.Success)
                Css = "judgeState1";
            else if (hack.Result == HackResult.BadData || hack.Result == HackResult.DatamakerError || hack.Result == HackResult.SystemError)
                Css = "judgeState7";
            else if (hack.Result == HackResult.Failure)
                Css = "judgeState3";
            else
                Css = "judgeState10";
            ProblemID = hack.Status.ProblemID;
            ProblemTitle = hack.Status.Problem.Title;
            StatusID = hack.StatusID;
        }
        public vHackResult(Hack hack, bool Defender)
        {
            ID = hack.ID;
            HackerID = hack.HackerID;
            HackerName = Helpers.ColorName.GetNicknameHtml(hack.Hacker.Username, hack.Hacker.Role);
            HackerGravatar = Helpers.Gravatar.GetAvatarURL(hack.Hacker.Gravatar, 180);
            DefenderID = hack.DefenderID;
            DefenderName = Helpers.ColorName.GetNicknameHtml(hack.Defender.Username, hack.Defender.Role);
            DefenderGravatar = Helpers.Gravatar.GetAvatarURL(hack.Defender.Gravatar, 180);
            Result ="Hacked";
            Css = "status-text-wa";
            ProblemID = hack.Status.ProblemID;
            ProblemTitle = hack.Status.Problem.Title;
            StatusID = hack.StatusID;
        }

        public int ID { get; set; }
        public int HackerID { get; set; }
        public string HackerName { get; set; }
        public int DefenderID { get; set; }
        public string DefenderName { get; set; }
        public string Result { get; set; }
        public string Css { get; set; }
        public int StatusID { get; set; }
        public string ProblemTitle { get; set; }
        public int ProblemID { get; set; }
        public string HackerGravatar { get; set; }
        public string DefenderGravatar { get; set; }
    }
}