using DisneyAPI.Entities;
using DisneyAPI.ViewModel.Auth.Register;
using DisneyAPI.ViewModel.Mail;
using System.Threading.Tasks;

namespace DisneyAPI.Interfaces
{
    public interface IAuthRepository
    {
        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<User> CreateNewUser(RegisterReqVM model);

        /// <summary>
        /// Sets the title and body for welcoming the new user.
        /// </summary>
        /// <param name="user"></param>
        MailServiceResVM CreateMailNewUserWelcome(User user);
    }
}
