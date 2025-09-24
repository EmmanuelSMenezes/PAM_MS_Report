using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class DecodedToken
    {
        public Guid UserId { get; set; }    
        public Guid RoleId { get; set; }    
        public string Email { get; set; }   
        public string Name { get; set; }    
    }
}
