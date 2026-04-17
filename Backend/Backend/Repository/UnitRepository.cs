using Backend.Models;

namespace Backend.Repository
{
    public interface UnitRepository
    {

        Task<List<Unit>> GetAllUnits(int courseId, bool? isActive);
        Task AddUnit(Unit Unit);
        Task UpdateUnit(Unit Unit);
        Task DeleteUnits(List<int> ids);
        Task SaveUnit();
        Task<List<UserProgress>> GetUserProgress(int userId, int courseId);
        Task CompleteUnit(int userId, int Unit);

        Task<int> GetMaxOrderIndex(int courseId);
        Task<List<Unit>> GetUnitsByIds(List<int> ids);
        Task<Unit> GetUnitById(int Unit);

    }
}