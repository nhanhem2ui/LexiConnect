using BusinessObjects;
using System.Linq.Expressions;

namespace Services
{
    public class DocumentService : IGenericService<Document>
    {
        private readonly IGenericService<Document> _repo;

        public DocumentService(IGenericService<Document> repo)
        {
            _repo = repo;
        }

        public IQueryable<Document> GetAllQueryable(Expression<Func<Document, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<Document?> GetAsync(Expression<Func<Document, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(Document entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(Document entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int DocumentId)
        {
            return await _repo.DeleteAsync(DocumentId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Document, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
