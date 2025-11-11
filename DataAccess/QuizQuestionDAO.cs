using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class QuizQuestionDAO : IGenericDAO<QuizQuestion>
    {
        private readonly AppDbContext _db;

        public QuizQuestionDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<QuizQuestion> GetAllQueryable(Expression<Func<QuizQuestion, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<QuizQuestion> query = _db.QuizQuestions;
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

        public async Task<QuizQuestion?> GetAsync(Expression<Func<QuizQuestion, bool>> predicate)
        {
            return await _db.QuizQuestions
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching QuizQuestion or null if none found
        }

        public async Task<bool> AddAsync(QuizQuestion entity)
        {
            _db.QuizQuestions.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(QuizQuestion entity)
        {
            _db.QuizQuestions.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int QuizQuestionId)
        {
            var QuizQuestion = await _db.QuizQuestions.FindAsync(QuizQuestionId);
            if (QuizQuestion == null)
            {
                return false; // QuizQuestion not found
            }

            _db.QuizQuestions.Remove(QuizQuestion);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<QuizQuestion, bool>> predicate)
        {
            return await _db.QuizQuestions.AnyAsync(predicate);
        }
    }
}