using BusinessObjects;
using Repositories;
using System.Linq.Expressions;

namespace Services
{
    public class DocumentTagService : IGenericService<DocumentTag>
    {
        private readonly IGenericRepository<DocumentTag> _repo;

        public DocumentTagService(IGenericRepository<DocumentTag> repo)
        {
            _repo = repo;
        }

        public IQueryable<DocumentTag> GetAllQueryable(Expression<Func<DocumentTag, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<DocumentTag?> GetAsync(Expression<Func<DocumentTag, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(DocumentTag entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(DocumentTag entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int DocumentTagId)
        {
            return await _repo.DeleteAsync(DocumentTagId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<DocumentTag, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
