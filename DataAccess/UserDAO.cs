using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class UserDAO : IGenericDAO<Users>
    {
        private readonly AppDbContext _db;

        public UserDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<Users> GetAllQueryable(Expression<Func<Users, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<Users> query = _db.Users;
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

        public async Task<Users?> GetAsync(Expression<Func<Users, bool>> predicate)
        {
            return await _db.Users
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .Include(u => u.University)
                .FirstOrDefaultAsync(predicate); // Return the first matching User or null if none found
        }

        public async Task<bool> AddAsync(Users entity)
        {
            _db.Users.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(Users entity)
        {
            _db.Users.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int UserId)
        {
            var User = await _db.Users.FindAsync(UserId);
            if (User == null)
            {
                return false; // User not found
            }

            _db.Users.Remove(User);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<Users, bool>> predicate)
        {
            return await _db.Users.AnyAsync(predicate);
        }
    }
}
