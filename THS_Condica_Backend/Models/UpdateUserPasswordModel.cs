using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace THS_Condica_Backend.Models
{
    public class UpdateUserPasswordModel
    {

        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
