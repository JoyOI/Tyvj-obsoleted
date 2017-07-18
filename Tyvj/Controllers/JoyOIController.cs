using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Newtonsoft.Json;
using Tyvj.ViewModels;

namespace Tyvj.Controllers
{
    public class JoyOIController : BaseController
    {
        // GET: JoyOI
        public ActionResult Callback()
        {
            using (var sr = new StreamReader(Request.InputStream))
            {
                var body = sr.ReadToEnd();
                var callback = JsonConvert.DeserializeObject<vManagementServiceCallBack>(body);
                var status = DbContext.Statuses.Single(x => x.StateMachineId == callback.StateMachineId.ToString());
                if (callback.Status == "System Error")
                {
                    foreach (var x in status.JudgeTasks)
                    {
                        x.Result = DataModels.JudgeResult.SystemError;
                        x.Hint = callback.Hint;
                    }
                }
                else if (callback.Status == "Compile Error")
                {
                    foreach (var x in status.JudgeTasks)
                    {
                        x.Result = DataModels.JudgeResult.CompileError;
                        x.Hint = callback.Hint;
                    }
                }
                else if (callback.Status == "Validator Error")
                {
                    var jt = status.JudgeTasks.Single(x => x.TestCase.InputBlobId == callback.InputBlobId.ToString());
                    jt.Result = DataModels.JudgeResult.SystemError;
                    jt.Hint = "Validator Error: " + callback.Hint;
                }
                else if (callback.Status == "Time Limit Exceeded")
                {
                    var jt = status.JudgeTasks.Single(x => x.TestCase.InputBlobId == callback.InputBlobId.ToString());
                    jt.Result = DataModels.JudgeResult.TimeLimitExceeded;
                    jt.TimeUsage = callback.Time;
                    jt.MemoryUsage = callback.Memory;
                }
                else if (callback.Status == "Memory Limit Exceeded")
                {
                    var jt = status.JudgeTasks.Single(x => x.TestCase.InputBlobId == callback.InputBlobId.ToString());
                    jt.Result = DataModels.JudgeResult.MemoryLimitExceeded;
                    jt.TimeUsage = callback.Time;
                    jt.MemoryUsage = callback.Memory;
                }
                else if (callback.Status == "Presentation Error")
                {
                    var jt = status.JudgeTasks.Single(x => x.TestCase.InputBlobId == callback.InputBlobId.ToString());
                    jt.Result = DataModels.JudgeResult.PresentationError;
                    jt.TimeUsage = callback.Time;
                    jt.MemoryUsage = callback.Memory;
                    jt.Hint = callback.Hint;
                }
                else if (callback.Status == "Wrong Answer")
                {
                    var jt = status.JudgeTasks.Single(x => x.TestCase.InputBlobId == callback.InputBlobId.ToString());
                    jt.Result = DataModels.JudgeResult.WrongAnswer;
                    jt.TimeUsage = callback.Time;
                    jt.MemoryUsage = callback.Memory;
                    jt.Hint = callback.Hint;
                }
                else if (callback.Status == "Accepted")
                {
                    var jt = status.JudgeTasks.Single(x => x.TestCase.InputBlobId == callback.InputBlobId.ToString());
                    jt.Result = DataModels.JudgeResult.Accepted;
                    jt.TimeUsage = callback.Time;
                    jt.MemoryUsage = callback.Memory;
                }
                else if (callback.Status == "Running")
                {
                    var jt = status.JudgeTasks.Single(x => x.TestCase.InputBlobId == callback.InputBlobId.ToString());
                    jt.Result = DataModels.JudgeResult.Running;
                }

                if (status.JudgeTasks.Count(x => x.Result == DataModels.JudgeResult.Running || x.Result == DataModels.JudgeResult.Pending) == 0)
                {
                    status.ResultAsInt = status.JudgeTasks.Max(x => x.ResultAsInt);
                    status.MemoryUsage = status.JudgeTasks.Max(x => x.MemoryUsage);
                    status.TimeUsage = status.JudgeTasks.Sum(x => x.TimeUsage);
                    status.Score = status.JudgeTasks.Where(x => x.Result == DataModels.JudgeResult.Accepted).Count() * 100 / status.JudgeTasks.Count;
                }

                DbContext.SaveChanges();
                SignalR.UserHub.context.Clients.Group("Status").onStatusChanged(new vStatus(status));//推送新状态
            }
            return Content("ok");
        }
    }
}