using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DisneyAPI.ViewModel.Auth.Role
{
    public class CreateRoleReqVM
    {
        [Required]
        public string RoleName { get; set; }
    }
}
