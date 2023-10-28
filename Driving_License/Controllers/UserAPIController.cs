using Driving_License.Filters;
using Driving_License.Models;
using Driving_License.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Driving_License.Controllers
{
    [LoginFilter]
    [Route("api/userapi")]
    [ApiController]
    public class UserAPIController : ControllerBase
    {
        private readonly DrivingLicenseContext _context;
        
        public UserAPIController(DrivingLicenseContext content)
        {
            _context=content;
        }
        [HttpGet]
        public async Task<IActionResult> returnUser()
        {
            try
            {
                var user = HttpContext.Session.GetString("usersession");

                if (user != null)
                {
                    return Ok(user);
                }
                else
                {
                    return NotFound("Bạn chưa đăng nhập");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi trong quá trình xử lý yêu.");
            }
        }

        [HttpPost]
        [Route("{LicenseId}")]
        public async Task<IActionResult> RegisterExam(string LicenseId)
        {
            if (string.IsNullOrEmpty(LicenseId))
            {
                return BadRequest("LicenseId is missing or empty in the request body.");
            }

            var usersession = System.Text.Json.JsonSerializer.Deserialize<Account>(HttpContext.Session.GetString("usersession"));
            var user = _context.Users.SingleOrDefault(x => x.AccountId.Equals(usersession.AccountId));

            try
            {
                ExamProfile e = new ExamProfile
                {
                    UserId = user.UserId,
                    LicenseId = LicenseId,
                };

                await _context.ExamProfiles.AddAsync(e);
                await _context.SaveChangesAsync();

                return Ok($"Bạn đã đăng ký thi bằng lái xe {LicenseId} thành công");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }

}
