using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdaWebApplicationTemplate.Shared.DataTransferObjects
{
    public class DTOConfirmResetPassword
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
