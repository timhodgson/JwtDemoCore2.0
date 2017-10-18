using Jwt.Issuer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Security.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Jwt;
using System.Threading.Tasks;

namespace Security.Controllers
{
    [AllowAnonymous]
    [Route("api/security")]
    public class JwtIssuerController : Controller
    {
        private readonly IJwtIssuerOptions JwtOptions;
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly SignInManager<ApplicationUser> SignInManager;
        private readonly RoleManager<IdentityRole> RoleManager;

        public JwtIssuerController(IJwtIssuerOptions jwtOptions,
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          RoleManager<IdentityRole> roleManager)
        {
            JwtOptions = jwtOptions;
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
        }


        [HttpPost(nameof(Login))]
        public async Task<IActionResult> Login([FromBody] LoginResource resource)
        {
            if (resource == null)
                return BadRequest("Login resource must be assigned");

            var user = await UserManager.FindByEmailAsync(resource.Email);

            if (user == null)
            {
                return BadRequest("Invalid credentials.  Cannot find user.");
            }
            if (!(await SignInManager.PasswordSignInAsync(user, resource.Password, false, false)).Succeeded)
            {
                return BadRequest(string.Format("Invalid credentials: u: {0}, e:{1}, p:{2}", user, resource.Email, resource.Password));
            }

            var token = await CreateJwtTokenAsync(user);

            // Token is created, we can sign out
            await SignInManager.SignOutAsync();

            var result = new ContentResult() { Content = token, ContentType = "application/text" };
            return result;
        }

        /// <summary>
        /// Fetch user roles and claims from storage
        /// </summary>
        /// <param name="user">application user</param>
        /// <returns>JWT token</returns>
        private async Task<String> CreateJwtTokenAsync(ApplicationUser user)
        {
            // Create JWT claims
            var claims = new List<Claim>(new[]
            {
        // Issuer
        new Claim(JwtRegisteredClaimNames.Iss, JwtOptions.Issuer),   

        // UserName
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),       

        // Email is unique
        new Claim(JwtRegisteredClaimNames.Email, user.Email),        

        // Unique Id for all Jwt tokes
        new Claim(JwtRegisteredClaimNames.Jti, await JwtOptions.JtiGenerator()), 

        // Issued at
        new Claim(JwtRegisteredClaimNames.Iat, JwtOptions.IssuedAt.ToUnixEpochDate().ToString(), ClaimValueTypes.Integer64)
      });

            // Add userclaims from storage
            claims.AddRange(await UserManager.GetClaimsAsync(user));

            // Add user role, they are converted to claims
            var roleNames = await UserManager.GetRolesAsync(user);
            foreach (var roleName in roleNames)
            {
                // Find IdentityRole by name
                var role = await RoleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    // Convert Identity to claim and add 
                    var roleClaim = new Claim(ClaimTypes.Role, role.Name, ClaimValueTypes.String, JwtOptions.Issuer);
                    claims.Add(roleClaim);

                    // Add claims belonging to the role
                    var roleClaims = await RoleManager.GetClaimsAsync(role);
                    claims.AddRange(roleClaims);
                }
            }

            // Prepare Jwt Token
            var jwt = new JwtSecurityToken(
                issuer: JwtOptions.Issuer,
                audience: JwtOptions.Audience,
                claims: claims,
                notBefore: JwtOptions.NotBefore,
                expires: JwtOptions.Expires,
                signingCredentials: JwtOptions.SigningCredentials);

            // Serialize token
            var result = new JwtSecurityTokenHandler().WriteToken(jwt);

            return result;
        }

        /// <summary>
        /// Renew Token when not yet expired. 
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <returns>New JWt token with refreshed claims and a new timespan for expiration</returns>
        [HttpPost(nameof(RenewToken))]
        public async Task<IActionResult> RenewToken(String jwtToken)
        {
            // Setup handler for processing Jwt token
            var tokenHandler = new JwtSecurityTokenHandler();

            // Setup token checking
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = JwtOptions.SigningCredentials.Key,

                RequireExpirationTime = true,
                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };

            try
            {
                // retrieve principal from Jwt token
                var principal = tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out var validatedToken);

                // cast needed to access Claims property
                var securityToken = validatedToken as JwtSecurityToken;

                var email = securityToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value;
                var user = await UserManager.FindByEmailAsync(email);

                var result = await CreateJwtTokenAsync(user);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
    }
}