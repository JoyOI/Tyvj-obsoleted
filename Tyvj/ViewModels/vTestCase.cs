using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tyvj.DataModels;

namespace Tyvj.ViewModels
{
    public class vTestCase
    {
        public vTestCase() { }
        public vTestCase(TestCase testcase)
        {
            ID = testcase.ID;
            Input = testcase.Input;
            Output = testcase.Output;
        }
        public int ID { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
    }
}