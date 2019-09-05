using eventshighest.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Service
{
    public interface IJwtTokenService
    {
        Token CreateToken(AppUser appUser);
    }
}
