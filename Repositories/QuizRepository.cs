using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class QuizRepository : IGenericRepository<Quiz>
    {
        private readonly IGenericDAO<Quiz> _dao;

        public QuizRepository(IGenericDAO<Quiz> dao)
        {
            _dao = dao;
        }

        public IQueryable<Quiz> GetAllQueryable(Expression<Func<Quiz, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<Quiz?> GetAsync(Expression<Func<Quiz, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(Quiz entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(Quiz entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int QuizId)
        {
            return await _dao.DeleteAsync(QuizId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Quiz, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
