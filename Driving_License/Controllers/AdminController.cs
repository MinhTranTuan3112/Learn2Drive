using Driving_License.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Driving_License.Models;
namespace Driving_License.Controllers
{
    public class AdminController : Controller
    {
        private readonly DrivingLicenseContext _context;

        public AdminController(DrivingLicenseContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View("/Views/Admin.cshtml");
        }

        public IActionResult UserList()
        {
            var query = from a in _context.Accounts
                        join u in _context.Users on a.AccountId equals u.AccountId into userGroup
                        from u in userGroup.DefaultIfEmpty()
                        select new
                        {
                            Username = a.Username,
                            FullName = u.FullName,
                            Avatar = u.Avatar,
                            Role = a.Role,
                            Email = u.Email,
                            AccountID = a.AccountId
                        };

            var listAccount = query.ToList();

            ViewBag.listAccount = listAccount;
            return View("/Views/UserList.cshtml");
        }

        public IActionResult UserManage(string AccountID)
        {
            var user = _context.Users.FirstOrDefaultAsync(x => x.AccountId.Equals(AccountID));
            ViewBag.user = user;
            return View("/Views/UserManageInfo.cshtml");
        }
    }
}
