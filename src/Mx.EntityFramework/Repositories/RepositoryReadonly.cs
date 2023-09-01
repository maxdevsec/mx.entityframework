using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Mx.EntityFramework.Contracts;
using Mx.Library.ExceptionHandling;


namespace Mx.EntityFramework.Repositories;

	public abstract class RepositoryReadOnly<TEntity> : IRepositoryReadOnly<TEntity> where TEntity : class
	{
		private readonly IEntityContext _entityContext;

		protected RepositoryReadOnly(IEntityContext entityContext)
		{
			_entityContext = entityContext;
		}



		protected DbContext Context => _entityContext.Context;

		public virtual IQueryable<TEntity> GetAll()
		{
			return Context.Set<TEntity>();
		}
        

        public virtual void Reload(TEntity entity)
        {
            Context.Entry<TEntity>(entity).Reload();
        }

        public virtual TEntity? FindSingle(Expression<Func<TEntity, bool>> predicate)
        {
            var entities = FindBy(predicate).ToList();

            AssertSingle(entities);

            return entities.FirstOrDefault();
        }

        public virtual async Task<TEntity?> FindSingleAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var entities = await FindByAsync(predicate);

            AssertSingle(entities);

            return entities.FirstOrDefault();
        }

        private static void AssertSingle(IList<TEntity> entities)
        {
	        if(!entities.Any())
	        {
		        throw new MxNotFoundException($"{(typeof(TEntity).Name)} was not found matching the filter criteria");
	        }
            if (entities.Count > 1)
            {
                throw new MxMultipleFoundException($"More than one {(typeof(TEntity).Name)} was found matching the filter criteria");
            }

        }


        public virtual IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate)
		{
			return Context.Set<TEntity>().Where(predicate);
		}

        public virtual async Task<IList<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Context.Set<TEntity>().Where(predicate).ToListAsync();
        }



        /// <summary>
        /// Generates a comma separated string containing the query parameter names
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected string GenerateCommaSeparatedParameterNames(ICollection<SqlParameter> parameters)
		{
			return string.Join(",", parameters.Select(parameter => parameter.ParameterName).ToList());
		}
	}