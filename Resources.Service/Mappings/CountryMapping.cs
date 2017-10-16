using AutoMapper;
using System;

namespace Entities.Mappings
{
  public class EmployeeMapping : Profile
  {
    public EmployeeMapping()
    {
      // 2 way mapping resource <==> entity model
      CreateMap<Resources.EmployeeResource, Employee>();
      CreateMap<Employee, Resources.EmployeeResource>();
    }
  }
}
