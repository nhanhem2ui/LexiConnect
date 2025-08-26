using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{

    public class UserFavoriteDAO : IGenericDAO<UserFavorite>
    {
        private readonly AppDbContext _db;

        public UserFavoriteDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<UserFavorite> GetAllQueryable(Expression<Func<UserFavorite, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<UserFavorite> query = _db.UserFavorites;
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

        public async Task<UserFavorite?> GetAsync(Expression<Func<UserFavorite, bool>> predicate)
        {
            return await _db.UserFavorites
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching UserFavorite or null if none found
        }

        public async Task<bool> AddAsync(UserFavorite entity)
        {
            _db.UserFavorites.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(UserFavorite entity)
        {
            _db.UserFavorites.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int UserFavoriteId)
        {
            var UserFavorite = await _db.UserFavorites.FindAsync(UserFavoriteId);
            if (UserFavorite == null)
            {
                return false; // UserFavorite not found
            }

            _db.UserFavorites.Remove(UserFavorite);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<UserFavorite, bool>> predicate)
        {   
            return await _db.UserFavorites.AnyAsync(predicate);
        }
    }
}