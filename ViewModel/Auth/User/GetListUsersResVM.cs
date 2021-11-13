using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisneyAPI.ViewModel.Auth.User
{
    public class GetListUsersResVM
    {
        public bool IsActive { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        
        public string Role { get; set; }
    }
}
