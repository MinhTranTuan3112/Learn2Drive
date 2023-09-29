using Driving_License.Models;

namespace Driving_License.ViewModels
{
    public class RentViewModel
    {
        public List<Vehicle> VehicleList {get; set;} = new List<Vehicle>();
        public List<string> BrandList {get;set;} = new List<string>();
        public List<string> TypeList {get; set;} = new List<string>();
    }
}
    