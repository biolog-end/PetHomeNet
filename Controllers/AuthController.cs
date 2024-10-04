using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using PetHome.Data;
using PetHome.Models;
using PetHome.Services;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace PetHome.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<AuthController> _logger;
        private static ConcurrentDictionary<string, (string identifier, string code, DateTime expiration)> _pendingRegistrations = new();
        private static ConcurrentDictionary<string, (string code, DateTime expiration)> _pendingResets = new();
        private static ConcurrentDictionary<string, List<DateTime>> _registrationAttempts = new();
        private static ConcurrentDictionary<string, List<DateTime>> _resetAttempts = new();
        private readonly TimeSpan _confirmationCodeLifetime = TimeSpan.FromMinutes(40);
        private readonly TimeSpan _resetCodeLifetime = TimeSpan.FromMinutes(3);

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validationError = ValidateRegistrationData(model);
            if (!string.IsNullOrEmpty(validationError))
                return BadRequest(validationError);

            var isPhoneNumber = Regex.IsMatch(model.Identifier, @"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$");

            if (isPhoneNumber)
            {
                if (await _userManager.FindByNameAsync(model.Identifier) != null)
                    return BadRequest("This phone number is already registered.");
            }
            else
            {
                if (await _userManager.FindByEmailAsync(model.Identifier) != null)
                    return BadRequest("This email is already registered.");
            }

            if (await _userManager.FindByNameAsync(model.Nickname) != null)
                return BadRequest("This nickname is already taken.");

            if (!CanSendRegistrationCode(model.Identifier))
                return BadRequest("Exceeded the number of code sending attempts. Try again later.");

            var confirmationCode = GenerateConfirmationCode();

            _pendingRegistrations[model.Identifier] = (model.Identifier, confirmationCode, DateTime.UtcNow.Add(_confirmationCodeLifetime));

            if (isPhoneNumber)
            {
                await _smsService.SendSmsAsync(model.Identifier, $"Your confirmation code from PetHome: {confirmationCode}");
            }
            else
            {
                await _emailService.SendRegistrationConfirmationAsync(model.Identifier, confirmationCode);
            }

            RecordRegistrationAttempt(model.Identifier);

            return Ok(new { message = isPhoneNumber ? "Check your SMS for confirmation." : "Check your email for confirmation." });
        }

        [HttpPost("confirm-registration")]
        public async Task<IActionResult> ConfirmRegistration([FromBody] ConfirmRegistrationModel model)
        {
            if (!_pendingRegistrations.TryGetValue(model.Identifier, out var registrationInfo))
                return BadRequest("Registration not found or expired.");

            if (DateTime.UtcNow > registrationInfo.expiration)
            {
                _pendingRegistrations.TryRemove(model.Identifier, out _);
                return BadRequest("The confirmation code has expired.");
            }

            if (model.ConfirmationCode != registrationInfo.code)
                return BadRequest("Invalid confirmation code.");

            var isPhoneNumber = Regex.IsMatch(model.Identifier, @"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$");

            var user = new ApplicationUser
            {
                UserName = model.Nickname,
                Email = isPhoneNumber ? null : model.Identifier,
                PhoneNumber = isPhoneNumber ? model.Identifier : null,
                Nickname = model.Nickname,
                AvatarUrl = "https://example.com/default-avatar.png",
                EmailConfirmed = !isPhoneNumber,
                PhoneNumberConfirmed = isPhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _pendingRegistrations.TryRemove(model.Identifier, out _);
                return Ok("Registration completed successfully.");
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] ResetPasswordRequestModel model)
        {
            if (!ValidateIdentifier(model.Identifier))
                return BadRequest("Invalid identifier format.");

            ApplicationUser user = null;

            if (ValidateEmail(model.Identifier))
            {
                user = await _userManager.FindByEmailAsync(model.Identifier);
            }
            else if (ValidatePhoneNumber(model.Identifier))
            {
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.Identifier);
            }
            else 
            {
                user = await _userManager.FindByNameAsync(model.Identifier);
            }

            if (user == null)
                return BadRequest("User not found.");

            string contactMethod = user.Email ?? user.PhoneNumber;
            if (string.IsNullOrEmpty(contactMethod))
                return BadRequest("No contact method available for this user.");

            if (!CanSendResetCode(contactMethod))
                return BadRequest("Exceeded the number of password reset attempts. Try again later.");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetCode = GenerateResetCode();
            _pendingResets[contactMethod] = (resetCode, DateTime.UtcNow.Add(_resetCodeLifetime));

            if (ValidatePhoneNumber(contactMethod))
            {
                await _smsService.SendSmsAsync(contactMethod, $"Your password reset code: {resetCode}");
            }
            else
            {
                await _emailService.SendPasswordResetAsync(contactMethod, resetCode);
            }

            RecordResetAttempt(contactMethod);

            return Ok(new { message = $"The password reset code has been sent to your {(ValidatePhoneNumber(contactMethod) ? "phone" : "email")}.", identifier = contactMethod });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ConfirmResetPasswordModel model)
        {
            if (!_pendingResets.TryGetValue(model.Identifier, out var resetInfo))
                return BadRequest("Password reset not found.");

            if (resetInfo.expiration < DateTime.UtcNow)
            {
                _pendingResets.TryRemove(model.Identifier, out _);
                return BadRequest("The password reset code has expired.");
            }

            if (model.Code != resetInfo.code)
                return BadRequest("Invalid password reset code.");

            if (!ValidatePassword(model.NewPassword))
                return BadRequest("Password does not meet security requirements.");

            ApplicationUser user = null;

            if (ValidateEmail(model.Identifier))
            {
                user = await _userManager.FindByEmailAsync(model.Identifier);
            }
            else if (ValidatePhoneNumber(model.Identifier))
            {
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.Identifier);
            }
            else
            {
                user = await _userManager.FindByNameAsync(model.Identifier);
            }

            if (user == null)
                return BadRequest("User not found.");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

            if (result.Succeeded)
            {
                _pendingResets.TryRemove(model.Identifier, out _);
                return Ok(new { message = "Password successfully reset." });
            }

            return BadRequest(result.Errors);
        }

        private bool CanSendRegistrationCode(string email)
        {
            if (!_registrationAttempts.TryGetValue(email, out var attempts))
                return true;

            var recentAttempts = attempts.Where(a => a > DateTime.UtcNow.AddMinutes(-15)).ToList();
            return recentAttempts.Count < 3;
        }

        private void RecordRegistrationAttempt(string email)
        {
            _registrationAttempts.AddOrUpdate(email,
                new List<DateTime> { DateTime.UtcNow },
                (_, attempts) =>
                {
                    attempts.Add(DateTime.UtcNow);
                    return attempts.Where(a => a > DateTime.UtcNow.AddMinutes(-15)).ToList();
                });
        }

        private bool CanSendResetCode(string email)
        {
            if (!_resetAttempts.TryGetValue(email, out var attempts))
                return true;

            var recentAttempts = attempts.Where(a => a > DateTime.UtcNow.AddMinutes(-30)).ToList();
            return recentAttempts.Count < 2;
        }

        private void RecordResetAttempt(string email)
        {
            _resetAttempts.AddOrUpdate(email,
                new List<DateTime> { DateTime.UtcNow },
                (_, attempts) =>
                {
                    attempts.Add(DateTime.UtcNow);
                    return attempts.Where(a => a > DateTime.UtcNow.AddMinutes(-30)).ToList();
                });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Identifier) || string.IsNullOrEmpty(model.Password))
                return BadRequest("Identifier and password are required.");

            if (!ValidateIdentifier(model.Identifier))
                return BadRequest("Invalid identifier format.");

            ApplicationUser user = null;

            if (ValidateEmail(model.Identifier))
            {
                user = await _userManager.FindByEmailAsync(model.Identifier);
            }
            else if (ValidatePhoneNumber(model.Identifier))
            {
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.Identifier);
            }
            else 
            {
                user = await _userManager.FindByNameAsync(model.Identifier);
            }

            if (user == null)
                return BadRequest("User not found.");

            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);
            if (result.Succeeded)
            {
                var token = GenerateJwtToken(user);
                return Ok(new { token });
            }

            return Unauthorized("Invalid credentials.");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Logged out successfully.");
        }

        private string GenerateConfirmationCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }
        private string GenerateResetCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim("username", user.UserName ?? string.Empty)
            };

            var jwtKey = _configuration["JwtSettings:SecretKey"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtSettings:ExpiryInDays"] ?? "1"));

            var token = new JwtSecurityToken(
                _configuration["JwtSettings:Issuer"],
                _configuration["JwtSettings:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string ValidateRegistrationData(RegisterModel model)
        {
            if (!ValidateNickname(model.Nickname))
            {
                return "The nickname does not meet the requirements.";
            }

            if (!ValidatePassword(model.Password))
            {
                return "The password does not meet security requirements.";
            }

            var isPhoneNumber = Regex.IsMatch(model.Identifier, @"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$");
            if (!isPhoneNumber && !ValidateEmail(model.Identifier))
            {
                return "Invalid email or phone number.";
            }

            return string.Empty;
        }

        private bool ValidatePassword(string password)
        {
            return !string.IsNullOrEmpty(password) &&
                   password.Length >= 8 &&
                   Regex.IsMatch(password, @"[A-Z]") &&
                   Regex.IsMatch(password, @"[a-z]") &&
                   Regex.IsMatch(password, @"\d") &&
                   Regex.IsMatch(password, @"[\W_]");
        }
        private bool ValidateIdentifier(string identifier)
        {
            return ValidateEmail(identifier) || ValidatePhoneNumber(identifier) || ValidateNickname(identifier);
        }

        private bool ValidateEmail(string email)
        {
            return Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
        }

        private bool ValidatePhoneNumber(string phoneNumber)
        {
            return Regex.IsMatch(phoneNumber, @"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$");
        }

        private bool ValidateNickname(string nickname)
        {
            return !string.IsNullOrEmpty(nickname) &&
                   nickname.Length >= 3 &&
                   nickname.Length <= 15 &&
                   Regex.IsMatch(nickname, @"^[a-zA-Z][a-zA-Z0-9_-]+$");
        }
        public static void CleanupExpiredData()
        {
            var now = DateTime.UtcNow;

            foreach (var kvp in _pendingRegistrations.ToList())
            {
                if (kvp.Value.expiration < now)
                {
                    _pendingRegistrations.TryRemove(kvp.Key, out _);
                }
            }

            foreach (var kvp in _pendingResets.ToList())
            {
                if (kvp.Value.expiration < now)
                {
                    _pendingResets.TryRemove(kvp.Key, out _);
                }
            }

            foreach (var kvp in _registrationAttempts.ToList())
            {
                var updatedAttempts = kvp.Value.Where(a => a > now.AddMinutes(-15)).ToList();
                if (updatedAttempts.Count == 0)
                {
                    _registrationAttempts.TryRemove(kvp.Key, out _);
                }
                else
                {
                    _registrationAttempts[kvp.Key] = updatedAttempts;
                }
            }

            foreach (var kvp in _resetAttempts.ToList())
            {
                var updatedAttempts = kvp.Value.Where(a => a > now.AddMinutes(-30)).ToList();
                if (updatedAttempts.Count == 0)
                {
                    _resetAttempts.TryRemove(kvp.Key, out _);
                }
                else
                {
                    _resetAttempts[kvp.Key] = updatedAttempts;
                }
            }
        }
    }

    public class RegisterModel
    {
        public string Identifier { get; set; }
        public string Password { get; set; }
        public string Nickname { get; set; }
    }

    public class ConfirmRegistrationModel
    {
        public string Identifier { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
        public string ConfirmationCode { get; set; }
    }

    public class LoginModel
    {
        public string Identifier { get; set; } 
        public string Password { get; set; }
    }
    public class ResetPasswordRequestModel
    {
        public string Identifier { get; set; }
    }

    public class ConfirmResetPasswordModel
    {
        public string Identifier { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }
}
