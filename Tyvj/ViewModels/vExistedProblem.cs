using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;

namespace Tyvj.ViewModels
{
    public class vExistedProblem
    {
        public vExistedProblem() { }
        public vExistedProblem(Problem Problem)
        {
            ID = Problem.ID;
            Title = HttpUtility.HtmlEncode(Problem.Title);
            Content = Helpers.String.CleanHTML(Problem.Background + " " + Problem.Description + " " + Problem.Input + " " + Problem.Output);
            if (Content.Length > 255)
            {
                Content = Content.Substring(0, 253) + "...";
            }
        }
        public int ID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}