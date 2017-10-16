using AutoMapper;
using Entities;
using Entities.Mappings;
using Resources.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Resources.Service
{
  public class EmployeeResourceService : ICustomerResourceService, IDisposable
  {
    private readonly IMapper mapper;
    protected EntityContext EntityContext { get; private set; }

    public EmployeeResourceService(EntityContext entityContext)
    {
      EntityContext = entityContext;

      // Setup AutoMapper between Resource and Entity
      var config = new AutoMapper.MapperConfiguration(cfg =>
      {
        cfg.AddProfiles(typeof(EmployeeMapping).GetTypeInfo().Assembly);
      });

      mapper = config.CreateMapper();
    }


    public EmployeeResource Create()
    {
      var result = new EmployeeResource()
      {
        Id = Guid.NewGuid()
      };

      return result;
    }

  protected virtual void BeautifyResource(EmployeeResource resource)
  {
  }


  /// <summary>
  ///  Perform basic validation
  /// </summary>
  /// <param name="resource"></param>
  /// <param name="errors"></param>
  protected void ValidateAttributes(EmployeeResource resource, IList<ValidationError> errors)
  {
    var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(resource);
    var validationResults = new List<ValidationResult>(); ;

    Validator.TryValidateObject(resource, validationContext, validationResults, true);

    foreach (var item in validationResults)
      errors.Add(new ValidationError(item.ErrorMessage, item.MemberNames?.FirstOrDefault() ?? ""));
  }


  protected virtual void ValidateBusinessRules(EmployeeResource resource, IList<ValidationError> errors)
  {
  }


  protected virtual void ValidateDelete(EmployeeResource resource, IList<ValidationError> errors)
  {
  }


  public async Task<EmployeeResource> FindAsync(Guid id)
  {
    // Fetch entity from storage
    var entity = await EntityContext.FindAsync<Employee>(id);

    // Convert emtity to resource
    var result = mapper.Map<EmployeeResource>(entity);

    return result;
  }


  public IQueryable<EmployeeResource> Items()
  {
    var entities = Enumerable.AsEnumerable(EntityContext.Employees);
    var result = mapper.Map<IEnumerable<Employee>, IEnumerable<EmployeeResource>>(entities);

    return result.AsQueryable();
  }


  private IEnumerable<String> CreateFieldNames(IQueryable items, String searchFields = "")
  {
    IEnumerable<String> fieldNames = searchFields.Split(new Char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

    IEnumerable<String> propertyNames = items.ElementType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToList();

    // Use only valid field names
    IEnumerable<String> result = fieldNames.Where(n => propertyNames.Contains(n)).ToList();

    return result;
  }


  // needs System.Linq.Dynamic.Core
  private IQueryable SearchItems(IQueryable items, String sortBy, String sortDirection, Int32 skip, Int32 take, String search, String searchFields)
  {
    IEnumerable<String> propertyNames = items.ElementType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToList();

    // Apply filtering to all visible column names
    if (!String.IsNullOrEmpty(search))
    {
      // Use only valid fieldnames
      IEnumerable<String> fieldNames = CreateFieldNames(items, searchFields);

      StringBuilder sb = new StringBuilder();

      // create dynamic Linq expression
      foreach (String fieldName in fieldNames)
        sb.AppendFormat("({0} == null ? false : {0}.ToString().IndexOf(@0, @1) >=0) or {1}", fieldName, Environment.NewLine);

      String searchExpression = sb.ToString();
      // remove last "or" occurrence
      searchExpression = searchExpression.Substring(0, searchExpression.LastIndexOf("or"));

      // Apply filtering
      items = items.Where(searchExpression, search, StringComparison.OrdinalIgnoreCase);
    }

    // Skip requires sorting, so make sure there is always sorting
    String sortExpression = "";

    if (propertyNames.Any(c => c == sortBy))
    {
      sortExpression += String.Format("{0} {1}", sortBy, sortDirection);
      items = items.OrderBy(sortExpression);
    }

    // show 100 records if limit is not set
    if (take == 0)
      take = 100;

    items = items.Skip(skip).Take(take);

    return items;
  }


  public LoadResult<EmployeeResource> Load(String sortBy, String sortDirection, Int32 skip, Int32 take, String search, String searchFields)
  {
    IQueryable entities = EntityContext.Employees.AsQueryable();

    // where clause is set, count all records
    Int32 count = entities.Count();

    // Perform filtering, ordering and paging
    entities = SearchItems(entities, sortBy, sortDirection, skip, take, search, searchFields);

    // Prepare result
    var result = new LoadResult<EmployeeResource>()
    {
      CountUnfiltered = count,
      Items = mapper.Map<IList<Employee>, IList<EmployeeResource>>(entities.ToDynamicList<Employee>())
    };

    return result;
  }


  public async Task<ResourceResult<EmployeeResource>> InsertAsync(EmployeeResource resource)
  {
    // Fields are set by persistance service 
    resource.CreatedBy = null;

    resource.ModifiedAt = null;
    resource.ModifiedBy = null;

    resource.RowVersion = null;

    return await UpsertAsync(resource);
  }


  public async Task<ResourceResult<EmployeeResource>> UpdateAsync(EmployeeResource resource)
  {
    return await UpsertAsync(resource);
  }


  public async Task<ResourceResult<EmployeeResource>> UpsertAsync(EmployeeResource resource)
  {
    var result = new ResourceResult<EmployeeResource>();

    // Beautify before validation and make validation more succesfull
    BeautifyResource(resource);

    // save beautify effect effect 
    result.Resource = resource;

    // Apply simple validation on attribute level
    ValidateAttributes(resource, result.Errors);

    // Apply complex business rules validation
    ValidateBusinessRules(resource, result.Errors);

    // Save is only usefull when error free
    if (result.Errors.Count == 0)
    {
      // Convert resource to entity
      var entity = mapper.Map<Employee>(resource);

      // save entity
      await EntityContext.UpsertAsync(entity);

      // convert save result back to resource and get database created values like auto incremental field and timestamps.
      result.Resource = mapper.Map<EmployeeResource>(entity);
    }

    return result;
  }


  public async Task<ResourceResult<EmployeeResource>> DeleteAsync(Guid id)
  {
    var result = new ResourceResult<EmployeeResource>();

    // Check if resource still exists
    result.Resource = await FindAsync(id);

    if (result.Resource != null)
    {
      // Check if delete is allowed 
      ValidateDelete(result.Resource, result.Errors);

      // Delete only if allowed
      if (result.Errors.Count == 0)
      {
        var entity = mapper.Map<Employee>(result.Resource);

        await EntityContext.DeleteAsync(entity);
      }
    }

    return result;
  }

  #region IDisposable Support
  protected virtual void Dispose(Boolean isDisposing)
  {
    if (isDisposing && EntityContext != null)
    {
      EntityContext.Dispose();
      EntityContext = null;
    }
  }

  public void Dispose()
  {
    Dispose(true);
  }
  #endregion
}
}
