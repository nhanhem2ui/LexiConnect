using BusinessObjects;
using System.Linq.Expressions;

namespace Services
{
    public class SubscriptionPlanService : IGenericService<SubscriptionPlan>
    {
        private readonly IGenericService<SubscriptionPlan> _repo;

        public SubscriptionPlanService(IGenericService<SubscriptionPlan> repo)
        {
            _repo = repo;
        }

        public IQueryable<SubscriptionPlan> GetAllQueryable(Expression<Func<SubscriptionPlan, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<SubscriptionPlan?> GetAsync(Expression<Func<SubscriptionPlan, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(SubscriptionPlan entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(SubscriptionPlan entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int SubscriptionPlanId)
        {
            return await _repo.DeleteAsync(SubscriptionPlanId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<SubscriptionPlan, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
