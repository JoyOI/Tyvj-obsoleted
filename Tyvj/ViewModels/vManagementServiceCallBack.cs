using System;

namespace Tyvj.ViewModels
{
    public class vManagementServiceCallBack
    {
        public string StateMachineId { get; set; }
        public string InputBlobId { get; set; }
        public string Status { get; set; }
        public int Memory { get; set; }
        public int Time { get; set; }
        public string Hint { get; set; }
    }
}