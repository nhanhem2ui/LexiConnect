using BusinessObjects;
using Repositories;
using System.Linq.Expressions;

namespace Services
{
    public class QuizService : IGenericService<Quiz>
    {
        private readonly IGenericRepository<Quiz> _repo;

        public QuizService(IGenericRepository<Quiz> repo)
        {
            _repo = repo;
        }

        public IQueryable<Quiz> GetAllQueryable(Expression<Func<Quiz, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<Quiz?> GetAsync(Expression<Func<Quiz, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(Quiz entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(Quiz entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int QuizId)
        {
            return await _repo.DeleteAsync(QuizId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Quiz, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
