using BusinessObjects;
using System.Linq.Expressions;
using DataAccess;

namespace Repositories
{
    public class PaymentRecordRepository : IGenericRepository<PaymentRecord>
    {
        private readonly IGenericDAO<PaymentRecord> _dao;

        public PaymentRecordRepository(IGenericDAO<PaymentRecord> dao)
        {
            _dao = dao;
        }

        public IQueryable<PaymentRecord> GetAllQueryable(Expression<Func<PaymentRecord, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<PaymentRecord?> GetAsync(Expression<Func<PaymentRecord, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(PaymentRecord entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(PaymentRecord entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int PaymentRecordId)
        {
            return await _dao.DeleteAsync(PaymentRecordId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<PaymentRecord, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
