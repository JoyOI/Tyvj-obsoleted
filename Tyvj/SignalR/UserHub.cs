using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Tyvj.SignalR
{
    public class UserHub : Hub
    {
        public static Microsoft.AspNet.SignalR.IHubContext context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<UserHub>();
        public void Join(string Name)
        {
            Groups.Add(Context.ConnectionId, Name);
        }
    }
}