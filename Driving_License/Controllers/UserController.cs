using Driving_License.Models;
using Driving_License.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using System.Text.Json;

namespace Driving_License.Controllers
{
    public class UserController : Controller
    {
        private readonly DrivingLicenseContext _context;

        public UserController(DrivingLicenseContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var usersession = JsonSerializer.Deserialize<Account>(HttpContext.Session.GetString("usersession"));            
            if (usersession.Role.Equals("user"))
            {
                var user = _context.Users.FirstOrDefault(x => x.AccountId.Equals(usersession.AccountId));
                ViewBag.user = user;
                return View("/Views/EditInfo.cshtml");
            }
            else if (usersession.Role.Equals("lecturer"))
            {
                return View("/Views/EditInfo.cshtml");
            }
            return RedirectToAction("Index","Home");
        }
        public IActionResult EditInfo(IFormCollection Form)
        {
            string FullName = Form["FullName"];
            string BirthDate = Form["BirthDate"];
            string Email = Form["Email"];
            string Nationality = Form["Nationality"];
            string PhoneNumber = Form["PhoneNumber"];
            string Address = Form["Address"];
            var filesend = Form.Files["Avatar"];
            var usersession = JsonSerializer.Deserialize<Account>(HttpContext.Session.GetString("usersession"));
            var user = _context.Users.FirstOrDefault(x => x.AccountId.Equals(usersession.AccountId));

            user.FullName = !string.IsNullOrEmpty(FullName) ? FullName : null;
            user.BirthDate = !BirthDate.IsNullOrEmpty() ? DateTime.Parse(BirthDate) : null;
            user.Email = !string.IsNullOrEmpty(Email) ? Email : null;
            user.Nationality = !string.IsNullOrEmpty(Nationality) ? Nationality : null;
            user.PhoneNumber = !string.IsNullOrEmpty(PhoneNumber) ? PhoneNumber : null;
            user.Address = !string.IsNullOrEmpty(Address) ? Address : null;
            if (filesend != null)
            {
                var filePath = Path.Combine("wwwroot/img/Avatar",user.UserId.ToString() + Path.GetExtension(filesend.FileName));
                using var filestream = new FileStream(filePath, FileMode.Create);
                filesend.CopyTo(filestream);
                user.Avatar = user.UserId.ToString() + Path.GetExtension(filesend.FileName);
                filestream.Close();
            }
            _context.Users.Update(user);
            _context.SaveChanges();
            return RedirectToAction("Index","User");
        }
    }
}
