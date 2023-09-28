using Driving_License.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;

namespace Driving_License.Controllers
{
    public class RentController : Controller
    {
        private readonly DrivingLicenseContext _context;

        public RentController(DrivingLicenseContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.Vehicles = _context.Vehicles.ToList();
            return View("/Views/Rent.cshtml");
        }

        public async Task<IActionResult> SearchVec(IFormCollection Form)
        {
            string type = Form["type"];
            string brand = Form["brand"];
            string price = Form["price"];
            string keyword = Form["keyword"];
            var query = _context.Vehicles.ToList();
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
