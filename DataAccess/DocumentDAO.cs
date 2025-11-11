using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace DataAccess
{
    public class DocumentDAO : IGenericDAO<Document>
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DocumentDAO>? _logger;

        public DocumentDAO(AppDbContext db, ILogger<DocumentDAO>? logger = null)
        {
            _db = db;
            _logger = logger;
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
            try
            {
                _db.Documents.Add(entity);
                var result = await _db.SaveChangesAsync() > 0;
                
                if (result)
                {
                    _logger?.LogInformation("Document added successfully. DocumentId: {DocumentId}, Title: {Title}", 
                        entity.DocumentId, entity.Title);
                }
                else
                {
                    _logger?.LogWarning("Document add operation returned false. DocumentId: {DocumentId}, Title: {Title}", 
                        entity.DocumentId, entity.Title);
                }
                
                return result;
            }
            catch (DbUpdateException dbEx)
            {
                _logger?.LogError(dbEx, "Database error while adding document. Title: {Title}, CourseId: {CourseId}, UploaderId: {UploaderId}", 
                    entity.Title, entity.CourseId, entity.UploaderId);
                
                // Log inner exception if exists
                if (dbEx.InnerException != null)
                {
                    _logger?.LogError("Inner exception: {InnerException}", dbEx.InnerException.Message);
                }
                
                throw; // Re-throw to allow caller to handle
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error while adding document. Title: {Title}", entity.Title);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Document entity)
        {
            try
            {
                _db.Documents.Update(entity);
                var result = await _db.SaveChangesAsync() > 0;
                
                if (!result)
                {
                    _logger?.LogWarning("Document update operation returned false. DocumentId: {DocumentId}", entity.DocumentId);
                }
                
                return result;
            }
            catch (DbUpdateException dbEx)
            {
                _logger?.LogError(dbEx, "Database error while updating document. DocumentId: {DocumentId}", entity.DocumentId);
                
                if (dbEx.InnerException != null)
                {
                    _logger?.LogError("Inner exception: {InnerException}", dbEx.InnerException.Message);
                }
                
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error while updating document. DocumentId: {DocumentId}", entity.DocumentId);
                throw;
            }
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