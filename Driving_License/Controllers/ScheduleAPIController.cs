using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Driving_License.Models;
using Driving_License.Utils;
using Microsoft.EntityFrameworkCore;

namespace Driving_License.Controllers
{
    [Route("api/scheduleapi")]
    [ApiController]
    public class ScheduleAPIController : ControllerBase
    {
        private readonly DrivingLicenseContext _context;
        private static List<Schedule> scheduleslist = null;
        public ScheduleAPIController(DrivingLicenseContext content)
        {
            _context=content;
        }

        [HttpGet]
        public IActionResult ViewSchedule()
        {
            try
            {
                scheduleslist = _context.Schedules.ToList();
                return Ok(scheduleslist);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult DangKyHoc([FromBody]Schedule schedule)
        {
            try
            {
                var usersession = System.Text.Json.JsonSerializer.Deserialize<Account>(HttpContext.Session.GetString("usersession"));
                var user = _context.Users.SingleOrDefault(x => x.AccountId.Equals(usersession.AccountId));
                schedule.UserId = user.UserId;
                var teacher = _context.Teachers.SingleOrDefault(x => x.TeacherId.Equals(schedule.TeacherId));
                _context.Schedules.Add(schedule);
                _context.SaveChanges();
                return Ok($"Bạn đã đăng ký học thành công khóa học của giảng viên {teacher.FullName}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
