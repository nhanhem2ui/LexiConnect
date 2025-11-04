using BusinessObjects;
using System.Linq.Expressions;

namespace Services
{
    public class PaymentRecordService : IGenericService<PaymentRecord>
    {
        private readonly IGenericService<PaymentRecord> _repo;

        public PaymentRecordService(IGenericService<PaymentRecord> repo)
        {
            _repo = repo;
        }

        public IQueryable<PaymentRecord> GetAllQueryable(Expression<Func<PaymentRecord, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<PaymentRecord?> GetAsync(Expression<Func<PaymentRecord, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(PaymentRecord entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(PaymentRecord entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int PaymentRecordId)
        {
            return await _repo.DeleteAsync(PaymentRecordId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<PaymentRecord, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
