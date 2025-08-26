using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class SubscriptionPlanRepository : IGenericRepository<SubscriptionPlan>
    {
        private readonly IGenericDAO<SubscriptionPlan> _dao;

        public SubscriptionPlanRepository(IGenericDAO<SubscriptionPlan> dao)
        {
            _dao = dao;
        }

        public IQueryable<SubscriptionPlan> GetAllQueryable(Expression<Func<SubscriptionPlan, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<SubscriptionPlan?> GetAsync(Expression<Func<SubscriptionPlan, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(SubscriptionPlan entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(SubscriptionPlan entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int SubscriptionPlanId)
        {
            return await _dao.DeleteAsync(SubscriptionPlanId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<SubscriptionPlan, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
