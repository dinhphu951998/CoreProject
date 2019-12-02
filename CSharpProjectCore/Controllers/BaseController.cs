using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CSharpProjectCore.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CSharpProjectCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        public ExtensionSettings extensionSettings { get; }

        public ClaimsPrincipal currentUser => extensionSettings.httpContextAccessor.HttpContext.User;

        public string CurrentUserId => currentUser.FindFirstValue(ClaimTypes.NameIdentifier);

        public string CurrentUsername => currentUser.FindFirstValue(ClaimTypes.Name);


        public BaseController(ExtensionSettings extensionSettings)
        {
            this.extensionSettings = extensionSettings;
            string name = currentUser.Identity.Name;
            List<Claim> claims = currentUser.Claims.ToList();
            Claim claim = currentUser.Claims.FirstOrDefault(c => c.Value == ClaimTypes.NameIdentifier);
        }

        protected dynamic ExecuteInMonitoring(Func<dynamic> function)
        {
            dynamic result;
            try
            {
                result = function();
            }
            catch (BaseException ex)
            {
                //var err = new Dictionary<string, IEnumerable<string>>();
                //err.Add("General", new List<string> { ex.Message });
                return ex;
            }
            catch (Exception ex)
            {
                //var err = new Dictionary<string, IEnumerable<string>>();
                //err.Add("General", new List<string> { ex.ToString() });
                return ex;
            }
            return result;
        }

        protected async Task<dynamic> ExecuteInMonitoring(Func<Task<dynamic>> function)
        {
            dynamic result;
            try
            {
                result = await function();
            }
            catch (BaseException ex)
            {
                var err = new Dictionary<string, IEnumerable<string>>();
                err.Add("General", new List<string> { ex.Message });
                return ex;
            }
            catch (Exception ex)
            {
                var err = new Dictionary<string, IEnumerable<string>>();
                err.Add("General", new List<string> { ex.ToString() });
                return ex;
            }
            return result;
        }

    }
}