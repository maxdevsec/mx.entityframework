using Microsoft.EntityFrameworkCore;
using Mx.EntityFramework.Contracts;
using Mx.Library.ExceptionHandling;


namespace Mx.EntityFramework.Repositories;

public abstract class Repository<TEntity> : RepositoryReadOnly<TEntity>, IRepository<TEntity> where TEntity : class
{
		
    protected Repository(IEntityContext entityContext) : base(entityContext){}

    public virtual void Insert(TEntity entity)
    {
        Context.Set<TEntity>().Add(entity);

    }


    public virtual void Update(TEntity entity)
    {
        if (!Exists(entity))
        {
            Context.Set<TEntity>().Attach(entity);
        }
        Context.Entry(entity).State = EntityState.Modified;

    }

    public virtual void Delete(TEntity entity)
    {
        if (!Exists(entity))
        {
            Context.Set<TEntity>().Attach(entity);
        }

        Context.Set<TEntity>().Remove(entity);

    }


    public virtual void SaveChanges()
    {
        try
        {
            Context.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            if (IsReferentialIntegrationException(e))
            {
                ThrowReferentialIntegrityException(e);
            }
            else
            {
                throw;
            }
        }		
         
    }
    public virtual async Task SaveChangesAsync()
    {
        try
        {
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException e)
        {
            if (IsReferentialIntegrationException(e))
            {
                ThrowReferentialIntegrityException(e);
            }
            else
            {
                throw;
            }
        }
        
    }

    private bool Exists(TEntity entity) 
    {
        return Context.Set<TEntity>().Local.Any(attachedEntity => attachedEntity == entity);
    }

    private bool IsReferentialIntegrationException(Exception e)
    {
        return e.InnerException != null && e.InnerException.Message.Contains("REFERENCE");
    }

    private void ThrowReferentialIntegrityException(DbUpdateException e)
    {
        throw new MxReferentialIntegrityException("Unable to save the database changes due to a referential integrity constraint violation",  e.InnerException);
    }

}