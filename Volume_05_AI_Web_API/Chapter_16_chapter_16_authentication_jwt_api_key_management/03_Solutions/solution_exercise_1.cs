
/*
#
# These sources are part of the "C# Programming Series" by Edgar Milvus, 
# for additional info, new volumes, link to stores:
# https://github.com/edgarmilvus/CSharpProgrammingSeries
#
# MIT License
# Copyright (c) 2026 Edgar Milvus
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.
*/

// Source File: solution_exercise_1.cs
// Description: Solution for Exercise 1
// ==========================================

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

public interface ITokenService
{
    Task<AuthResult> GenerateTokensAsync(string userId, string email, List<string> roles);
    Task<AuthResult> RefreshTokenAsync(string expiredToken, string refreshToken);
    Task<bool> RevokeTokenAsync(string userId);
}

public record AuthResult(string AccessToken, string RefreshToken, DateTime AccessTokenExpiry);

// Configuration class for JWT settings
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IMemoryCache _cache;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public TokenService(IOptions<JwtSettings> jwtSettings, IMemoryCache cache)
    {
        _jwtSettings = jwtSettings.Value;
        _cache = cache;
        
        // Setup validation parameters for validating the expired Access Token during refresh
        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = false, // We manually check expiration for refresh logic
            ClockSkew = TimeSpan.Zero
        };
    }

    public async Task<AuthResult> GenerateTokensAsync(string userId, string email, List<string> roles)
    {
        var accessToken = GenerateAccessToken(userId, email, roles);
        var refreshToken = GenerateRefreshToken();

        // Store refresh token in cache (simulating a distributed store)
        // Key: userId, Value: refreshToken (hashed for security)
        var refreshTokenHash = ComputeSha256Hash(refreshToken);
        
        // Cache the token with an expiration matching the refresh token (7 days)
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
        };

        // In a real distributed system, we might store a list of valid tokens per user
        // For this simulation, we store the current valid hash. 
        // Note: To support rotation, we will overwrite the previous hash. 
        // In a real DB/Cache, we would store a list or a specific token entity.
        await Task.Run(() => _cache.Set(GetCacheKey(userId), refreshTokenHash, cacheOptions));

        return new AuthResult(accessToken, refreshToken, DateTime.UtcNow.AddMinutes(15));
    }

    public async Task<AuthResult> RefreshTokenAsync(string expiredToken, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(expiredToken);
        if (principal == null) throw new SecurityTokenException("Invalid token");

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue(ClaimTypes.Email);
        var roles = principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        if (string.IsNullOrEmpty(userId)) throw new SecurityTokenException("Invalid token claims");

        // 1. Check for Replay Attack (Double Usage)
        // Retrieve the stored hash from cache
        if (!_cache.TryGetValue(GetCacheKey(userId), out string storedHash))
        {
            // Token was revoked or expired
            throw new SecurityTokenException("Refresh token expired or revoked");
        }

        var inputHash = ComputeSha256Hash(refreshToken);
        
        // Constant-time comparison to prevent timing attacks
        if (!CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(storedHash), 
            Convert.FromBase64String(inputHash)))
        {
            // Potential Replay Attack detected!
            // Requirement: Revoke all tokens for this user immediately
            await RevokeTokenAsync(userId);
            throw new SecurityTokenException("Token replay detected. All tokens revoked.");
        }

        // 2. Token Rotation: Invalidate the used refresh token
        // We remove the old token from the cache
        _cache.Remove(GetCacheKey(userId));

        // 3. Generate new pair
        return await GenerateTokensAsync(userId, email, roles);
    }

    public async Task<bool> RevokeTokenAsync(string userId)
    {
        await Task.Run(() => _cache.Remove(GetCacheKey(userId)));
        return true;
    }

    // Helper to generate Access Token
    private string GenerateAccessToken(string userId, string email, List<string> roles)
    {
        var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add roles
        var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToArray();
        var allClaims = claims.Concat(roleClaims).ToArray();

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: allClaims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Helper to generate Refresh Token (Cryptographically Secure)
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out SecurityToken validatedToken);
            
            if (validatedToken is not JwtSecurityToken jwtToken || 
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private string ComputeSha256Hash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    private string GetCacheKey(string userId) => $"RefreshToken:{userId}";
}

// Program.cs Setup
public static class ServiceCollectionExtensions
{
    public static void AddCustomTokenService(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        services.AddSingleton(Options.Create(jwtSettings));

        services.AddScoped<ITokenService, TokenService>();

        // Configure JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true, // Standard validation checks expiry
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddMemoryCache(); // Required for the TokenService simulation
    }
}
