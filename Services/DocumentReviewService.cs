using BusinessObjects;
using Repositories;
using System.Linq.Expressions;

namespace Services
{
    public class DocumentReviewService : IGenericService<DocumentReview>
    {
        private readonly IGenericRepository<DocumentReview> _repo;

        public DocumentReviewService(IGenericRepository<DocumentReview> repo)
        {
            _repo = repo;
        }

        public IQueryable<DocumentReview> GetAllQueryable(Expression<Func<DocumentReview, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<DocumentReview?> GetAsync(Expression<Func<DocumentReview, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(DocumentReview entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(DocumentReview entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int DocumentReviewId)
        {
            return await _repo.DeleteAsync(DocumentReviewId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<DocumentReview, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
