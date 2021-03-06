using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SMECommerce.App.Models.Auth;
using SMECommerce.Models.EntityModels.Identity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SMECommerce.App.Controllers
{
    public class AuthController : Controller
    {
        SignInManager<AppUser> _signInManager;
        UserManager<AppUser> _userManager;
        IPasswordHasher<AppUser> _passwordHasher;
        IConfiguration _config; 
        public AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IPasswordHasher<AppUser> passwordHasher, IConfiguration config)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _passwordHasher = passwordHasher;
            _config = config; 
        }



        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }


            return View();

        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // code. . .. 

            if (ModelState.IsValid)
            {
                var user = new AppUser()
                {
                    UserName = model.Email,
                    Email = model.Email
                };
                
               var result = await _userManager.CreateAsync(user, model.Password);

              

                if (result.Succeeded)
                {
                    return RedirectToAction("Login");
                }
            }


            return View(); 
        }

        
        [HttpPost]
        public async Task<IActionResult> Token([FromBody] LoginViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);

            if(user != null)
            {
                // verify password 

               var result =  _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password); 

               if(result == PasswordVerificationResult.Success)
                {
                    // token generate for user. 

                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                    var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    var userClaims = await _userManager.GetClaimsAsync(user);


                    var claims = new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                        new Claim(JwtRegisteredClaimNames.GivenName, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, (new Guid()).ToString())

                    }.Union(userClaims);


                    var token = new JwtSecurityToken(
                        issuer: _config["Jwt:Issuer"],
                        audience: _config["Jwt:Issuer"],
                        claims: claims,
                        expires: DateTime.Now.AddMinutes(120),
                        signingCredentials: signingCredentials
                        ) ;


                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                    return Ok(new { token = tokenString, expires = token.ValidTo });

                }


            }

            return BadRequest("User or Password could not match, please check"); 

        }
        
    }
}
