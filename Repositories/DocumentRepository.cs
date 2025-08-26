using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class DocumentRepository : IGenericRepository<Document>
    {
        private readonly IGenericDAO<Document> _dao;

        public DocumentRepository(IGenericDAO<Document> dao)
        {
            _dao = dao;
        }

        public IQueryable<Document> GetAllQueryable(Expression<Func<Document, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<Document?> GetAsync(Expression<Func<Document, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(Document entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(Document entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int DocumentId)
        {
            return await _dao.DeleteAsync(DocumentId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Document, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
