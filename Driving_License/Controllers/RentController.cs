//using Driving_License.Models;
//using Driving_License.Models.Users;
using Driving_License.Models;
using Driving_License.Utils;
using Driving_License.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Driving_License.Filters;
using System.Net.Mail;
using System.Text.Json;
using X.PagedList;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Logging;

namespace Driving_License.Controllers
{
    [LoginFilter]
    public class RentController : Controller
    {

        private readonly DrivingLicenseContext _context;

        public RentController(DrivingLicenseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            page = page < 1 ? 1 : page;
            int pagesize = 6;
            List<Vehicle> VehicleList = null;
            var jsonstring = TempData["vehiclelist"] as string;
            if (!jsonstring.IsNullOrEmpty())
            {
                VehicleList = JsonSerializer.Deserialize<List<Vehicle>>(jsonstring);
            }
            else
            {
                VehicleList = await _context.Vehicles.ToListAsync();
            }
            var rentViewModel = new RentViewModel();
            ViewBag.vehiclecount = VehicleList.Count;
            rentViewModel.VehicleList.AddRange(VehicleList.ToPagedList(page, pagesize));
            rentViewModel.BrandList = await _context.Vehicles.Select(vehicle => vehicle.Brand).Distinct().ToListAsync();
            rentViewModel.TypeList = await _context.Vehicles.Select(vehicle => vehicle.Type).Distinct().ToListAsync();

            int totalPage = (VehicleList.Count + pagesize - 1) / pagesize;
            ViewBag.totalPage = totalPage;
            ViewBag.currentPage = page;
            return View("~/Views/Rent.cshtml", rentViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SearchVec(IFormCollection Form)
        {
            string type = Form["type"];
            string brand = Form["brand"];
            string price = Form["price"];
            string keyword = Form["keyword"];
            var query = _context.Vehicles.AsQueryable();
            if (!type.IsNullOrEmpty())
            {
                query = query.Where(vehicle => vehicle.Type.Equals(type));
            }
            if (!brand.IsNullOrEmpty())
            {
                query = query.Where(vehicle => vehicle.Brand.Equals(brand));
            }
            if (!price.IsNullOrEmpty() && Decimal.Parse(price) > 0)
            {
                query = query.Where(vehicle => vehicle.RentPrice < Decimal.Parse(price));
            }
            if (!keyword.IsNullOrEmpty())
            {
                /*string pattern = string.Format("name like '%%{0}%%'", keyword);*/
                query = query.Where(x => x.Brand.ToLower().Contains(keyword.ToLower()) || x.Name.ToLower().Contains(keyword.ToLower()) || x.Type.ToLower().Contains(keyword.ToLower()));
                //query = query.Where(vehicle => vehicle.Name.Contains(pattern));
            }
            var VehicleList = await query.OrderBy(vehicle => vehicle.Name).ToListAsync();
            TempData["vehiclelist"] = JsonSerializer.Serialize(VehicleList);
            TempData["keyword"] = JsonSerializer.Serialize(keyword);
            return RedirectToAction("Index", "Rent");
        }
        public async Task<IActionResult> RentDetail(Guid carid)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(x => x.VehicleId.Equals(carid));
            return View("/Views/RentDetail.cshtml", vehicle);
        }

        public async Task<IActionResult> InsertNewRent(IFormCollection form)
        {
            string vechicleid = form["vechicleid"];
            string date1 = form["partydate"];
            string date2 = form["partydate2"];
            string totalpriceInput = form["totalpriceInput"];
            var UserIDString = await getUserIDFromSession();

            Rent RentOrder = new Rent()
            {
                RentId = new Guid(),
                UserId = Guid.Parse(UserIDString),
                VehicleId = Guid.Parse(vechicleid),
                StartDate = DateTime.Parse(date1),
                EndDate = DateTime.Parse(date2),
                Status = "false",
                TotalRentPrice = decimal.Parse(totalpriceInput),
            };
            await _context.Rents.AddAsync(RentOrder);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Rent");
        }
        public async Task<string> getUserIDFromSession()
        {
            string AccountID = string.Empty;
            var usersession = JsonSerializer.Deserialize<Account>(HttpContext.Session.GetString("usersession"));
            var user = await _context.Users.FirstOrDefaultAsync(user => user.AccountId.Equals(usersession.AccountId));
            return (user is not null) ? user.UserId.ToString() : string.Empty;
        }
    }

}
