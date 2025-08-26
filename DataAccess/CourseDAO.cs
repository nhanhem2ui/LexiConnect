using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class CourseDAO : IGenericDAO<Course>
    {

        private readonly AppDbContext _db;

        public CourseDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<Course> GetAllQueryable(Expression<Func<Course, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<Course> query = _db.Courses;
            if (asNoTracking)
            {
                query = query.AsNoTracking(); // Use AsNoTracking for read-only queries
            }
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            return query;
        }

        public async Task<Course?> GetAsync(Expression<Func<Course, bool>> predicate)
        {
            return await _db.Courses
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching Course or null if none found
        }

        public async Task<bool> AddAsync(Course entity)
        {
            _db.Courses.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(Course entity)
        {
            _db.Courses.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int CourseId)
        {
            var Course = await _db.Courses.FindAsync(CourseId);
            if (Course == null)
            {
                return false; // Course not found
            }

            _db.Courses.Remove(Course);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<Course, bool>> predicate)
        {
            return await _db.Courses.AnyAsync(predicate);
        }
    }
}