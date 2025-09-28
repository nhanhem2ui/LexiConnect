using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    
        public class DocumentLikeDAO : IGenericDAO<DocumentLike>
        {
            private readonly AppDbContext _db;

            public DocumentLikeDAO(AppDbContext db)
            {
                _db = db;
            }

            public IQueryable<DocumentLike> GetAllQueryable(Expression<Func<DocumentLike, bool>>? predicate = null, bool asNoTracking = true)
            {
                IQueryable<DocumentLike> query = _db.DocumentLikes;
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

            public async Task<DocumentLike?> GetAsync(Expression<Func<DocumentLike, bool>> predicate)
            {
                return await _db.DocumentLikes
                    .AsNoTracking() // Use AsNoTracking for read-only queries
                    .FirstOrDefaultAsync(predicate); // Return the first matching DocumentLike or null if none found
            }

            public async Task<bool> AddAsync(DocumentLike entity)
            {
                _db.DocumentLikes.Add(entity);
                return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
            }

            public async Task<bool> UpdateAsync(DocumentLike entity)
            {
                _db.DocumentLikes.Update(entity);
                return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
            }

            public async Task<bool> DeleteAsync(int DocumentLikeId)
            {
                var DocumentLike = await _db.DocumentLikes.FindAsync(DocumentLikeId);
                if (DocumentLike == null)
                {
                    return false; // DocumentLike not found
                }

                _db.DocumentLikes.Remove(DocumentLike);
                return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
            }

            public async Task<bool> ExistsAsync(Expression<Func<DocumentLike, bool>> predicate)
            {
                return await _db.DocumentLikes.AnyAsync(predicate);
            }
        }
    }

