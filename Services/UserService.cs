using BusinessObjects;
using System.Linq.Expressions;

namespace Services
{
    public class UserService : IGenericService<Users>
    {
        private readonly IGenericService<Users> _repo;

        public UserService(IGenericService<Users> repo)
        {
            _repo = repo;
        }

        public IQueryable<Users> GetAllQueryable(Expression<Func<Users, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<Users?> GetAsync(Expression<Func<Users, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(Users entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(Users entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int UsersId)
        {
            return await _repo.DeleteAsync(UsersId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Users, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
