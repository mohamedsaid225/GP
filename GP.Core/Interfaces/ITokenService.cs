using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GP.Core.Entities;

namespace GP.Core.Interfaces
{
    public interface ITokenService
    { 
        string CreateToken(User user);
    }
}
