using CustomFanController;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FanContController : ControllerBase
    {
        private readonly ILogger<FanContController> Logger;
        private readonly List<FanController> FanControllers;

        public FanContController(ILogger<FanContController> Logger, FanControllers FanControllers)
        {
            this.Logger = Logger;
            this.FanControllers = FanControllers.Controllers;
        }

        [HttpGet(nameof(GetAvailableDevices))]
        public Dictionary<byte, string> GetAvailableDevices()
        {
            var available = new Dictionary<byte, string>();

            foreach (var controller in FanControllers)
            {
                available.Add(controller.DeviceID, controller.DeviceName);
            }

            return available;
        }

        [HttpGet(nameof(GetReadings))]
        public async Task<IEnumerable<Readings>> GetReadings()
        {
            var readings = new List<Readings>();

            foreach (var controller in FanControllers)
            {
                readings.Add(await controller.GetReadings());
            }

            return readings;
        }
    }
}