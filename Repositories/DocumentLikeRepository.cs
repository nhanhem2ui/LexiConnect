using BusinessObjects;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class DocumentLikeRepository : IGenericRepository<DocumentLike>
    {
        private readonly IGenericDAO<DocumentLike> _dao;

        public DocumentLikeRepository(IGenericDAO<DocumentLike> dao)
        {
            _dao = dao;
        }

        public IQueryable<DocumentLike> GetAllQueryable(Expression<Func<DocumentLike, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<DocumentLike?> GetAsync(Expression<Func<DocumentLike, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(DocumentLike entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(DocumentLike entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int DocumentLikeId)
        {
            return await _dao.DeleteAsync(DocumentLikeId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<DocumentLike, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
