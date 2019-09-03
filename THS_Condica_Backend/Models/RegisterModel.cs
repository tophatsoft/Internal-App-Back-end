using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace THS_Condica_Backend.Models
{
    public class RegisterModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime RegisteredDate { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string Role { get; set; }
        public bool PassChanged { get; set; }

    }
}
