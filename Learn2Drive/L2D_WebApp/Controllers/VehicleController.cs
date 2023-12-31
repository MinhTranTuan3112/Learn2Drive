﻿using L2D_WebApp.Controllers;
using L2D_DataAccess.Models;
using L2D_DataAccess.Utils;
using L2D_DataAccess.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using L2D_WebApp.Utils;

namespace L2D_WebApp.Controllers
{
    public record VehicleRequestData
    {
        public string Keyword { get; set; }
        public List<string> Types { get; set; }
        public List<string> Brands { get; set; }
        public decimal StartPrice { get; set; } = Decimal.MinusOne;
        public decimal EndPrice { get; set; } = Decimal.MinusOne;
    }

    public record VehicleFormData
    {
        public string Name { get; set; }
        public IFormFile Image { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }
        public int Years { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public decimal RentPrice { get; set; }
        public string Description { get; set; }
    };

    public class VehicleController : Controller
    {
        private readonly DrivingLicenseContext _context;
        private readonly ImageUtils _imageUtils;
        public VehicleController(DrivingLicenseContext context, ImageUtils imageUtils)
        {
            _context = context;
            _imageUtils = imageUtils;
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

        [HttpPost]
        [Route("api/vehicle/{page}")]
        [Produces("application/json")]
        public async Task<IActionResult> Page([FromBody] VehicleRequestData data, [FromRoute] int page = 1)
        {
            const int pageSize = 8;
            IQueryable<Vehicle> query = _context.Vehicles.AsQueryable();
            if (!string.IsNullOrEmpty(data.Keyword))
            {
                query = query.Where(vehicle => EF.Functions.Like(vehicle.Name, $"%{data.Keyword}%"));
                //query = query.Where(vehicle => vehicle.Name.Contains(data.Keyword));
            }
            if (data.Types.Count > 0)
            {
                foreach (var type in data.Types)
                {
                    query = query.Where(vehicle => data.Types.Contains(vehicle.Type));
                }
            }
            if (data.Brands.Count > 0)
            {
                foreach (var brand in data.Brands)
                {
                    query = query.Where(vehicle => data.Brands.Contains(vehicle.Brand));
                }
            }
            if (data.StartPrice != Decimal.MinusOne)
            {
                query = query.Where(vehicle => vehicle.RentPrice >= data.StartPrice);
            }
            if (data.EndPrice != Decimal.MinusOne)
            {
                query = query.Where(vehicle => vehicle.RentPrice <= data.EndPrice);
            }
            query = query.OrderBy(vehicle => vehicle.Name);
            PageResult<Vehicle> pageResult = await GetPagedDataAsync<Vehicle>(query, page, pageSize);
            return Ok(pageResult);
        }

        [HttpGet]
        [Route("api/vehicles/types")]
        [Produces("application/json")]
        public async Task<IActionResult> GetAllVehicleTypes()
        {
            var typeList = await _context.Vehicles
                .Select(vehicle => vehicle.Type).Distinct().AsNoTracking().ToListAsync();
            return Ok(typeList);
        }

        [HttpGet]
        [Route("api/vehicles/filterdata")]
        [Produces("application/json")]
        public async Task<IActionResult> GetVehiclesFilterData()
        {
            var vehicleList = await _context.Vehicles.AsNoTracking().ToListAsync();
            var brandList = vehicleList.Select(vehicle => vehicle.Brand).Distinct().ToList();
            var typeList = vehicleList.Select(vehicle => vehicle.Type).Distinct().ToList();
            return Ok(new
            {
                brandList = brandList,
                typeList = typeList
            });
        }

        [HttpGet]
        [Route("api/vehicle/topone")]
        [Produces("application/json")]
        public async Task<IActionResult> GetTop1RentVehicle()
        {
            int RentCount = await _context.Rents.CountAsync();
            if (RentCount == 0)
            {
                return NoContent();
            }
            var vehicleWithMostRents = await _context.Rents
                .GroupBy(r => r.VehicleId)
                .Select(g => new
                {
                    VehicleID = g.Key,
                    RentCount = g.Count()
                })
                .OrderByDescending(g => g.RentCount)
                .Join(_context.Vehicles, rent => rent.VehicleID,
                vehicle => vehicle.VehicleId, (rent, vehicle) => new
                {
                    Vehicle = vehicle,
                    RentCount = rent.RentCount
                }).AsNoTracking().FirstOrDefaultAsync();
            return Ok(vehicleWithMostRents);
        }

        private async Task<string> HandleVehicleImage(VehicleFormData data)
        {
            if (data.Image is null)
            {
                return string.Empty;
            }
            string FinalImagePath = string.Empty;
            string FormattedName = data.Name.Replace(" ", "");
            var filePath = Path.Combine("wwwroot/img/vehicle", FormattedName + Path.GetExtension(data.Image.FileName));
            using var fileStream = new FileStream(filePath, FileMode.CreateNew);
            try
            {
                await data.Image.CopyToAsync(fileStream);
                FinalImagePath = FormattedName + Path.GetExtension(data.Image.FileName);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Save vehicle image error: {ex.Message}");
            }
            finally
            {
                fileStream.Close();
            }
            return FinalImagePath;
        }

        [HttpPost]
        [Route("api/vehicles/add")]
        public async Task<IActionResult> AddVehicle([FromForm] VehicleFormData data)
        {
            if (data is null)
            {
                return BadRequest();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var vehicle = new Vehicle
                {
                    Name = data.Name,
                    Brand = data.Brand,
                    Type = data.Type,
                    Years = data.Years,
                    ContactNumber = data.ContactNumber,
                    Address = data.Address,
                    RentPrice = data.RentPrice,
                    Description = data.Description
                };

                if (data.Image is not null)
                {
                    //var vehicleImagePath = await HandleVehicleImage(data);
                    //if (!string.IsNullOrEmpty(vehicleImagePath))
                    //{
                    //    vehicle.Image = vehicleImagePath;
                    //}
                    var imagePath = data.Name.Replace(" ", "") + Path.GetExtension(data.Image.FileName);
                    await _imageUtils.UpdateImageAsync(data.Image, $"img/vehicle/{imagePath}");
                    vehicle.Image = imagePath;
                }

                await _context.Vehicles.AddAsync(vehicle);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"An error occurred: {ex.Message}");
            }
            return NoContent();
        }

        [HttpGet]
        [Route("api/vehicles/{vid:guid}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetVehicleFromID([FromRoute] Guid vid)
        {
            if (vid == Guid.Empty)
            {
                return BadRequest();
            }
            var vehicle = await _context.Vehicles.AsNoTracking().SingleOrDefaultAsync(v => v.VehicleId.Equals(vid));
            return (vehicle is not null) ? Ok(vehicle) : NotFound($"Can't find any vehicle with id {vid}");
        }

        [HttpDelete]
        [Route("api/vehicles/delete/{vid:guid}")]
        public async Task<IActionResult> RemoveVehicle([FromRoute] Guid vid)
        {
            if (vid == Guid.Empty)
            {
                return BadRequest();
            }
            var vehicle = await _context.Vehicles.SingleOrDefaultAsync(v => v.VehicleId.Equals(vid));
            if (vehicle is null)
            {
                return NotFound($"Can't find any vehicle with id {vid}");
            }
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {

                if (await _context.Rents.CountAsync(r => r.VehicleId.Equals(vid)) > 0)
                {
                    await _context.Rents.Where(rent => rent.VehicleId.Equals(vid))
                        .ExecuteDeleteAsync();
                }

                //Delete vehicle image 
                await _imageUtils.DeleteImageAsync($"img/vehicle/{vehicle.Image}");

                await _context.Vehicles.Where(vehicle => vehicle.VehicleId.Equals(vid))
                    .ExecuteDeleteAsync();
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

        [HttpPut]
        [Route("api/vehicles/update/{vid:guid}")]
        public async Task<IActionResult> UpdateVehicle([FromRoute] Guid vid, [FromForm] VehicleFormData data)
        {
            if (vid == Guid.Empty || data is null)
            {
                return BadRequest();
            }
            var vehicle = await _context.Vehicles.SingleOrDefaultAsync(v => v.VehicleId.Equals(vid));
            if (vehicle is null)
            {
                return NotFound($"Can't find any vehicle with id {vid}");
            }
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (data.Image is not null)
                {
                    var vehicleImagePath = await HandleVehicleImage(data);
                    if (!string.IsNullOrEmpty(vehicleImagePath))
                    {
                        await _context.Vehicles.Where(v => v.VehicleId.Equals(vid))
                            .ExecuteUpdateAsync(setters => setters.SetProperty(v => v.Image, vehicleImagePath));
                    }
                }

                await _context.Vehicles.Where(v => v.VehicleId.Equals(vid))
                    .ExecuteUpdateAsync(setters => setters.SetProperty(v => v.Name, data.Name)
                    .SetProperty(v => v.Brand, data.Brand)
                    .SetProperty(v => v.Type, data.Type)
                    .SetProperty(v => v.Years, data.Years)
                    .SetProperty(v => v.Address, data.Address)
                    .SetProperty(v => v.RentPrice, data.RentPrice)
                    .SetProperty(v => v.Description, data.Description));
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
    }
}
