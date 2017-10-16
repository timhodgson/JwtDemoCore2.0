using System;

namespace Resources.Shared
{
  public class Resource<TKey> where TKey : IEquatable<TKey>
  {
    virtual public TKey Id { get; set; }

    public String CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }
    public String ModifiedBy { get; set; }

    public String RowVersion { get; set; }
  }
}
