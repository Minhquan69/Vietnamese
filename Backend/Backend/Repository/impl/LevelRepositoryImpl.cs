using Backend.Data;
using Backend.dto;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.impl
{
    public class LevelRepositoryImpl : LevelRepository
    {
        private readonly AppDbContext _context;
        public LevelRepositoryImpl(AppDbContext context)
        {
            _context = context;
        }
        /*
         * lấy tất cả các cấp độ
         * 26/03/2026
         * thuphuong21072004
         */
        public async Task<List<Level>> GetAllLevels( bool? isActive)
        {
            var query = _context.Levels.AsQueryable();

            if (isActive.HasValue)
                query = query.Where(l => l.IsActive == isActive.Value);

            return await query
                .OrderBy(x => x.OrderIndex)
                .ToListAsync();
        }
        /*
         * lấy cấp độ theo id
         * 26/03/2026
         * thuphuong21072004
         */
        public async Task<Level> GetLevelById(int id)
        {
            return await _context.Levels.FindAsync(id);
        }
        /*
         * thêm cấp độ mới
         * 26/03/2026
         * thuphuong21072004
         */
        public async Task AddLevel(Level level)
        {
            await _context.Levels.AddAsync(level);
        }
        /*
         * sửa thông tin cấp độ
         * 26/03/2026
         * thuphuong21072004
         */
        public async Task UpdateLevel(Level level)
        {
            _context.Levels.Update(level);
        }
        /*
         * xóa cấp độ và toàn bộ dữ liệu liên quan
         * 26/03/2026
         * thuphuong21072004
         */
        public async Task DeleteLevels(List<int> ids)
        {
            var levels = await _context.Levels
                .Where(x => ids.Contains(x.LevelId))
                .ToListAsync();

            _context.Levels.RemoveRange(levels);

            await _context.SaveChangesAsync();
        }
        /*
         * lưu thay đổi vào database
         * 26/03/2026
         * thuphuong21072004
         */
        public async Task SaveLevel()
        {
            await _context.SaveChangesAsync();
        }
        /*
         * lấy order index lớn nhất của cấp độ
         * 
         * thuphuong21072004
         */
        public async Task<int> GetMaxOrderIndex()
        {
            var max = await _context.Levels
                .Select(x => (int?)x.OrderIndex)
                .MaxAsync();

            return max ?? 0;
        }
        /*
         * 
         * 
         * thuphuong21072004
         */
        public IQueryable<Level> GetQueryable()
        {
          
            return _context.Levels.AsNoTracking();
        }
    }
}
