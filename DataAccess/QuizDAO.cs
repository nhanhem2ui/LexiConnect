using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class QuizDAO : IGenericDAO<Quiz>
    {
        private readonly AppDbContext _db;

        public QuizDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<Quiz> GetAllQueryable(Expression<Func<Quiz, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<Quiz> query = _db.Quizzes;
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

        public async Task<Quiz?> GetAsync(Expression<Func<Quiz, bool>> predicate)
        {
            return await _db.Quizzes
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching Quiz or null if none found
        }

        public async Task<bool> AddAsync(Quiz entity)
        {
            _db.Quizzes.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(Quiz entity)
        {
            _db.Quizzes.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int QuizId)
        {
            var Quiz = await _db.Quizzes.FindAsync(QuizId);
            if (Quiz == null)
            {
                return false; // Quiz not found
            }

            _db.Quizzes.Remove(Quiz);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<Quiz, bool>> predicate)
        {
            return await _db.Quizzes.AnyAsync(predicate);
        }
    }
}