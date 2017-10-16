using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
  public class EmployeeModel
  {
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(80)]
    public String FirstName { get; set; }

    [Required]
    [StringLength(80)]
    public String LastName { get; set; }

    [Required]
    [StringLength(128)]
    [DataType(DataType.EmailAddress)]
    public String Email { get; set; }

    [StringLength(20)]
    public String Gender { get; set; }

    public Decimal Salary { get; set; }

    public String RowVersion { get; set; }
  }
}
