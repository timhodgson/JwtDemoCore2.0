using AutoMapper;
using Models;

namespace Models.Mappings
{
  public class EmployeeMapping : Profile
  {
    public EmployeeMapping()
    {
      // 2 way mapping resource <==> ViewModel
      CreateMap<Resources.EmployeeResource, EmployeeModel>();
      CreateMap<EmployeeModel, Resources.EmployeeResource>();
    }
  }
}
