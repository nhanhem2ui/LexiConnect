using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class PointTransactionRepository : IGenericRepository<PointTransaction>
    {
        private readonly IGenericDAO<PointTransaction> _dao;

        public PointTransactionRepository(IGenericDAO<PointTransaction> dao)
        {
            _dao = dao;
        }

        public IQueryable<PointTransaction> GetAllQueryable(Expression<Func<PointTransaction, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<PointTransaction?> GetAsync(Expression<Func<PointTransaction, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(PointTransaction entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(PointTransaction entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int PointTransactionId)
        {
            return await _dao.DeleteAsync(PointTransactionId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<PointTransaction, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
