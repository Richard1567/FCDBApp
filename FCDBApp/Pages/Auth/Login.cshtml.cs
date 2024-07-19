using System.ComponentModel.DataAnnotations;
using FCDBApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FCDBApp.Pages.Auth
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            public string Password { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var loginModel = new LogModel
            {
                Username = Input.Username,
                Password = Input.Password
            };

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Username", loginModel.Username),
                new KeyValuePair<string, string>("Password", loginModel.Password)
            });

            var client = new HttpClient();
            var response = await client.PostAsync("https://localhost:7093/Auth/Login", content);
            if (response.IsSuccessStatusCode)
            {
                // Handle success, e.g., redirect to a protected page
                return RedirectToPage("/Index");
            }
            else
            {
                // Handle failure
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }
    }
}
