using Resources.Shared;
using System;
using System.ComponentModel.DataAnnotations;

namespace Resources
{

  public class EmployeeResource : Resource<Guid>
  {
    [Required]
    [StringLength(80)]
    public String FirstName { get; set; }

    [Required]
    [StringLength(80)]
    public String LastName { get; set; }

    [Required]
    [StringLength(128)]
    public String Email { get; set; }

    [StringLength(20)]
    public String Gender { get; set; }

    public Decimal Salary { get; set; }
  }
}
