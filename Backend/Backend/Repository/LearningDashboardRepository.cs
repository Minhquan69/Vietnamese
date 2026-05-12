namespace Backend.Repository
{
    public interface LearningDashboardRepository
    {
        Task<decimal> GetTotalXpAsync(int userId);
        Task<decimal> GetXpOnDateAsync(int userId, DateTime dayUtc);
        Task<Dictionary<DateTime, decimal>> GetDailyXpAsync(int userId, DateTime startUtcInclusive, DateTime endUtcInclusive);
        Task<HashSet<DateTime>> GetActivityDatesUtcAsync(int userId, DateTime sinceUtc);
        Task<int> CountPassedQuizzesOnDateAsync(int userId, DateTime dayUtc);
        Task<int> CountUnitsCompletedOnDateAsync(int userId, DateTime dayUtc);
        Task<int> CountWrongAnswersSinceAsync(int userId, DateTime sinceUtc);
        Task<int> CountPassedQuizzesAsync(int userId);
        Task<int> CountUnitsCompletedAsync(int userId);
    }
}
