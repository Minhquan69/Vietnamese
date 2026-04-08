using Backend.Models;

namespace Backend.Repository
{
    public interface LevelRepository
    {
        Task<List<Level>> GetLevels();
        Task<Level> GetLevelById(int id);
        Task AddLevel(Level level);
        Task UpdateLevel(Level level);
        Task DeleteLevels(List<int> ids);
        Task Save();
        Task<int> GetMaxOrderIndex();
    }
}
