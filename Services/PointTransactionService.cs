using BusinessObjects;
using Repositories;
using System.Linq.Expressions;

namespace Services
{
    public class PointTransactionService : IGenericService<PointTransaction>
    {
        private readonly IGenericRepository<PointTransaction> _repo;

        public PointTransactionService(IGenericRepository<PointTransaction> repo)
        {
            _repo = repo;
        }

        public IQueryable<PointTransaction> GetAllQueryable(Expression<Func<PointTransaction, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<PointTransaction?> GetAsync(Expression<Func<PointTransaction, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(PointTransaction entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(PointTransaction entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int PointTransactionId)
        {
            return await _repo.DeleteAsync(PointTransactionId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<PointTransaction, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
