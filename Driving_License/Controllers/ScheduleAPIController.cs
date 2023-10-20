﻿using Microsoft.AspNetCore.Http;
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

        [HttpGet]
        [Route("search/{starttime}/{endtime}")]
        public IActionResult ViewScheduleFilterTime(TimeSpan starttime, TimeSpan endtime)
        {
            try
            {
                scheduleslist = _context.Schedules.Where(x => x.StartTime >= starttime && x.EndTime <= endtime).ToList();
                return Ok(scheduleslist);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("search/{date}")]
        public IActionResult ViewScheduleByDate(DateTime date)
        {
            try
            {
                scheduleslist = _context.Schedules.Where(x => x.Date.HasValue && x.Date.Value.Date == date.Date).ToList();
                return Ok(scheduleslist);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("search/{teacherid}")]
        public IActionResult ViewScheduleByTeacher(Guid teacherid)
        {
            try
            {
                scheduleslist = _context.Schedules.Where(x => x.TeacherId.Equals(teacherid)).ToList();
                return Ok(scheduleslist);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult DangKyHoc([FromBody] Schedule schedule)
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
        [HttpPut]
        [Route("{ScheduleId}")]
        public async Task<IActionResult> Update(Guid ScheduleId, Schedule schedule)
        {
            try
            {
                if (ScheduleId != schedule.ScheduleId)
                {
                    return NotFound("Mã lịch học sửa không đúng");
                }
                else
                {
                    var findschedule = await _context.Schedules.FindAsync(schedule.ScheduleId);
                    if (findschedule != null)
                    {
                        findschedule.TeacherId = schedule.TeacherId;
                        findschedule.UserId = schedule.UserId;
                        findschedule.LicenseId = schedule.LicenseId;
                        findschedule.StartTime = schedule.StartTime;
                        findschedule.EndTime = schedule.EndTime;
                        findschedule.Price = schedule.Price;
                        findschedule.Address = schedule.Address;
                        findschedule.Status = schedule.Status;
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        return NotFound("Mã lịch học không tìm thấy trong database");
                    }
                    return Ok(schedule);

                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpDelete]
        [Route("{scheduleid}")]
        public async Task<IActionResult> DeleteRent(Guid scheduleid)
        {
            try
            {
                var findschedule = await _context.Schedules.FindAsync(scheduleid);
                if (findschedule != null)
                {
                    _context.Schedules.Remove(findschedule);
                    _context.SaveChanges();
                    return Ok(findschedule);
                }
                else
                {
                    return NotFound("Không tìm thấy lịch học cần xóa");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi trong quá trình xử lý yêu cầu.");
            }
        }


    }
}
