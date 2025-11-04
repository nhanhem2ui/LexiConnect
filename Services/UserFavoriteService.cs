using BusinessObjects;
using Repositories;
using System.Linq.Expressions;

namespace Services
{
    public class UserFavoriteService : IGenericService<UserFavorite>
    {
        private readonly IGenericRepository<UserFavorite> _repo;

        public UserFavoriteService(IGenericRepository<UserFavorite> repo)
        {
            _repo = repo;
        }

        public IQueryable<UserFavorite> GetAllQueryable(Expression<Func<UserFavorite, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<UserFavorite?> GetAsync(Expression<Func<UserFavorite, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(UserFavorite entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(UserFavorite entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int UserFavoriteId)
        {
            return await _repo.DeleteAsync(UserFavoriteId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<UserFavorite, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
