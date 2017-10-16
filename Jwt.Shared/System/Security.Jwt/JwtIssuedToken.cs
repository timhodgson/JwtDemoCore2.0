using System;

namespace System.Security.Jwt
{
  public class JwtIssuedToken
  {
    public String AccessToken { get; set; }
    public Int32 ValidFor { get; set; }
    public DateTime ExpiresUtc { get; set; }
  }
}