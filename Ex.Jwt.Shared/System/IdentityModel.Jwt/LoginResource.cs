using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace System.IdentityModel.Jwt
{
  public class LoginResource
  {
    public String Email { get; set; }
    public String Password { get; set; }
  }
}
