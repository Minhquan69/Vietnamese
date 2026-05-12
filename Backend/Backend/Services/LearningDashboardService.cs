using Backend.dto;

namespace Backend.Services
{
    public interface LearningDashboardService
    {
        Task<LearningDashboardDto> GetDashboardAsync();
    }
}
