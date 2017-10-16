using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Config;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace System.Security.Claims
{
  public interface IClaimPrincipalManager
  {
    String UserName { get; }
    Boolean IsAuthenticated { get; }

    ClaimsPrincipal User { get; }

    Task<Boolean> LoginAsync(String email, String password);
    Task LogoutAsync();
    Task RenewTokenAsync(String jwtToken);

    Task<Boolean> HasPolicy(String policyName);
  }


  public class ClaimPrincipalManager : IClaimPrincipalManager
  {
    private readonly HttpContext httpContext;
    private readonly IAuthorizationService authorizationService;
    private readonly IJwtTokenValidationSettings jwtTokenValidationSettings;
    private readonly IJwtTokenIssuerSettings jwtTokenIssuerSettings;
    private readonly IAuthenticationSettings authenticationSettings;

    public Boolean IsAuthenticated
    {
      get
      {
        return User.Identities.Any(u => u.IsAuthenticated);
      }
    }
      
      //=> User.Identities.Any(u => u.IsAuthenticated);
    public String UserName
    {
      get
      {
        return User.Identities.FirstOrDefault(u => u.IsAuthenticated)?.FindFirst(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
      }
    }

      //=> User.Identities.FirstOrDefault(u => u.IsAuthenticated)?.FindFirst(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
    public ClaimsPrincipal User => httpContext?.User;

    public ClaimPrincipalManager(IHttpContextAccessor httpContextAccessor,
                                 IAuthorizationService authorizationService,
                                 IJwtTokenValidationSettings jwtTokenValidationSettings,
                                 IJwtTokenIssuerSettings jwtTokenIssuerSettings,
                                 IAuthenticationSettings authenticationSettings)
    {
      this.httpContext = httpContextAccessor.HttpContext;
      this.authorizationService = authorizationService;
      this.jwtTokenValidationSettings = jwtTokenValidationSettings;
      this.jwtTokenIssuerSettings = jwtTokenIssuerSettings;
      this.authenticationSettings = authenticationSettings;
    }


    public async Task LogoutAsync()
    {
      await httpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
    }


    private HttpClient CreateClient()
    {
      var url = new Uri(jwtTokenIssuerSettings.BaseAddress);

      var result = new HttpClient() { BaseAddress = url };

      result.DefaultRequestHeaders.Accept.Clear();
      result.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

      return result;
    }


    private async Task<String> FetchJwtToken(String email, String password)
    {
      var apiUrl = jwtTokenIssuerSettings.Login;

      using (var client = CreateClient())
      {
        var resource = new
        {
          Email = email,
          Password = password
        };
        using (var content = new StringContent(JsonConvert.SerializeObject(resource), Encoding.UTF8, "application/json"))
        {
          using (var response = await client.PostAsync(apiUrl, content))
          {
            var result = response.StatusCode == HttpStatusCode.OK ? await response.Content.ReadAsStringAsync() : String.Empty;

            return result;
          }
        }
      }
    }


    public async Task<Boolean> LoginAsync(String email, String password)
    {
      // Fetch token from JWT issuer
      var jwtToken = await FetchJwtToken(email, password);

      return await Login(jwtToken);
    }


    private async Task<Boolean> Login(String jwtToken)
    {
      try
      {
        // No use if token is empty
        if (jwtToken.IsNullOrEmpty())
          return false;

        // Logout first
        await LogoutAsync();

        // Setup handler for processing Jwt token
        var tokenHandler = new JwtSecurityTokenHandler();

        var settings = jwtTokenValidationSettings.CreateTokenValidationParameters();

        // Retrieve principal from Jwt token
        var principal = tokenHandler.ValidateToken(jwtToken, settings, out var validatedToken);

        // Cast needed for accessing claims property
        var identity = principal.Identity as ClaimsIdentity;

        // parse jwt token to get all claims
        var securityToken = tokenHandler.ReadToken(jwtToken) as JwtSecurityToken;

        // Search for missed claims, for example claim 'sub'
        var extraClaims = securityToken.Claims.Where(c => !identity.Claims.Any(x => x.Type == c.Type)).ToList();

        // Adding the original Jwt has 2 benefits:
        //  1) Authenticate REST service calls with orginal Jwt
        //  2) The original Jwt is available for renewing during sliding expiration
        extraClaims.Add(new Claim("jwt", jwtToken));

        // Merge claims
        identity.AddClaims(extraClaims);

        // Setup authenticaties 
        // ExpiresUtc is used in sliding expiration 
        var authenticationProperties = new AuthenticationProperties()
        {
          IssuedUtc = identity.Claims.First(c => c.Type == JwtRegisteredClaimNames.Iat)?.Value.ToInt64().ToUnixEpochDate(),
          ExpiresUtc = identity.Claims.First(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value.ToInt64().ToUnixEpochDate(),
          IsPersistent = true
        };

        // The actual Login
        await httpContext.SignInAsync(JwtBearerDefaults.AuthenticationScheme, principal, authenticationProperties);

        return identity.IsAuthenticated;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      return false;
    }


    public async Task RenewTokenAsync(String jwtToken)
    {
      var apiUrl = jwtTokenIssuerSettings.RenewToken;

      using (var httpClient = CreateClient())
      {
        using (var content = new FormUrlEncodedContent(new Dictionary<String, String>() { { "", jwtToken } }))
        {
          using (var response = await httpClient.PostAsync(apiUrl, content))
          {
            var renewedToken = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK)
              await Login(renewedToken);
          }
        }
      }
    }


    /// <summary>
    /// Returns true when user meets policy requirements
    /// Based on https://docs.microsoft.com/en-us/aspnet/core/security/authorization/views
    /// </summary>
    /// <param name="policyName"></param>
    /// <returns></returns>
    public async Task<Boolean> HasPolicy(String policyName)
    {
      var result = await authorizationService.AuthorizeAsync(this.User, null, policyName);

      return result.Succeeded;
    }
  }
}
