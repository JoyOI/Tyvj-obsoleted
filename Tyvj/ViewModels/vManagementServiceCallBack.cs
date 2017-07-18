using System;

namespace Tyvj.ViewModels
{
    public class vManagementServiceCallBack
    {
        public Guid StateMachineId { get; set; }
        public Guid? InputBlobId { get; set; }
        public string Status { get; set; }
        public int Memory { get; set; }
        public int Time { get; set; }
        public string Hint { get; set; }
    }
}