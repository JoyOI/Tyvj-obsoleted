using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;

namespace Tyvj.ViewModels
{
    public class vProblemStatus
    {
        public vProblemStatus() { }
        public vProblemStatus(Status Status) 
        {
            ID = Status.ID;
            Result = Status.Result.ToString();
            ResultAsInt = Status.ResultAsInt;
            Time = Status.Time.ToString("MM-dd");
        }
        public int ID { get; set; }
        public string Result { get; set; }
        public int ResultAsInt { get; set; }
        public string Time { get; set; }
    }
}