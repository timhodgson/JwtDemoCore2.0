using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
  public static class GuidExtensions
  {
    public static Boolean IsNullOrEmpty(this Guid guid)
    {
      var result = guid == null || guid == Guid.Empty;

      return result;
    }
  }
}
