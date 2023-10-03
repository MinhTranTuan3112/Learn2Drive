//using Driving_License.Models;
//using Driving_License.Models.Users;
﻿using Driving_License.Models;
using Driving_License.Utils;
using Driving_License.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Driving_License.Filters;
using System.Net.Mail;
using System.Text.Json;

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
        public async Task<IActionResult> Index()
        {
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
            rentViewModel.VehicleList.AddRange(VehicleList);
            rentViewModel.BrandList = await _context.Vehicles.Select(vehicle => vehicle.Brand).Distinct().ToListAsync();
            rentViewModel.TypeList = await _context.Vehicles.Select(vehicle => vehicle.Type).Distinct().ToListAsync();
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
                /*query = query.Where(vehicle => vehicle.Name.Contains(pattern));*/
            }
            var VehicleList = await query.OrderBy(vehicle => vehicle.Name).ToListAsync();
            TempData["vehiclelist"] = JsonSerializer.Serialize(VehicleList);
            return RedirectToAction("Index", "Rent");
        }
    }
}
