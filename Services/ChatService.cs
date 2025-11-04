using BusinessObjects;
using System.Linq.Expressions;

namespace Services
{
    public class ChatService : IGenericService<Chat>
    {
        private readonly IGenericService<Chat> _repo;

        public ChatService(IGenericService<Chat> repo)
        {
            _repo = repo;
        }

        public IQueryable<Chat> GetAllQueryable(Expression<Func<Chat, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<Chat?> GetAsync(Expression<Func<Chat, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(Chat entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(Chat entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int ChatId)
        {
            return await _repo.DeleteAsync(ChatId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Chat, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
