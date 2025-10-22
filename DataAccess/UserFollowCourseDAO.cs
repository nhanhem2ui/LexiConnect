using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{

    public class UserFollowCourseDAO : IGenericDAO<UserFollowCourse>
    {
        private readonly AppDbContext _db;

        public UserFollowCourseDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<UserFollowCourse> GetAllQueryable(Expression<Func<UserFollowCourse, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<UserFollowCourse> query = _db.UserFollowCourses;
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

        public async Task<UserFollowCourse?> GetAsync(Expression<Func<UserFollowCourse, bool>> predicate)
        {
            return await _db.UserFollowCourses
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching UserFollowCourse or null if none found
        }

        public async Task<bool> AddAsync(UserFollowCourse entity)
        {
            _db.UserFollowCourses.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(UserFollowCourse entity)
        {
            _db.UserFollowCourses.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int UserFollowCourseId)
        {
            var UserFollowCourse = await _db.UserFollowCourses.FindAsync(UserFollowCourseId);
            if (UserFollowCourse == null)
            {
                return false; // UserFollowCourse not found
            }

            _db.UserFollowCourses.Remove(UserFollowCourse);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<UserFollowCourse, bool>> predicate)
        {
            return await _db.UserFollowCourses.AnyAsync(predicate);
        }
    }
}