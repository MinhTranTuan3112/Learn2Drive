using Driving_License.Filters;
using Driving_License.Models;
using Driving_License.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using System.Text.Json;

namespace Driving_License.Controllers
{
    [LoginFilter]

    public class UserController : Controller
    {
        private readonly DrivingLicenseContext _context;

        public UserController(DrivingLicenseContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            const string UserProfileViewPath = "~/Views/UserProfile.cshtml";
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
            string Email = form["Email"];
            string Nationality = form["Nationality"];
            string CCCD = form["CCCD"];
            string Address = form["Address"];
            string PhoneNumber = form["PhoneNumber"];
            string BirthDate = form["BirthDate"];
            var filesend = form.Files["Avatar"];
            var usersession = JsonSerializer.Deserialize<Account>(HttpContext.Session.GetString("usersession"));
            var user = await _context.Users.FirstOrDefaultAsync(u => u.AccountId.Equals(usersession.AccountId));

            user.FullName = FullName;
            user.BirthDate = !BirthDate.IsNullOrEmpty() ? DateTime.Parse(BirthDate) : null;
            user.Email = Email;
            user.Nationality = Nationality;
            user.PhoneNumber = PhoneNumber;
            user.Address = Address;
            user.Cccd = CCCD;

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

        public async Task<IActionResult> ViewExamForm()
        {
            var usersession = JsonSerializer.Deserialize<Account>(HttpContext.Session.GetString("usersession"));
            var user = await _context.Users.FirstOrDefaultAsync(x => x.AccountId.Equals(usersession.AccountId));
            ViewBag.user = user;
            var listlicense = _context.Licenses.ToList();
            return View("~/Views/DangKyThi.cshtml", listlicense);
        }

        public async Task<IActionResult> RegisterExam(IFormCollection form)
        {
            if(form == null)
            {
                return NotFound();
            }
            Record r = new Record
            {
                UserId = Guid.Parse(form["userID"]),
                LicenseId = form["LicenseID"]
            };
            _context.Records.Add(r);
            _context.SaveChanges();
            return Ok();
        }
    }
}
