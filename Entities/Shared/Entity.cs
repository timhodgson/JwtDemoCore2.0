using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Shared
{
  public class Entity
  {
    [StringLength(128)]
    public String CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    [StringLength(128)]
    public String ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    [StringLength(40)]
    public String RowVersion { get; set; }
  }

}
