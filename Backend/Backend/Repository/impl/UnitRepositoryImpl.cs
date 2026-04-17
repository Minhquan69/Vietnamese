using Backend.Data;
using Backend.Models;
using Backend.Repository;
using Microsoft.EntityFrameworkCore;

public class UnitRepositoryImpl : UnitRepository
{
    private readonly AppDbContext _context;

    public UnitRepositoryImpl(AppDbContext context)
    {
        _context = context;
    }

    /*
     * Lấy danh sách Unit theo courseId
     * 
     * thuphuong21072004
     */
    public async Task<List<Unit>> GetAllUnits(int courseId, bool? isActive)
    {
        var query = _context.Units
            .Where(l => l.CourseId == courseId)
            .AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(l => l.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(l => l.OrderIndex)
            .ToListAsync();
    }
    /*
     * lấy thông tin Unit theo UnitId
     * 
     * thuphuong21072004
     */
    public async Task<Unit> GetUnitById(int UnitId)
    {
        return await _context.Units.FindAsync(UnitId);
    }
    /*
     * Thêm Unit mới
     * 
     * thuphuong21072004
     */
    public async Task AddUnit(Unit Unit)
    {
        await _context.Units.AddAsync(Unit);
    }
    /*
     * Cập nhật thông tin Unit
     * 
     * thuphuong21072004
     */
    public async Task UpdateUnit(Unit Unit)
    {
        _context.Units.Update(Unit);
    }
    /*
     * xóa Unit và toàn bộ quiz, question, answer liên quan
     * 
     * thuphuong21072004
     */
    public async Task DeleteUnits(List<int> ids)
    {
        var Units = await _context.Units
            .Where(l => ids.Contains(l.UnitId))
            .ToListAsync();

        _context.Units.RemoveRange(Units);
        
    }
    /*
     * Lưu thay đổi vào database
     * 
     * thuphuong21072004
     */
    public async Task SaveUnit()
    {
        await _context.SaveChangesAsync();
    }
    /*
     * lấy tiến trình học của user theo course
     * 
     * thuphuong21072004
     */
    public async Task<List<UserProgress>> GetUserProgress(int userId, int courseId)
    {
        if (courseId == 0)
        {
            return await _context.UserProgress
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        return await _context.UserProgress
            .Include(x => x.Unit)
            .Where(x => x.UserId == userId && x.Unit.CourseId == courseId)
            .ToListAsync();
    }
    /*
     * cập nhật tiến trình học (complete Unit)
     * 
     * thuphuong21072004
     */
    public async Task CompleteUnit(int userId, int UnitId)
    {
        var progress = await _context.UserProgress
            .FirstOrDefaultAsync(x => x.UserId == userId && x.UnitId == UnitId);

        if (progress != null)
        {
            progress.Status = true;
            progress.CompletedDate = DateTime.Now;
            _context.UserProgress.Update(progress); 
        }
        else
        {
            await _context.UserProgress.AddAsync(new UserProgress
            {
                UserId = userId,
                UnitId = UnitId,
                Status = true,
                CompletedDate = DateTime.Now
            });
        }
    }
    /*
     * lấy ordex max
     * 
     * thuphuong21072004
     */
    public async Task<int> GetMaxOrderIndex(int courseId)
    {
        var maxOrder = await _context.Units
            .Where(x => x.CourseId == courseId)
            .MaxAsync(x => (int?)x.OrderIndex);

        return maxOrder ?? 0;
    }
    /*
     * lấy nhiều Unit 1 lúc theo UnitId
     * 
     * thuphuong21072004
     */
    public async Task<List<Unit>> GetUnitsByIds(List<int> ids)
    {
        return await _context.Units
            .Where(x => ids.Contains(x.UnitId))
            .ToListAsync();
    }
}