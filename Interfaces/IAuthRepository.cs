using DisneyAPI.Entities;
using DisneyAPI.ViewModel.Auth.Register;
using DisneyAPI.ViewModel.Auth.Role;
using DisneyAPI.ViewModel.Auth.User;
using DisneyAPI.ViewModel.Mail;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DisneyAPI.Interfaces
{
    public interface IAuthRepository
    {
        /// <summary>
        /// Checks if the user exists in the DB.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<User> CheckUserExistence(string userName);

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<User> CreateNewUser(RegisterReqVM model);

        /// <summary>
        /// Sets a specific role to an user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<User> SetRole(string userName, string roleName);

        /// <summary>
        /// Sets the title and body for sending a email to a certain user.
        /// </summary>
        /// <param name="user"></param>
        MailServiceResVM EmailUser(User user, string title, string body);

        /// <summary>
        /// Lists all the roles in the DB
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        List<GetRolesResVM> GetRoles();

        /// <summary>
        /// Lists all the users in the DB
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        List<GetListUsersResVM> GetUsers();
    }
}
