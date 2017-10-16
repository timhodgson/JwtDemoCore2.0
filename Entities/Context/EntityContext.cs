using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Entities.Shared;
using System.Threading;
using System.Security.Principal;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Entities
{
  public partial class EntityContext : DbContext
  {
    protected readonly IIdentity Identity;

    //public DbSet<Country> Countries { get; set; }
    public DbSet<Employee> Employees{ get; set; }

    public EntityContext(DbContextOptions<EntityContext> options, IIdentity identity) : base(options)
    {
      Identity = identity;

      ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

      // Check if database exists, if created add content
      if (Database.EnsureCreated())
        InitDatabaseContent();
    }

    public void InitDatabaseContent()
    {
      // Extract Employee Json file from embedded resource
      var assembly = GetType().GetTypeInfo().Assembly;

      var fileName = assembly.GetManifestResourceNames().FirstOrDefault();

      using (var resourceStream = assembly.GetManifestResourceStream(fileName))
      {
        using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
        {
          var document = reader.ReadToEnd();

          var items = JsonConvert.DeserializeObject<List<Employee>>(document);
          
          foreach (var item in items)
          {
            Add(item);
            Entry(item).State = EntityState.Added;

            var ra = SaveChanges();

            Entry(item).State = EntityState.Detached;
          }
        }
      }
    }

    public async Task<TEntity> UpsertAsync<TEntity>(TEntity entity) where TEntity : Entity
    {
      // transaction is required for concurrency check
      using (var transaction = Database.BeginTransaction())
      {
        try
        {
          // Detect Insert or Update
          var entityState = String.IsNullOrEmpty(entity.RowVersion) ? EntityState.Added : EntityState.Modified;

          // Check for concurrency error before update
          if (entityState == EntityState.Modified)
          {
            var keyValues = GetKeyValues(entity);

            // Find existing entity based on keyvale(s)
            var existingEntity = await FindAsync<TEntity>(keyValues);

            var existingRowVersion = existingEntity?.RowVersion ?? null;

            // If the rowversion does not match with the entity
            // the entity is updated by an other user or process and concurrency error has occured
            if (existingRowVersion != entity.RowVersion)
              throw new ConcurrencyException("Concurrency Error");
          }

          if (entityState == EntityState.Added)
            Add(entity);
          else
            Attach(entity);

          Entry(entity).State = entityState;

          var ra = await SaveChangesAsync();

          Database.CommitTransaction();
        }
        catch (Exception ex)
        {
          Database.RollbackTransaction();

          throw ex;
        }

        return entity;
      }
    }

    public async Task DeleteAsync<TEntity>(TEntity entity) where TEntity : Entity
    {
      Attach(entity);

      Entry(entity).State = EntityState.Deleted;

      var ra = await SaveChangesAsync();
    }
    
    public override Int32 SaveChanges(Boolean acceptAllChangesOnSuccess)
    {
      AddAuditInfo();

      var result = base.SaveChanges(acceptAllChangesOnSuccess);

      return result;
    }

    public override Task<Int32> SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
    {
      AddAuditInfo();

      return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void AddAuditInfo()
    {
      // process all pending items
      foreach (var entry in this.ChangeTracker.Entries())
      {
        var entity = entry.Entity as Entity;

        var currentUserName = Identity?.Name ?? "Unknown";

        if (entity != null)
        {
          // Create new row version for concurrency support
          entity.RowVersion = Guid.NewGuid().ToString();

          // set audit info new entity
          if (Entry(entity).State == EntityState.Added)
          {
            entity.CreatedBy = currentUserName;
            entity.CreatedAt = DateTime.UtcNow;
          }

          // set audit info existing entity
          if (Entry(entity).State == EntityState.Modified)
          {
            entity.ModifiedBy = currentUserName;
            entity.ModifiedAt = DateTime.UtcNow;
          }
        }
      }
    }

    private Object[] GetKeyValues<TEntity>(TEntity entity) where TEntity : Entity
    {
      var KeyProperties = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.IsDefined(typeof(KeyAttribute), true));

      var result = KeyProperties.Select(p => p.GetValue(entity, null)).ToArray();

      return result;
    }
  }
}
