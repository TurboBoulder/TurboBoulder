﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdaWebApplicationTemplate.Shared.DataTransferObjects
{
    public class DTOToken
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
