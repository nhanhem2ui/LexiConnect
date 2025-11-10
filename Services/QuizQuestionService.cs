using BusinessObjects;
using Repositories;
using System.Linq.Expressions;

namespace Services
{
    public class QuizQuestionService : IGenericService<QuizQuestion>
    {
        private readonly IGenericRepository<QuizQuestion> _repo;

        public QuizQuestionService(IGenericRepository<QuizQuestion> repo)
        {
            _repo = repo;
        }

        public IQueryable<QuizQuestion> GetAllQueryable(Expression<Func<QuizQuestion, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<QuizQuestion?> GetAsync(Expression<Func<QuizQuestion, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(QuizQuestion entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(QuizQuestion entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int QuizQuestionId)
        {
            return await _repo.DeleteAsync(QuizQuestionId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<QuizQuestion, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
