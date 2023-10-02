using Driving_License.Models;
using Driving_License.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Index()
        {
            const string UserProfileViewPath = "~/Views/EditInfo.cshtml";
            var usersession = JsonSerializer.Deserialize<Account>(HttpContext.Session.GetString("usersession"));
            if (usersession.Role.Equals("user"))
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.AccountId.Equals(usersession.AccountId));
                ViewBag.user = user;
            }
            return View(UserProfileViewPath);
        }
        [HttpPost]
        public async Task<IActionResult> EditInfo(IFormCollection form)
        {
            string FullName = form["FullName"];
            string BirthDate = form["BirthDate"];
            string Email = form["Email"];
            string Nationality = form["Nationality"];
            string PhoneNumber = form["PhoneNumber"];
            string Address = form["Address"];
            var filesend = form.Files["Avatar"];
            var usersession = JsonSerializer.Deserialize<Account>(HttpContext.Session.GetString("usersession"));
            var user = await _context.Users.FirstOrDefaultAsync(u => u.AccountId.Equals(usersession.AccountId));

            user.FullName = !string.IsNullOrEmpty(FullName) ? FullName : null;
            user.BirthDate = !BirthDate.IsNullOrEmpty() ? DateTime.Parse(BirthDate) : null;
            user.Email = !string.IsNullOrEmpty(Email) ? Email : null;
            user.Nationality = !string.IsNullOrEmpty(Nationality) ? Nationality : null;
            user.PhoneNumber = !string.IsNullOrEmpty(PhoneNumber) ? PhoneNumber : null;
            user.Address = !string.IsNullOrEmpty(Address) ? Address : null;
            if (filesend is not null)
            {
                var filePath = Path.Combine("wwwroot/img/Avatar", user.UserId.ToString() + Path.GetExtension(filesend.FileName));
                using var filestream = new FileStream(filePath, FileMode.Create);
                try
                {
                    await filesend.CopyToAsync(filestream);
                    user.Avatar = user.UserId.ToString() + Path.GetExtension(filesend.FileName);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Update user error: " + ex.Message);
                }
                finally
                {
                    filestream.Close();
                }
            }
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "User");
        }
    }
}
