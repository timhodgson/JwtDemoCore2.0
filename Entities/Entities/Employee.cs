using Entities.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{
  [Table("Employees")]
  public class Employee : Entity
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
    public String Email { get; set; }
        
    [StringLength(20)]
    public String Gender { get; set; }

    public Decimal Salary { get; set; }
  }
}
