using Driving_License.Models;
using Driving_License.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Driving_License.Controllers
{
    [Route("api/rent-admin")]
    [ApiController]
    public class RentAPIcontroller : ControllerBase
    {
        private readonly DrivingLicenseContext _context;

        private static List<Rent> RentList = null;

        public RentAPIcontroller(DrivingLicenseContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllRent()
        {
            try
            {
                RentList = await _context.Rents.ToListAsync();
                return Ok(RentList);
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi trong quá trình xử lý yêu cầu.");
            }
        }
        [HttpGet]
        [Route("{userid}")]
        public async Task<IActionResult> GetRentByUser(Guid userid)
        {
            try
            {
                RentList = await _context.Rents.Where(x => x.UserId.Equals(userid)).ToListAsync();
                return Ok(RentList);
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi trong quá trình xử lý yêu cầu.");
            }

        }
        [HttpGet]
        [Route("filter/{startDate}/{endDate}")]
        public async Task<IActionResult> GetRentByDate(DateTime startDate,DateTime endDate)
        {
            try
            {
                RentList = await _context.Rents.Where(x => x.StartDate >= startDate && x.EndDate <= endDate)
                    .OrderBy(x => x.StartDate).ToListAsync();
                return Ok(RentList);
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi trong quá trình xử lý yêu cầu.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> InsertRent(Rent rent)
        {
            try
            {
                var findrent = await _context.Rents.FindAsync(rent.RentId);
                if (findrent != null)
                {
                    return Ok("Mã đơn thuê đã tồn tại");
                }else
                {   
                    rent.RentId = Guid.NewGuid();
                    await _context.Rents.AddAsync(rent);
                    await _context.SaveChangesAsync();
                    return Ok(rent);
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi trong quá trình xử lý yêu cầu.");
            }
        }
        [HttpPut]
        [Route("{rentid}")]
        public async Task<IActionResult> UpdateRent(Guid rentid,Rent rent)
        {
            try
            {
                if (rentid != rent.RentId)
                {
                    return NotFound("Mã đơn thuê sửa không đúng");
                }
                else
                {
                    var findrent = await _context.Rents.FindAsync(rent.RentId);
                    if(findrent != null)
                    {
                        findrent.StartDate = rent.StartDate;
                        findrent.EndDate = rent.EndDate;
                        findrent.TotalRentPrice = rent.TotalRentPrice;
                        findrent.Status = rent.Status;
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        return NotFound("Mã đơn thuê không tìm thấy trong database");
                    }
                    return Ok(rent);
                    
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi trong quá trình xử lý yêu cầu.");
            }
        }
        [HttpDelete]
        [Route("{rentid}")]
        public async Task<IActionResult> DeleteRent(Guid rentid)
        {
            try
            {
                var findrent = await _context.Rents.FindAsync(rentid);
                if(findrent != null)
                {
                    _context.Rents.Remove(findrent);
                    _context.SaveChanges();
                    return Ok(findrent);
                }
                else
                {
                    return NotFound("Không tìm thấy đơn thuê cần xóa");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi trong quá trình xử lý yêu cầu.");
            }
        }

    }
}
