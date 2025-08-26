using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{

    public class UserFollowerDAO : IGenericDAO<UserFollower>
    {
        private readonly AppDbContext _db;

        public UserFollowerDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<UserFollower> GetAllQueryable(Expression<Func<UserFollower, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<UserFollower> query = _db.UserFollowers;
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

        public async Task<UserFollower?> GetAsync(Expression<Func<UserFollower, bool>> predicate)
        {
            return await _db.UserFollowers
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching UserFollower or null if none found
        }

        public async Task<bool> AddAsync(UserFollower entity)
        {
            _db.UserFollowers.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(UserFollower entity)
        {
            _db.UserFollowers.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int UserFollowerId)
        {
            var UserFollower = await _db.UserFollowers.FindAsync(UserFollowerId);
            if (UserFollower == null)
            {
                return false; // UserFollower not found
            }

            _db.UserFollowers.Remove(UserFollower);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<UserFollower, bool>> predicate)
        {
            return await _db.UserFollowers.AnyAsync(predicate);
        }
    }
}
