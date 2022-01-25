using DisneyAPI.Entities;
using DisneyAPI.Interfaces;
using DisneyAPI.ViewModel.Auth.Login;
using DisneyAPI.ViewModel.Auth.Register;
using DisneyAPI.ViewModel.Auth.Role;
using DisneyAPI.ViewModel.Auth.User;
using DisneyAPI.ViewModel.Mail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DisneyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMailService _mailService;
        private readonly IAuthRepository _authRepository;

        public AuthController(UserManager<User> userManager, 
                              SignInManager<User> signInManager,
                              RoleManager<IdentityRole> roleManager,
                              IMailService mailService,
                              IAuthRepository authRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _mailService = mailService;
            _authRepository = authRepository;
        }

        [HttpPost]
        [Route("register")]
        //Creates a new User
        public async Task<IActionResult> Register([FromQuery] RegisterReqVM model)
        {
            try
            {
                var user = await _authRepository.CreateNewUser(model);

                await SetRole(model.Username, "User");

                return Ok(new
                            {
                                Status = "Ok",
                                Message = $"User {user.UserName} created succesfully."
                            });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    Message = $"User creation failed for username {model.Username}. {ex.Message}"
                });
            }
        }

        [HttpPost]
        [Route("set-role")]
        public async Task<IActionResult> SetRole(string userName, string roleName)
        {
            try
            {
                var user = await _authRepository.SetRole(userName, roleName);

                return Ok(new
                            {
                                Status = "Ok",
                                Message = $"Role {roleName} assigned to {userName}"
                            });
            }
            catch (Exception ex)
            {
                if(ex.Message == "The user is not in the database" ||
                   ex.Message == "The role is not in the database")
                        return NotFound(ex.Message);
                return BadRequest(ex.Message);

            }
        }

        [HttpGet]
        [Route("check-user-role")]
        public async Task<IActionResult> CheckUserRole(string userName)
        {
            try
            {
                var roles = await _authRepository.CheckUserRole(userName);
                string allRoles = "";
                for (int i = 0; i < roles.Count; i++)
                {
                    if (roles.Count == 1) {
                        allRoles = roles[i];
                        break;
                    }

                    if (i == 0) allRoles = roles[i] + ", ";

                    if (i == roles.Count) {
                        allRoles = allRoles + roles[i];
                        break;
                    }
                    allRoles = allRoles + roles[i] + ", ";
                }

                return Ok(new
                {
                    Status = "Ok",
                    Message = $"The roles for the user {userName} is: {allRoles}."
                });
            }
            catch (Exception ex)
            {
                if (ex.Message == "The user is not in the database")
                    return NotFound(ex.Message);

                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromQuery] RegisterReqVM model)
        {
            try
            {
                var userCreated = await _authRepository.CreateNewUser(model);
                var user = await _authRepository.SetRole(userCreated.UserName, "Admin");

                return Ok(new
                            {
                                Status = "Ok",
                                Message = $"User {model.Username} created succesfully as an admin."
                            });
            }
            catch (Exception ex)
            {
                if (ex.Message == "The user is not in the database" ||
                    ex.Message == "The role is not in the database")
                        return NotFound(ex.Message);

                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromQuery] LoginReqVM model)
        {
            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
                if (result.Succeeded)
                {
                    var currentUser = await _userManager.FindByNameAsync(model.Username);
                    if (currentUser.IsActive)
                    {
                        return Ok(await GetToken(currentUser));
                    }
                }

                return StatusCode(StatusCodes.Status401Unauthorized, new
                {
                    Status = "Error",
                    Message = $"The user {model.Username} is not authorized."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost]
        [Route("create-role")]
        public async Task<IActionResult> CreateRole([FromQuery] CreateRoleReqVM model)
        {
            var valid = ModelState.IsValid;
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _authRepository.CreateRole(model);
                    return Ok($"Role {model.RoleName} created successfully.");
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Role creation failed.")
                        return BadRequest(new
                        {
                            Status = "Error",
                            Message = $"Couldn't create role {model.RoleName}."
                        });

                    return BadRequest();
                }
            }
            return BadRequest(new
            {
                Status = "Error",
                Message = "Please complete correctly the model."
            });
        }

        [HttpGet]
        [Route("list-roles")]
        public IActionResult ListRoles()
        {
            var roles = _authRepository.GetRoles();

            if (roles is null) return NoContent();

            return Ok(roles);
        }

        [HttpGet]
        [Route("list-users")]
        public IActionResult ListUsers()
        {
            var users = _authRepository.GetUsers();

            if (users is null) return NoContent();

            return Ok(users);
        }

        private async Task<LoginResVM> GetToken(User currentUser)
        {
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            var authClaim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, currentUser.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            authClaim.AddRange(userRoles.Select(x => new Claim(ClaimTypes.Role, x)));

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("VeryLongANDSecureKeyfORtESTING"));

            var token = new JwtSecurityToken(issuer: "http://localhost:5000",
                                             audience: "http://localhost:5000",
                                             expires: DateTime.Now.AddHours(1),
                                             claims: authClaim,
                                             signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));
            return new LoginResVM
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ValidTo = token.ValidTo
            };
        }
    }
}
