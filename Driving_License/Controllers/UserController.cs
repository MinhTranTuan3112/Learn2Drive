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
        {
            var usersession = JsonSerializer.Deserialize<Account>(HttpContext.Session.GetString("usersession"));
            if (usersession.Role.Equals("user"))
            {
                ViewBag.user = user;
                return View("/Views/EditInfo.cshtml");
            }
            else if (usersession.Role.Equals("lecturer"))
            {
                return View("/Views/EditInfo.cshtml");
            }
        }
        {
            var usersession = JsonSerializer.Deserialize<Account>(HttpContext.Session.GetString("usersession"));

            user.FullName = !string.IsNullOrEmpty(FullName) ? FullName : null;
            user.BirthDate = !BirthDate.IsNullOrEmpty() ? DateTime.Parse(BirthDate) : null;
            user.Email = !string.IsNullOrEmpty(Email) ? Email : null;
            user.Nationality = !string.IsNullOrEmpty(Nationality) ? Nationality : null;
            user.PhoneNumber = !string.IsNullOrEmpty(PhoneNumber) ? PhoneNumber : null;
            user.Address = !string.IsNullOrEmpty(Address) ? Address : null;
            {
                using var filestream = new FileStream(filePath, FileMode.Create);
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
        }
    }
}
