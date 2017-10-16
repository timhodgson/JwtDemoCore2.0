using Resources.Shared;
using System;

namespace Resources.Service
{
  public interface ICustomerResourceService : IResourceService<EmployeeResource, Guid>
  {
  }

}
