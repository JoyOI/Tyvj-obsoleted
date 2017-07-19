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
        private static object obj = new object();

        // GET: JoyOI
        public ActionResult Callback()
        {
            using (var sr = new StreamReader(Request.InputStream))
            {
                var body = sr.ReadToEnd();
                var callback = JsonConvert.DeserializeObject<vManagementServiceCallBack>(body);
                callback.Memory /= 1024;

                var status = DbContext.Statuses.Single(x => x.StateMachineId == callback.StateMachineId);
                try
                {
                    if (callback.Status == "System Error")
                    {
                        foreach (var x in status.JudgeTasks)
                        {
                            x.ResultAsInt = (int)DataModels.JudgeResult.SystemError;
                            x.Hint = callback.Hint;
                        }
                    }
                    else if (callback.Status == "Compile Error")
                    {
                        foreach (var x in status.JudgeTasks)
                        {
                            x.ResultAsInt = (int)DataModels.JudgeResult.CompileError;
                            x.Hint = callback.Hint;
                        }
                    }
                    else if (callback.Status == "Validator Error")
                    {
                        var jt = status.JudgeTasks.Single(x => x.TestCase.InputBlobId == callback.InputBlobId.ToString());
                        jt.ResultAsInt = (int)DataModels.JudgeResult.SystemError;
                        jt.Hint = "Validator Error: " + callback.Hint;
                    }
                    else if (callback.Status == "Runtime Error")
                    {
                        var jt = DbContext.JudgeTasks.Where(x => x.StatusID == status.ID).Single(x => x.TestCase.InputBlobId == callback.InputBlobId.ToString());
                        jt.ResultAsInt = (int)DataModels.JudgeResult.RuntimeError;
                        jt.TimeUsage = callback.Time;
                        jt.MemoryUsage = callback.Memory;
                        jt.Hint = callback.Hint;
                    }
                    else if (callback.Status == "Time Limit Exceeded")
                    {
                        var jt = DbContext.JudgeTasks.Where(x => x.StatusID == status.ID).Single(x => x.TestCase.InputBlobId == callback.InputBlobId.ToString());
                        jt.ResultAsInt = (int)DataModels.JudgeResult.TimeLimitExceeded;
                        jt.TimeUsage = callback.Time;
                        jt.MemoryUsage = callback.Memory;
                    }
                    else if (callback.Status == "Memory Limit Exceeded")
                    {
                        var jt = DbContext.JudgeTasks.Where(x => x.StatusID == status.ID).Single(x => x.TestCase.InputBlobId == callback.InputBlobId.ToString());
                        jt.ResultAsInt = (int)DataModels.JudgeResult.MemoryLimitExceeded;
                        jt.TimeUsage = callback.Time;
                        jt.MemoryUsage = callback.Memory;
                    }
                    else if (callback.Status == "Presentation Error")
                    {
                        var jt = DbContext.JudgeTasks.Where(x => x.StatusID == status.ID).Single(x => x.TestCase.InputBlobId == callback.InputBlobId.ToString());
                        jt.ResultAsInt = (int)DataModels.JudgeResult.PresentationError;
                        jt.TimeUsage = callback.Time;
                        jt.MemoryUsage = callback.Memory;
                        jt.Hint = callback.Hint;
                    }
                    else if (callback.Status == "Wrong Answer")
                    {
                        var jt = DbContext.JudgeTasks.Where(x => x.StatusID == status.ID).Single(x => x.TestCase.InputBlobId == callback.InputBlobId.ToString());
                        jt.ResultAsInt = (int)DataModels.JudgeResult.WrongAnswer;
                        jt.TimeUsage = callback.Time;
                        jt.MemoryUsage = callback.Memory;
                        jt.Hint = callback.Hint;
                    }
                    else if (callback.Status == "Accepted")
                    {
                        var jt = DbContext.JudgeTasks.Where(x => x.StatusID == status.ID).Single(x => x.TestCase.InputBlobId == callback.InputBlobId.ToString());
                        jt.ResultAsInt = (int)DataModels.JudgeResult.Accepted;
                        jt.TimeUsage = callback.Time;
                        jt.MemoryUsage = callback.Memory;
                    }
                    else if (callback.Status == "Running")
                    {
                        var jt = DbContext.JudgeTasks.Where(x => x.StatusID == status.ID).Single(x => x.TestCase.InputBlobId == callback.InputBlobId.ToString());
                        jt.ResultAsInt = (int)DataModels.JudgeResult.Running;
                    }

                    DbContext.SaveChanges();

                    lock (obj)
                    {
                        if (DbContext.JudgeTasks
                            .Where(x => x.StatusID == status.ID)
                            .Where(x => x.ResultAsInt == (int)DataModels.JudgeResult.Running || x.ResultAsInt == (int)DataModels.JudgeResult.Pending)
                            .Count() == 0)
                        {
                            status.ResultAsInt = DbContext.JudgeTasks.Where(x => x.StatusID == status.ID).Max(x => x.ResultAsInt);
                            status.MemoryUsage = DbContext.JudgeTasks.Where(x => x.StatusID == status.ID).Max(x => x.MemoryUsage);
                            status.TimeUsage = DbContext.JudgeTasks.Where(x => x.StatusID == status.ID).Sum(x => x.TimeUsage);
                            status.Score = DbContext.JudgeTasks
                                .Where(x => x.StatusID == status.ID)
                                .Where(x => x.ResultAsInt == (int)DataModels.JudgeResult.Accepted)
                                .Count() * 100 / status.JudgeTasks.Count;
                            DbContext.SaveChanges();
                        }
                    }
                    
                    SignalR.UserHub.context.Clients.Group("Status").onStatusChanged(new vStatus(status));//推送新状态
                }
                catch (Exception ex)
                {
                    throw new Exception(JsonConvert.SerializeObject(callback) + " " + ex.ToString());
                }
            }
            return Content("ok");
        }
    }
}