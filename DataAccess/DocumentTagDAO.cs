using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class DocumentTagDAO : IGenericDAO<DocumentTag>
    {
        private readonly AppDbContext _db;

        public DocumentTagDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<DocumentTag> GetAllQueryable(Expression<Func<DocumentTag, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<DocumentTag> query = _db.DocumentTags;
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

        public async Task<DocumentTag?> GetAsync(Expression<Func<DocumentTag, bool>> predicate)
        {
            return await _db.DocumentTags
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching DocumentTag or null if none found
        }

        public async Task<bool> AddAsync(DocumentTag entity)
        {
            _db.DocumentTags.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(DocumentTag entity)
        {
            _db.DocumentTags.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int DocumentTagId)
        {
            var DocumentTag = await _db.DocumentTags.FindAsync(DocumentTagId);
            if (DocumentTag == null)
            {
                return false; // DocumentTag not found
            }

            _db.DocumentTags.Remove(DocumentTag);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<DocumentTag, bool>> predicate)
        {
            return await _db.DocumentTags.AnyAsync(predicate);
        }
    }
}