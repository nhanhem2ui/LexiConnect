using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class DocumentTagRepository : IGenericRepository<DocumentTag>
    {
        private readonly IGenericDAO<DocumentTag> _dao;

        public DocumentTagRepository(IGenericDAO<DocumentTag> dao)
        {
            _dao = dao;
        }

        public IQueryable<DocumentTag> GetAllQueryable(Expression<Func<DocumentTag, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<DocumentTag?> GetAsync(Expression<Func<DocumentTag, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(DocumentTag entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(DocumentTag entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int DocumentTagId)
        {
            return await _dao.DeleteAsync(DocumentTagId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<DocumentTag, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
