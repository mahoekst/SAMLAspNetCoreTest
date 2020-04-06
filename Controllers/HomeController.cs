using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Graph = Microsoft.Graph;
using Microsoft.Graph.Core;
using SAMLAspNetCoreTest.Models;
using Microsoft.Identity.Client;
using System.IO;
using SAMLAspNetCoreTest.Services;
using SAMLAspNetCoreTest.Infrastructure;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SAMLAspNetCoreTest.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        //readonly ITokenAcquisition tokenAcquisition;
        readonly WebOptions webOptions;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

//        public HomeController(
//                        IOptions<WebOptions> webOptionValue)
//        {
////            this.tokenAcquisition = tokenAcquisition;
//            this.webOptions = webOptionValue.Value;
//        }

        private IPublicClientApplication _msalClient;
        private string[] _scopes;
        private IAccount _userAccount;

        string signInEndpoint = "https://login.microsoftonline.com/63eb1bcb-f74f-4703-8243-6f73d78ebf52/oauth2/authorize";
        string tokenEndpoint = "https://login.microsoftonline.com/63eb1bcb-f74f-4703-8243-6f73d78ebf52/oauth2/token";
        string oauthResource = "https://graph.microsoft.com";
        string graphEndpoint = "https://graph.microsoft.com/beta/me";
        string clientId = "9fd05134-d507-479b-a432-580541125356";
        string secret = "ekdht8fuNhn/9Q74VL-fksIS[p?8Bo]H";
        string graphResponseContentFormatted = "";

        HttpClient client = new HttpClient();
        [HttpGet("")]
        public async Task<IActionResult> Index()// string id, string code, string state, string access_token, string token_type, string expires_in, string id_token)
        {
            //string type = Request.QueryString["type"];
            string code="";
            if (User.Identity.IsAuthenticated)
            {
                // Get users's email.
                //email = email ?? User.FindFirst("preferred_username")?.Value;
                //ViewData["Email"] = email;

                string tenantid = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value;



                //Set redirect URI to where app is deployed
                string redirectUri = "https://localhost:44300/"; // HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);

                //Build query string for /authorize url 
                var auth_params = new Dictionary<string, string>();
                auth_params.Add("client_id", clientId);
                auth_params.Add("response_type", "token"); //token  //code
                auth_params.Add("redirect_uri", redirectUri);
                auth_params.Add("response_mode", "fragment"); //fragment //query
                auth_params.Add("resource", oauthResource);
                auth_params.Add("prompt", "none");
                auth_params.Add("login_hint", User.Identity.Name);
                //auth_params.Add("nonce", "asd");
                var queryString = new FormUrlEncodedContent(auth_params);
                string oauthRequestUri = signInEndpoint + "?" + queryString.ReadAsStringAsync().Result;

                ViewData["oauthRequestUri"] = oauthRequestUri;
                if (!(code == null))
                {


                // If there's a code in the response, assume we have a redirect back from /authorize and we should go get an access token and call graph
                    using (client)
                    {
                        Uri signInUri = new Uri(tokenEndpoint);

                        //Build params string for /token POST
                        var req_params = new Dictionary<string, string>();
                        req_params.Add("client_id", clientId);
                        req_params.Add("grant_type", "authorization_code");
                        req_params.Add("redirect_uri", redirectUri);
                        req_params.Add("code", code);
                        //Badness for POC - get rid of this
                        req_params.Add("client_secret", secret);
                        //
                        req_params.Add("resource", oauthResource);
                        var content = new FormUrlEncodedContent(req_params);

                        //Send request to /token endpoint
                        HttpResponseMessage tokenResponse = await client.PostAsync(signInUri, content);
                        //Check the token response is successful, if so make graph call 
                        if (tokenResponse.IsSuccessStatusCode)
                        {

                            var tokenResponseContent = tokenResponse.Content.ReadAsStringAsync().Result;

                            ////Deserialize the response recieved from /token endpoint and store into the an OAuthTokenResponse object  
                            OAuthTokenResponse tokenResponseObject = JsonConvert.DeserializeObject<OAuthTokenResponse>(tokenResponseContent);

                            var bearerTokenHeader = new AuthenticationHeaderValue("Bearer", tokenResponseObject.access_token);
                            client.DefaultRequestHeaders.Authorization = bearerTokenHeader;

                            ////Call Graph to get Profile for current user
                            Uri graphUri = new Uri(graphEndpoint);
                            HttpResponseMessage graphResponse = await client.GetAsync(graphUri);
                            string graphResponseContent = await graphResponse.Content.ReadAsStringAsync();
                            graphResponseContentFormatted = JValue.Parse(graphResponseContent).ToString(Formatting.Indented);
                            ViewData["graphResponseContentFormatted"] = graphResponseContentFormatted;
                        }
                    }
                }

            //_msalClient = PublicClientApplicationBuilder
            //.Create("9fd05134-d507-479b-a432-580541125356")
            //.WithAuthority(AzureCloudInstance.AzurePublic, new Guid(tenantid))
            //.Build();

            //string[] scopes = new string[] {"user.read"};
            //AuthenticationResult result;


            //result = await _msalClient.AcquireTokenSilent(scopes, User.Identity.Name).ExecuteAsync();

        }

            return View();
        }


        public async Task<IActionResult> Profile()
        {
        //    // Initialize the GraphServiceClient. 
        //    //Graph::GraphServiceClient graphClient = GetGraphServiceClient(new[] { Constants.ScopeUserRead });

        //    //var me = await graphClient.Me.Request().GetAsync();
        //    //ViewData["Me"] = me;

        //    //try
        //    //{
        //    //    // Get user photo
        //    //    using (var photoStream = await graphClient.Me.Photo.Content.Request().GetAsync())
        //    //    {
        //    //        byte[] photoByte = ((MemoryStream)photoStream).ToArray();
        //    //        ViewData["Photo"] = Convert.ToBase64String(photoByte);
        //    //    }
        //    //}
        //    //catch (System.Exception)
        //    //{
        //    //    ViewData["Photo"] = null;
        //    //}

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //private Graph::GraphServiceClient GetGraphServiceClient(string[] scopes)
        //{
        //    return GraphServiceClientFactory.GetAuthenticatedGraphClient(async () =>
        //    {
        //        string result = await tokenAcquisition.GetAccessTokenForUserAsync(scopes);
        //        return result;
        //    }, webOptions.GraphApiUrl);
        //}

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
