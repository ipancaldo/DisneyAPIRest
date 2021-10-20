using DisneyAPI.Entities;
using DisneyAPI.Interfaces;
using DisneyAPI.ViewModel.Auth.Login;
using DisneyAPI.ViewModel.Auth.Register;
using DisneyAPI.ViewModel.Mail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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
        public async Task<IActionResult> Register([FromQuery] RegisterReqVM model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);

            if (userExists is not null) return BadRequest(new
            {
                Status = "Error",
                Message = $"User creation failed for username {model.Username}. User already in the database."
            });

            try
            {
                var user = await _authRepository.CreateNewUser(model);

                //if (!result.Succeeded)
                //{
                //    return StatusCode(StatusCodes.Status500InternalServerError, 
                //                      new
                //                        {
                //                            Status = "Error",
                //                            Message = $"The user couldn't be created. Errors: {string.Join(" ,", result.Errors.Select(x => x.Description ))}"
                //                        });
                //}

                await _mailService.SendEmail(_authRepository.CreateMailNewUserWelcome(user));

                return Ok(new
                            {
                                Status = "Ok",
                                Message = $"User {user.UserName} created succesfully."
                            });
            }
            catch (Exception ex)
            {
                if (ex.Message == "The password couldn't be setted. The user couldn't be created.")
                {
                  return StatusCode(StatusCodes.Status500InternalServerError,
                                      new
                                      {
                                          Status = "Error",
                                          Message = ex.Message
                                      });
                }

                if( ex.Message == "The user already exists in the database")
                {
                    return BadRequest(new
                    {
                        Status = "Error",
                        Message = $"User creation failed for username {model.Username}. User already in the database."
                    });
                }

                return BadRequest(new
                {
                    Status = "Error",
                    Message = $"User creation failed for username {model.Username}."
                });
            }
        }


        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromQuery] RegisterReqVM model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);

            if (userExists is not null) return BadRequest(new
            {
                Status = "Error",
                Message = $"User creation failed for username {model.Username}. User already in the database."
            });

            var user = _authRepository.CreateNewUser(model);

            try
            {
                //if (!result.Succeeded)
                //{
                //    return StatusCode(StatusCodes.Status500InternalServerError,
                //                      new
                //                      {
                //                          Status = "Error",
                //                          Message = $"The user couldn't be created. Errors: {string.Join(" ,", result.Errors.Select(x => x.Description))}"
                //                      });
                //}

                if (!await _roleManager.RoleExistsAsync("Admin"))
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));

                //await _userManager.AddToRoleAsync(user, "Admin");

                //MailServiceResVM mailServiceReqVM = new MailServiceResVM
                //{
                //    Title = "Welcome to Disney - ADMIN",
                //    Username = user.UserName,
                //    Email = user.Email
                //};
                //await _mailService.SendEmail(mailServiceReqVM);

                return Ok(new
                            {
                                Status = "Ok",
                                Message = $"User {model.Username} created succesfully."
                            });
            }
            catch (Exception)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    Message = $"User ADMIN creation failed."
                });
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
                        //return Ok(await GetToken(currentUser));
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
    }
}
