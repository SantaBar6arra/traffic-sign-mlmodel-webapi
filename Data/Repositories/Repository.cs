using Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly TrafficSignsContext _context;

        public Repository(TrafficSignsContext context)
        {
            _context = context;
        }

        bool IRepository<T>.Add(T entity)
        {
            try
            {
                _context.Set<T>().Add(entity);
                return true;
            }
            catch
            {
                return false;
            }
        }

        IEnumerable<T> IRepository<T>.GetAll()
        {
            return _context.Set<T>().ToList();
        }
    }
}
