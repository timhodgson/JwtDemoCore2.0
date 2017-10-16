using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Resources.Shared
{
  public class ValidationError
  {
    public String Message { get; set; }
    public String MemberName { get; set; }

    public ValidationError()
    {
    }

    public ValidationError(String message) : this(message, null)
    {
    }

    public ValidationError(String message, String memberName)
    {
      Message = message;
      MemberName = memberName;
    }
  }


  public class ResourceResult<TResource> where TResource : class
  {
    public TResource Resource { get; set; }
    public IList<ValidationError> Errors { get; private set; }
    public IList<String> Exceptions { get; private set; }

    public ResourceResult() : this(null)
    {
    }

    public ResourceResult(TResource resource)
    {
      Resource = resource;
      Errors = new List<ValidationError>();
      Exceptions = new List<String>();
    }
  }
}
