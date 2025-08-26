using BusinessObjects;
using System.Linq.Expressions;
using DataAccess;

namespace Repositories
{
    public class DocumentReviewRepository : IGenericRepository<DocumentReview>
    {
        private readonly IGenericDAO<DocumentReview> _dao;

        public DocumentReviewRepository(IGenericDAO<DocumentReview> dao)
        {
            _dao = dao;
        }

        public IQueryable<DocumentReview> GetAllQueryable(Expression<Func<DocumentReview, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<DocumentReview?> GetAsync(Expression<Func<DocumentReview, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(DocumentReview entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(DocumentReview entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int DocumentReviewId)
        {
            return await _dao.DeleteAsync(DocumentReviewId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<DocumentReview, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
