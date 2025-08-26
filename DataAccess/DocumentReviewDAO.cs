using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class DocumentReviewDAO : IGenericDAO<DocumentReview>
    {
        private readonly AppDbContext _db;

        public DocumentReviewDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<DocumentReview> GetAllQueryable(Expression<Func<DocumentReview, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<DocumentReview> query = _db.DocumentReviews;
            if (asNoTracking)
            {
                query = query.AsNoTracking(); // Use AsNoTracking for read-only queries
            }
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            return query;
        }

        public async Task<DocumentReview?> GetAsync(Expression<Func<DocumentReview, bool>> predicate)
        {
            return await _db.DocumentReviews
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching DocumentReview or null if none found
        }

        public async Task<bool> AddAsync(DocumentReview entity)
        {
            _db.DocumentReviews.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(DocumentReview entity)
        {
            _db.DocumentReviews.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int DocumentReviewId)
        {
            var DocumentReview = await _db.DocumentReviews.FindAsync(DocumentReviewId);
            if (DocumentReview == null)
            {
                return false; // DocumentReview not found
            }

            _db.DocumentReviews.Remove(DocumentReview);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<DocumentReview, bool>> predicate)
        {
            return await _db.DocumentReviews.AnyAsync(predicate);
        }
    }
}
