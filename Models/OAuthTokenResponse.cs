using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAMLAspNetCoreTest.Models
{
    public class OAuthTokenResponse
    {
        public string token_type;
        public string scope;
        public string expires_in;
        public string ext_expires_in;
        public string expires_on;
        public string not_before;
        public string resource;
        public string access_token;
        public string refresh_token;
    }
}
