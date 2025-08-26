using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class UserFavoriteRepository : IGenericRepository<UserFavorite>
    {
        private readonly IGenericDAO<UserFavorite> _dao;

        public UserFavoriteRepository(IGenericDAO<UserFavorite> dao)
        {
            _dao = dao;
        }

        public IQueryable<UserFavorite> GetAllQueryable(Expression<Func<UserFavorite, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<UserFavorite?> GetAsync(Expression<Func<UserFavorite, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(UserFavorite entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(UserFavorite entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int UserFavoriteId)
        {
            return await _dao.DeleteAsync(UserFavoriteId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<UserFavorite, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
