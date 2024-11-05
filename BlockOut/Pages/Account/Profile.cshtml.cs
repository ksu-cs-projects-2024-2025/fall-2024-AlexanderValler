// File: Pages/Account/Profile.cshtml.cs

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace BlockOut.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public string UserName { get; set; }
        public string Email { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                UserName = user.UserName;
                Email = user.Email;
            }
        }
    }
}
