using Driving_License.Models;
using Driving_License.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Driving_License.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAPIController : ControllerBase
    {
        private readonly DrivingLicenseContext _context;
        public UserAPIController(DrivingLicenseContext content)
        {
            _context=content;
        }

        public async Task<IActionResult> RegisterExam(IFormCollection form)
        {
            if (form["userID"].IsNullOrEmpty())
            {
                return BadRequest("You are not login");
            }
            Record r = new Record
            {
                UserId = Guid.Parse(form["userID"]),
                LicenseId = form["LicenseID"]
            };
            _context.Records.Add(r);
            _context.SaveChanges();
            return Ok(r);
        }
    }

}
