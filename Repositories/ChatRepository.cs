using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class ChatRepository : IGenericRepository<Chat>
    {
        private readonly IGenericDAO<Chat> _dao;

        public ChatRepository(IGenericDAO<Chat> dao)
        {
            _dao = dao;
        }

        public IQueryable<Chat> GetAllQueryable(Expression<Func<Chat, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<Chat?> GetAsync(Expression<Func<Chat, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(Chat entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(Chat entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int ChatId)
        {
            return await _dao.DeleteAsync(ChatId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Chat, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
