using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace System.Security.Principal
{
  public class IdentityResolverService : IIdentity
  {
    private readonly IHttpContextAccessor Context;

    public IdentityResolverService(IHttpContextAccessor context)
    {
      Context = context;
    }

    public String AuthenticationType => Context?.HttpContext.User?.Identity.AuthenticationType ?? null;

    public Boolean IsAuthenticated => Context?.HttpContext.User?.Identity.IsAuthenticated ?? false;

    public String Name => Context?.HttpContext.User.FindFirst(c => c.Type.ContainsEx("Email"))?.Value; 
  }
}