using System.Threading.Tasks;
using RestaurantManagementSystem.Dtos;

namespace RestaurantManagementSystem.Services
{
    public interface IReportingService
    {
        Task<DailySalesReportDto> GetDailySalesReportAsync();
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    }
}
