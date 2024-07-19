using FCDBApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace FCDBApp.Pages.Settings
{
    public class ManageUsersModel : PageModel
    {
        private readonly InspectionContext _context;

        public ManageUsersModel(InspectionContext context)
        {
            _context = context;
        }

        [BindProperty]
        public User User { get; set; }

        public IList<User> Users { get; set; }

        public async Task OnGetAsync()
        {
            Users = await _context.Users.ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                Users = await _context.Users.ToListAsync(); // Ensure Users is populated in case of error
                return Page();
            }

            User.PasswordHash = ComputeSha256Hash(User.PasswordHash);

            _context.Users.Add(User);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var userToUpdate = await _context.Users.FindAsync(User.UserID);
            if (userToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync(
                userToUpdate,
                "user",
                u => u.Username, u => u.FullName, u => u.Email, u => u.Role))
            {
                if (!string.IsNullOrEmpty(User.PasswordHash))
                {
                    userToUpdate.PasswordHash = ComputeSha256Hash(User.PasswordHash);
                }
                await _context.SaveChangesAsync();
                return RedirectToPage();
            }

            Users = await _context.Users.ToListAsync(); // Ensure Users is populated in case of error
            return Page();
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
