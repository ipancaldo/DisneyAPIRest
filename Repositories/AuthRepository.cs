using DisneyAPI.Context;
using DisneyAPI.Entities;
using DisneyAPI.Interfaces;
using DisneyAPI.ViewModel.Auth.Register;
using DisneyAPI.ViewModel.Mail;
using DisneyAPI.ViewModel.Error;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DisneyAPI.ViewModel.Auth.User;
using DisneyAPI.ViewModel.Auth.Role;

namespace DisneyAPI.Repositories
{
    public class AuthRepository : /*BaseRepository<User, UserContext>,*/ IAuthRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMailService _mailService;

        public AuthRepository(UserManager<User> userManager,
                              RoleManager<IdentityRole> roleManager,
                              IMailService mailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mailService = mailService;
        }

        public async Task<User> CheckUserExistence(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }


        public async Task<User> CreateNewUser(RegisterReqVM model)
        {
            try
            {
                if (await CheckUserExistence(model.Username) is not null) throw new Exception("The character is already in the database.");

                var user = new User
                {
                    UserName = model.Username,
                    Email = model.Email,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                await _mailService.SendEmail(EmailUser(user, "Welcome to Disney!", "Welcome to Disney."));

                if (!result.Succeeded)
                {
                    ErrorResVM error = new ErrorResVM
                    {
                        ExMessage = string.Join(" ", result.Errors.Select(x => x.Description)),
                        CustomMessage = "The password couldn't be setted. The user couldn't be created."
                    };

                    throw new Exception(error.ToString());
                }

                return user;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<User> SetRole(string userName, string roleName)
        {
            try
            {
                var users = _userManager.Users;

                var user = await _userManager.FindByNameAsync(userName);
                if (user is null) throw new Exception("The user is not in the database");

                var role = await _roleManager.FindByNameAsync(roleName);
                if (role is null) throw new Exception("The role is not in the database");

                if (await _userManager.IsInRoleAsync(user, role.Name)) throw new Exception("The user has already that role.");

                var result = await _userManager.AddToRoleAsync(user, role.Name);

                if (!result.Succeeded)
                {
                    ErrorResVM error = new ErrorResVM
                    {
                        ExMessage = string.Join(" ", result.Errors.Select(x => x.Description)),
                        CustomMessage = "The role couldn't be assigned to the user."
                    };

                    throw new Exception(error.ToString());
                }

                await _mailService.SendEmail(EmailUser(user, $"Role {roleName} - {userName}", $"Role {roleName} setted to user {userName}"));

                return user;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public List<GetRolesResVM> GetRoles()
        {
            var roles = _roleManager.Roles;
            if (roles is null)  throw new Exception("There is no roles in the DB.");

            List<GetRolesResVM> getRolesResVM = new List<GetRolesResVM>();

            foreach (var rol in roles)
            {
                getRolesResVM.Add(new GetRolesResVM
                {
                    RoleName = rol.Name
                }); 
            }

            return getRolesResVM;
        }        
        
        public List<GetListUsersResVM> GetUsers()
        {
            var users = _userManager.Users;

            if (users is null) throw new Exception("There is no users in the DB.");

            List<GetListUsersResVM> getListUsersResVM = new List<GetListUsersResVM>();

            foreach (var user in users)
            {
                getListUsersResVM.Add(new GetListUsersResVM
                {
                    IsActive = user.IsActive,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = ""
                });
            }

            return getListUsersResVM;
        }


        public MailServiceResVM EmailUser(User user, string title, string body)
        {
            MailServiceResVM mailServiceReqVM = new MailServiceResVM
            {
                Title = title,
                Username = user.UserName,
                Email = user.Email,
                Body = body
            };

            return mailServiceReqVM;
        }

    }
}
