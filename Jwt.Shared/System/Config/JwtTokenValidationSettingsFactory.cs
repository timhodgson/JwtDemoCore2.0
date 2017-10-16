using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace System.Config
{
  public class JwtTokenValidationSettings
  {
    public String ValidIssuer { get; set; }
    public Boolean ValidateIssuer { get; set; }

    public String ValidAudience { get; set; }
    public Boolean ValidateAudience { get; set; }

    public String SecretKey { get; set; }
  }

  public interface IJwtTokenValidationSettings
  {
    String ValidIssuer { get; }
    Boolean ValidateIssuer { get; }

    String ValidAudience { get; }
    Boolean ValidateAudience { get; }

    String SecretKey { get; }

    TokenValidationParameters CreateTokenValidationParameters();
  }

  public class JwtTokenValidationSettingsFactory : IJwtTokenValidationSettings
  {
    private readonly JwtTokenValidationSettings settings;

    public String ValidIssuer => settings.ValidIssuer;
    public Boolean ValidateIssuer => settings.ValidateIssuer;
    public String ValidAudience => settings.ValidAudience;
    public Boolean ValidateAudience => settings.ValidateAudience;
    public String SecretKey => settings.SecretKey;

    public JwtTokenValidationSettingsFactory(IOptions<JwtTokenValidationSettings> options)
    {
      settings = options.Value;
    }

    public TokenValidationParameters CreateTokenValidationParameters()
    {
      var result = new TokenValidationParameters
      {
        ValidateIssuer = ValidateIssuer,
        ValidIssuer = ValidIssuer,

        ValidateAudience = ValidateAudience,
        ValidAudience = ValidAudience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey)),

        RequireExpirationTime = true,
        ValidateLifetime = true,

        ClockSkew = TimeSpan.Zero
      };

      return result;
    }
  }
}