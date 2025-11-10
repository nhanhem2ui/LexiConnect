using BusinessObjects;
using Repositories;
using System.Linq.Expressions;

namespace Services
{
    public class DocumentLikeService : IGenericService<DocumentLike>
    {
        private readonly IGenericRepository<DocumentLike> _repo;

        public DocumentLikeService(IGenericRepository<DocumentLike> repo)
        {
            _repo = repo;
        }

        public IQueryable<DocumentLike> GetAllQueryable(Expression<Func<DocumentLike, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<DocumentLike?> GetAsync(Expression<Func<DocumentLike, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(DocumentLike entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(DocumentLike entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int DocumentLikeId)
        {
            return await _repo.DeleteAsync(DocumentLikeId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<DocumentLike, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
