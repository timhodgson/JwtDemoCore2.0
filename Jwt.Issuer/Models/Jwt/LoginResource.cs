using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jwt.Issuer.Models
{
  public class LoginResource
  {
    public String Email { get; set; }
    public String Password { get; set; }
  }
}
