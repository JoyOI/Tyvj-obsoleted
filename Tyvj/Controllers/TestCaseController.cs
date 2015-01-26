using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tyvj.DataModels;

namespace Tyvj.Controllers
{
    public class TestCaseController : BaseController
    {
        // GET: TestCase
        [Authorize]
        public ActionResult Index(int id)
        {
            var tc = DbContext.TestCases.Find(id);
            if(tc == null)return Message("没有找到该测试数据！");
            var problem = tc.Problem;
            if (problem.Hide && CurrentUser.Role < UserRole.Master && problem.UserID != CurrentUser.ID)
                return Message("没有找到该测试数据！");
            if (problem.VIP && CurrentUser.Role < UserRole.VIP && problem.UserID != CurrentUser.ID)
                return Message("没有找到该测试数据！");
            if (tc.Input.Length + tc.Output.Length > 1024 * 1024)
                return Message("测试数据过大，不支持查看！");
            var cnt = (from tcp in DbContext.TestCasePaids
                       where tcp.UserID == CurrentUser.ID
                       && tcp.TestCaseID == id
                       select tcp).Count();
            if (cnt == 0)
            {
                if (CurrentUser.Coins < 10)
                    return Message("您的金币不足，无法查看数据！");
                DbContext.TestCasePaids.Add(new TestCasePaid
                {
                    TestCaseID = id,
                    UserID = CurrentUser.ID
                });
                CurrentUser.Coins -= 10;
                DbContext.SaveChanges();
            }
            return View(tc);
        }
    }
}