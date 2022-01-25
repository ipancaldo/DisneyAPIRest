using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisneyAPI.Interfaces
{
    interface IBaseRepository<TEntity>
        where TEntity : class

    {
        /// <summary>
        /// Get an object by its is
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TEntity Get(int id);

        /// <summary>
        /// Get all the objects
        /// </summary>
        /// <returns></returns>
        List<TEntity> GetAll();

        /// <summary>
        /// Adds an object to de DB
        /// </summary>
        /// <param name="entity"></param>
        void Add(TEntity entity);

        /// <summary>
        /// Edits an object
        /// </summary>
        /// <param name="entity"></param>
        void Put(TEntity entity);

        /// <summary>
        /// Deletes and object
        /// </summary>
        /// <param name="id"></param>
        void Delete(int id);

        /// <summary>
        /// Save the changes
        /// </summary>
        void Save();
    }
}
