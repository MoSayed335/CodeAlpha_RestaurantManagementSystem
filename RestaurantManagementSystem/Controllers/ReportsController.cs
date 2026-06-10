using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Services;

namespace RestaurantManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportingService _reportingService;

        public ReportsController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailySalesReport()
        {
            var report = await _reportingService.GetDailySalesReportAsync();
            return Ok(report);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var summary = await _reportingService.GetDashboardSummaryAsync();
            return Ok(summary);
        }
    }
}
