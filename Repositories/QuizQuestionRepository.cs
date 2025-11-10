using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class QuizQuestionRepository : IGenericRepository<QuizQuestion>
    {
        private readonly IGenericDAO<QuizQuestion> _dao;

        public QuizQuestionRepository(IGenericDAO<QuizQuestion> dao)
        {
            _dao = dao;
        }

        public IQueryable<QuizQuestion> GetAllQueryable(Expression<Func<QuizQuestion, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<QuizQuestion?> GetAsync(Expression<Func<QuizQuestion, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(QuizQuestion entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(QuizQuestion entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int QuizQuestionId)
        {
            return await _dao.DeleteAsync(QuizQuestionId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<QuizQuestion, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
