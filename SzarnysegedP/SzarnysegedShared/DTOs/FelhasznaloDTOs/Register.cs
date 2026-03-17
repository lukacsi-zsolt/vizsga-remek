using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SzarnysegedShared.DTOs.FelhasznaloDTOs
{
    public class RegisterDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string TeljesNev { get; set; }
        public string Email { get; set; }
        public DateTime? SzuletesiDatum { get; set; }
    }
}
