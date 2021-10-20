using DisneyAPI.Context;
using DisneyAPI.Entities;
using DisneyAPI.Interfaces;
using DisneyAPI.ViewModel.Auth.Register;
using DisneyAPI.ViewModel.Mail;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace DisneyAPI.Repositories
{
    public class AuthRepository : /*BaseRepository<User, UserContext>,*/ IAuthRepository
    {
        private readonly UserManager<User> _userManager;

        public AuthRepository(/*UserContext dbContext*/UserManager<User> userManager) //: base(dbContext)
        {
            _userManager = userManager;
        }


        public async Task<User> CreateNewUser(RegisterReqVM model)
        {
            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) throw new Exception("The password couldn't be setted. The user couldn't be created.");

            return user;
        }


        public MailServiceResVM CreateMailNewUserWelcome(User user)
        {
            MailServiceResVM mailServiceReqVM = new MailServiceResVM
            {
                Title = "Welcome to DisneyApp!",
                Username = user.UserName,
                Email = user.Email,
                Body = $"Welcome to the brand new DisneyApp, {user.UserName}!" +
                       "Hope you enjoy it!"
            };

            return mailServiceReqVM;
        }

    }
}
