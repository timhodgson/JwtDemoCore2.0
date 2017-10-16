using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resources.Shared
{

  public class LoadResult<TResource> where TResource : class
  {
    public Int32 CountUnfiltered { get; set; }
    public IList<TResource> Items { get; set; }
  }

  public interface IResourceService<TResource, TKey> where TResource : Resource<TKey> where TKey : IEquatable<TKey>
  {
    TResource Create();

    Task<TResource> FindAsync(TKey id);

    IQueryable<TResource> Items();

    LoadResult<TResource> Load(String sortBy, String sortDirection, Int32 skip, Int32 take, String search, String searchFields);

    Task<ResourceResult<TResource>> InsertAsync(TResource resource);
    Task<ResourceResult<TResource>> UpdateAsync(TResource resource);
    Task<ResourceResult<TResource>> DeleteAsync(TKey id);
  }
}
