using Driving_License.Utils;
using Driving_License.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Driving_License.Controllers
{
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
            
            ViewBag.Vehicles = await _context.Vehicles.ToListAsync();
            return View("/Views/Rent.cshtml");
        }

        public async Task<IActionResult> SearchVec(IFormCollection Form)
        {
            string type = Form["type"];
            string brand = Form["brand"];
            string price = Form["price"];
            string keyword = Form["keyword"];
            var query = await _context.Vehicles.ToListAsync();
            if (!type.IsNullOrEmpty())
            {
                query = query.Where(x => x.Type.Equals(type)).ToList();
            }
            if (!brand.IsNullOrEmpty())
            {
                query = query.Where(x => x.Brand.Equals(brand)).ToList();
            }
            if (!price.IsNullOrEmpty() && Decimal.Parse(price) > 0)
            {
                query = query.Where(x => x.RentPrice < Decimal.Parse(price)).ToList();
            }
            if (!keyword.IsNullOrEmpty())
            {
                query = query.Where(x => x.Brand.ToLower().Contains(keyword.ToLower()) || x.Name.ToLower().Contains(keyword.ToLower()) || x.Type.ToLower().Contains(keyword.ToLower())).ToList();
            }
            ViewBag.Vehicles = query;
            return View("/Views/Rent.cshtml");
        }

        

    }
}
