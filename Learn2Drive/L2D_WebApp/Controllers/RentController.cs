﻿using L2D_DataAccess.Models;
using L2D_DataAccess.Utils;
using L2D_DataAccess.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using L2D_WebApp.Filters;
using System.Net.Mail;
using System.Text.Json;
using X.PagedList;
using Microsoft.VisualBasic;
using Azure;
using Microsoft.AspNetCore.JsonPatch;

namespace L2D_WebApp.Controllers
{
    public record RentHistoryData
    {
        public string Keyword { get; set; }
        public int DayRangeValue { get; set; } = -1;
    }
    //[LoginFilter]
    public class RentController : Controller
    {

        private readonly DrivingLicenseContext _context;

        public RentController(DrivingLicenseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rentViewModel = new RentViewModel();
            rentViewModel.BrandList = await _context.Vehicles.Select(vehicle => vehicle.Brand).Distinct().AsNoTracking().ToListAsync();
            rentViewModel.TypeList = await _context.Vehicles.Select(vehicle => vehicle.Type).Distinct().AsNoTracking().ToListAsync();
            return View("~/Views/Rent.cshtml", rentViewModel);
        }

        public async Task<IActionResult> RentDetail(Guid vid)
        {
            var vehicle = await _context.Vehicles.AsNoTracking().SingleOrDefaultAsync(v => v.VehicleId.Equals(vid));
            ViewBag.VehicleId = vid;
            return View("~/Views/RentDetail.cshtml", vehicle);
        }

        public async Task<string> getUserIDFromSession()
        {
            string AccountID = string.Empty;
            var usersession = JsonSerializer.Deserialize<Account>(HttpContext.Session.GetString("usersession"));

            var user = await _context.Users.SingleOrDefaultAsync(user => user.AccountId.Equals(usersession.AccountId));
            return (user is not null) ? user.UserId.ToString() : string.Empty;
        }


        //[LoginFilter]
        [HttpPost]
        [Route("api/rent/insert/{id:guid}")]
        public async Task<IActionResult> AddRentAPI([FromRoute] Guid id, [FromBody] Rent rent)
        {
            var UserIDString = await getUserIDFromSession();
            rent.UserId = Guid.Parse(UserIDString);
            rent.Status = "Chờ duyệt";
            await _context.AddAsync(rent);
            await _context.SaveChangesAsync();
            return Ok();
        }

        public async Task<PageResult<T>> GetPagedDataAsync<T>(IQueryable<T> query, int page, int pageSize)
        {
            //Get total number of rows in table
            int totalCount = await query.CountAsync();

            //Calculate total pages
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            int takingNums = pageSize;
            int skipNums = (page - 1) * pageSize;
            if (totalCount < pageSize)
            {
                takingNums = totalCount;
            }
            List<T> items = await query.Skip(skipNums)
                                       .Take(takingNums)
                                       .ToListAsync();
            return new PageResult<T>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = page,
                PageSize = pageSize,
                Items = items
            };
        }

        public async Task<PageResult<Rent>> GetRentHistoryDataPaging(IQueryable<Rent> query, int page, int pageSize)
        {

            //Get total number of rows in table
            int totalCount = await query.CountAsync();

            //Calculate total pages
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            int takingNums = pageSize;
            int skipNums = (page - 1) * pageSize;
            if (totalCount < pageSize)
            {
                takingNums = totalCount;
            }
            List<Rent> items = await query.Include(rent => rent.Vehicle)
                                       .Skip(skipNums)
                                       .Take(takingNums)
                                       .ToListAsync();
            return new PageResult<Rent>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = page,
                PageSize = pageSize,
                Items = items
            };
        }

        [HttpGet]
        [Route("api/vehicles")]
        [Produces("application/json")]
        public async Task<IActionResult> Vehicles()
        {
            var vehicleList = await _context.Vehicles.AsNoTracking().ToListAsync();
            return Ok(vehicleList);
        }

        [HttpPost]
        [Route("api/rent/filter/{uid:guid}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetRentByDate([FromRoute] Guid uid, [FromBody] RentHistoryData data)
        {
            if (await _context.Users.AnyAsync(user => user.UserId.Equals(uid)) == false)
            {
                return NotFound();
            }
            var query = _context.Rents.Where(rent => rent.UserId.Equals(uid)).AsQueryable();
            if (!string.IsNullOrEmpty(data.Keyword))
            {
                query = query.Where(rent => EF.Functions.Like(rent.Vehicle.Name, $"%{data.Keyword}%"));
            }
            if (data.DayRangeValue > 0)
            {
                DateTime Today = DateTime.Today;
                DateTime MarkDay = Today.AddDays((-1) * data.DayRangeValue);
                query = query.Where(rent => rent.StartDate >= MarkDay && rent.StartDate <= Today);
            }
            var rentDataList = await query
            .OrderBy(rent => rent.StartDate)
            .Select(rent => new
            {
                RentId = rent.RentId,
                VehicleName = rent.Vehicle.Name,
                StartDate = rent.StartDate,
                EndDate = rent.EndDate,
                TotalRentPrice = rent.TotalRentPrice,
                Status = rent.Status
            }).ToListAsync();
            return Ok(rentDataList);
        }

        [HttpPost]
        [Route("api/rent/filter/page/{uid:guid}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetRentDataPaging([FromRoute] Guid uid, [FromBody] RentHistoryData data, int page = 1)
        {
            if (await _context.Users.AnyAsync(user => user.UserId.Equals(uid)) == false)
            {
                return NotFound();
            }
            var query = _context.Rents.Where(rent => rent.UserId.Equals(uid)).AsQueryable();
            if (!string.IsNullOrEmpty(data.Keyword))
            {
                query = query.Where(rent => EF.Functions.Like(rent.Vehicle.Name, $"%{data.Keyword}%"));
            }
            if (data.DayRangeValue > 0)
            {
                DateTime Today = DateTime.Today;
                DateTime MarkDay = Today.AddDays((-1) * data.DayRangeValue);
                query = query.Where(rent => rent.StartDate >= MarkDay && rent.StartDate <= Today);
            }
            query = query.OrderBy(rent => rent.StartDate);


            const int pageSize = 8;
            var PagingResult = await GetRentHistoryDataPaging(query, page, pageSize);
            var finalRentHistoryData = new
            {
                TotalCount = PagingResult.TotalCount,
                TotalPages = PagingResult.TotalPages,
                PageNumber = PagingResult.PageNumber,
                PageSize = PagingResult.PageSize,
                rentData = PagingResult.Items.Select(rent => new
                {
                    RentId = rent.RentId,
                    VehicleName = rent.Vehicle.Name,
                    VehicleImage = rent.Vehicle.Image,
                    StartDate = rent.StartDate,
                    EndDate = rent.EndDate,
                    TotalRentPrice = rent.TotalRentPrice,
                    Status = rent.Status
                }).ToList()
            };
            return Ok(finalRentHistoryData);
        }


        [HttpDelete]
        [Route("api/rent/delete/{rid:guid}")]
        public async Task<IActionResult> DeleteUserRent([FromRoute] Guid rid)
        {
            if (!await _context.Rents.AnyAsync(rent => rent.RentId.Equals(rid)))
            {
                return NotFound($"Can't find any rents with id {rid}");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Rents.Where(r => r.RentId.Equals(rid)).ExecuteDeleteAsync();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"An error occurred during the request: {ex.Message}");
            }
            return NoContent();
        }


        [HttpGet]
        [Route("api/rent/{rid:guid}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetRentData([FromRoute] Guid rid)
        {
            var rent = await _context.Rents
                .Include(r => r.Vehicle)
                .AsSplitQuery()
                .AsNoTracking()
                .SingleOrDefaultAsync(r => r.RentId.Equals(rid));
            return (rent is null) ? NotFound("Can't find any rent") : Ok(rent);
        }



        [HttpPatch]
        [Route("api/rent/update/{rid:guid}")]
        public async Task<IActionResult> UpdateRent([FromRoute] Guid rid, [FromBody] JsonPatchDocument<Rent> patchDoc)
        {
            if (rid == Guid.Empty || patchDoc is null)
            {
                return BadRequest();
            }
            var rent = await _context.Rents.SingleOrDefaultAsync(rent => rent.RentId.Equals(rid));
            if (rent is null)
            {
                return NotFound("Can't find any rent");
            }
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                patchDoc.ApplyTo(rent, ModelState);
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"An error occurred during the request: {ex.Message}");
            }
            return NoContent();
        }

        [HttpPost]
        [Route("api/rents")]
        [Produces("application/json")]
        public async Task<IActionResult> GetAllRentData([FromBody] RentHistoryData data)
        {
            var query = _context.Rents
                .Include(rent => rent.Vehicle)
                .Include(rent => rent.User)
                .AsQueryable();
            if (data is not null)
            {
                if (!string.IsNullOrEmpty(data.Keyword))
                {
                    query = query.Where(rent => rent.Vehicle.Name.ToLower().Contains(data.Keyword.ToLower()));
                }
                if (data.DayRangeValue != -1)
                {
                    DateTime Today = DateTime.Today;
                    DateTime MarkDay = Today.AddDays((-1) * data.DayRangeValue);
                    query = query.Where(rent => rent.StartDate >= MarkDay && rent.StartDate <= Today);
                }
            }
            query = query.OrderByDescending(rent => rent.StartDate);
            var rentList = await query
                .Select(rent => new
                {
                    RentId = rent.RentId,
                    UserId = rent.UserId,
                    UserPhoneNumber = rent.User.PhoneNumber,
                    UserFullName = rent.User.FullName,
                    VehicleName = rent.Vehicle.Name,
                    VehicleImage = rent.Vehicle.Image,
                    StartDate = rent.StartDate,
                    EndDate = rent.EndDate,
                    Price = rent.TotalRentPrice,
                    Status = rent.Status
                })
                .ToListAsync();
            return Ok(rentList);
        }
    }
}
