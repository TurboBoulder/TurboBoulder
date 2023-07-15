using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdaWebApplicationTemplate.Shared.DataTransferObjects
{
    public class DTODeleteUser
    {
        public string Password { get; set; }
        public bool Delete { get; set; } = false;
    }
}
