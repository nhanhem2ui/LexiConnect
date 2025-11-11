using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace DataAccess
{
    public class RecentViewedDAO : IGenericDAO<RecentViewed>
    {

        private readonly AppDbContext _db;
        private readonly ILogger<RecentViewedDAO>? _logger;

        public RecentViewedDAO(AppDbContext db, ILogger<RecentViewedDAO>? logger = null)
        {
            _db = db;
            _logger = logger;
        }

        public IQueryable<RecentViewed> GetAllQueryable(Expression<Func<RecentViewed, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<RecentViewed> query = _db.RecentVieweds;
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

        public async Task<RecentViewed?> GetAsync(Expression<Func<RecentViewed, bool>> predicate)
        {
            return await _db.RecentVieweds
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching RecentViewed or null if none found
        }

        public async Task<bool> AddAsync(RecentViewed entity)
        {
            try
            {
                // Validate required fields before saving
                if (entity.CourseId == null || entity.CourseId == 0)
                {
                    _logger?.LogError("Cannot add RecentViewed: CourseId is null or zero. DocumentId: {DocumentId}, UserId: {UserId}", 
                        entity.DocumentId, entity.UserId);
                    throw new InvalidOperationException("CourseId is required for RecentViewed. Cannot be null or zero.");
                }

                if (entity.DocumentId == null || entity.DocumentId == 0)
                {
                    _logger?.LogError("Cannot add RecentViewed: DocumentId is null or zero. UserId: {UserId}", 
                        entity.UserId);
                    throw new InvalidOperationException("DocumentId is required for RecentViewed. Cannot be null or zero.");
                }

                _db.RecentVieweds.Add(entity);
                var result = await _db.SaveChangesAsync() > 0;
                
                if (result)
                {
                    _logger?.LogInformation("RecentViewed added successfully. Id: {Id}, DocumentId: {DocumentId}, CourseId: {CourseId}, UserId: {UserId}", 
                        entity.Id, entity.DocumentId, entity.CourseId, entity.UserId);
                }
                
                return result;
            }
            catch (DbUpdateException dbEx)
            {
                _logger?.LogError(dbEx, "Database error while adding RecentViewed. DocumentId: {DocumentId}, CourseId: {CourseId}, UserId: {UserId}", 
                    entity.DocumentId, entity.CourseId, entity.UserId);
                
                if (dbEx.InnerException != null)
                {
                    _logger?.LogError("Inner exception: {InnerException}", dbEx.InnerException.Message);
                }
                
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error while adding RecentViewed. DocumentId: {DocumentId}", entity.DocumentId);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(RecentViewed entity)
        {
            try
            {
                // Validate required fields before updating
                if (entity.CourseId == null || entity.CourseId == 0)
                {
                    _logger?.LogError("Cannot update RecentViewed: CourseId is null or zero. Id: {Id}, DocumentId: {DocumentId}", 
                        entity.Id, entity.DocumentId);
                    throw new InvalidOperationException("CourseId is required for RecentViewed. Cannot be null or zero.");
                }

                _db.RecentVieweds.Update(entity);
                var result = await _db.SaveChangesAsync() > 0;
                
                return result;
            }
            catch (DbUpdateException dbEx)
            {
                _logger?.LogError(dbEx, "Database error while updating RecentViewed. Id: {Id}", entity.Id);
                
                if (dbEx.InnerException != null)
                {
                    _logger?.LogError("Inner exception: {InnerException}", dbEx.InnerException.Message);
                }
                
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error while updating RecentViewed. Id: {Id}", entity.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int RecentViewedId)
        {
            var RecentViewed = await _db.RecentVieweds.FindAsync(RecentViewedId);
            if (RecentViewed == null)
            {
                return false; // RecentViewed not found
            }

            _db.RecentVieweds.Remove(RecentViewed);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<RecentViewed, bool>> predicate)
        {
            return await _db.RecentVieweds.AnyAsync(predicate);
        }
    }
}