using Entities.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{

  [Table("Countries")]
  public class Country : Entity
  {
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Int32 Id { get; set; }


    [Required]
    [StringLength(2)]
    public String Code2 { get; set; }

    [Required]
    [StringLength(3)]
    public String Code3 { get; set; }


    [Required]
    [StringLength(50)]
    public String Name { get; set; }

  }
}
