using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class UsersRepository : IGenericRepository<Users>
    {
        private readonly IGenericDAO<Users> _dao;

        public UsersRepository(IGenericDAO<Users> dao)
        {
            _dao = dao;
        }

        public IQueryable<Users> GetAllQueryable(Expression<Func<Users, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<Users?> GetAsync(Expression<Func<Users, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(Users entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(Users entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int UsersId)
        {
            return await _dao.DeleteAsync(UsersId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Users, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
