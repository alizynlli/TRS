using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using TRS.Data.Models;

namespace TRS.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoleList = await _userManager.GetRolesAsync(user);

            if (userRoleList != null && userRoleList.Any())
            {
                var userRole = userRoleList[0];

                if (userRole == "Client") return RedirectToAction("ClientTaskList", "ClientUser");
                if (userRole == "Personnel") return RedirectToAction("TaskList", "Personnel");
                if (userRole == "Super Admin") return RedirectToAction("TaskList", "Administration");
            }

            return RedirectToAction("AccessDenied", "Administration");
        }
    }
}
