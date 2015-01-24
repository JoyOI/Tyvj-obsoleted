using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;

namespace Tyvj.ViewModels
{
    public class vSolution
    {
        static string[] Css = { "green", "orange", "blue", "purple", "red" };
        public vSolution() { }
        public vSolution(Solution Solution) 
        {
            ID = Solution.ID;
            Title = Solution.Title;
            Username = HttpUtility.HtmlEncode(Solution.User.Username);
            Gravatar = "/Avatar/" + Solution.UserID;
            UserID = Solution.UserID;
            ProblemID = Solution.ProblemID;
            ProblemTitle = Solution.Problem.Title;
            Tags = "";
            var i = 0;
            foreach (var t in Solution.SolutionTags)
            {
                if (t.AlgorithmTag.FatherID != null)
                {
                    if (t.AlgorithmTag.Father.Title == t.AlgorithmTag.Title || t.AlgorithmTag.Father.Children.Count == 1)
                    {
                        Tags += "<span class='post-info "+Css[i++%5]+" label'>"+t.AlgorithmTag.Father.Title+"</span>";
                    }
                    else
                    {
                        Tags += "<span class='post-info " + Css[i++ % 5] + " label'>" + t.AlgorithmTag.Father.Title + " " + t.AlgorithmTag.Title + "</span>";
                    }
                }
            }
        }
        public int ID { get; set; }
        public string Title { get; set; }
        public int ProblemID { get; set; }
        public string ProblemTitle { get; set; }
        public string Username { get; set; }
        public int UserID { get; set; }
        public string Gravatar { get; set; }
        public string Tags { get; set; }
    }
}