using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class DocumentDAO : IGenericDAO<Document>
    {
        private readonly AppDbContext _db;

        public DocumentDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<Document> GetAllQueryable(Expression<Func<Document, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<Document> query = _db.Documents;
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

        public async Task<Document?> GetAsync(Expression<Func<Document, bool>> predicate)
        {
            return await _db.Documents
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching Document or null if none found
        }

        public async Task<bool> AddAsync(Document entity)
        {
            _db.Documents.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(Document entity)
        {
            _db.Documents.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int DocumentId)
        {
            var Document = await _db.Documents.FindAsync(DocumentId);
            if (Document == null)
            {
                return false; // Document not found
            }

            _db.Documents.Remove(Document);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<Document, bool>> predicate)
        {
            return await _db.Documents.AnyAsync(predicate);
        }
    }
}