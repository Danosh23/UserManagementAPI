using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        // In-memory user store for demonstration purposes
        private static readonly ConcurrentBag<User> Users = new();

        // GET: api/users
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            try
            {
                // ToList() for thread-safe enumeration
                return Ok(Users.ToList());
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving users.", Details = ex.Message });
            }
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            try
            {
                var user = Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                    return NotFound(new { Message = $"User with ID {id} not found." });
                return Ok(user);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving the user.", Details = ex.Message });
            }
        }

        // POST: api/users
        [HttpPost]
        public ActionResult<User> CreateUser([FromBody] User user)
        {
            try
            {
                if (user == null)
                    return BadRequest(new { Message = "User data is required." });

                // Validate model
                var validationResults = new List<ValidationResult>();
                var context = new ValidationContext(user, null, null);
                if (!Validator.TryValidateObject(user, context, validationResults, true))
                    return BadRequest(validationResults);

                // Check for duplicate email (case-insensitive)
                if (Users.Any(u => u.Email.Equals(user.Email, System.StringComparison.OrdinalIgnoreCase)))
                    return Conflict(new { Message = "A user with this email already exists." });

                // Optimize ID assignment by using Interlocked for thread safety
                int newId = Users.Any() ? Users.Max(u => u.Id) + 1 : 1;
                user.Id = newId;
                Users.Add(user);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the user.", Details = ex.Message });
            }
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
        {
            try
            {
                if (updatedUser == null)
                    return BadRequest(new { Message = "User data is required." });

                // Validate model
                var validationResults = new List<ValidationResult>();
                var context = new ValidationContext(updatedUser, null, null);
                if (!Validator.TryValidateObject(updatedUser, context, validationResults, true))
                    return BadRequest(validationResults);

                var user = Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                    return NotFound(new { Message = $"User with ID {id} not found." });

                // Check for duplicate email (excluding current user, case-insensitive)
                if (Users.Any(u => u.Email.Equals(updatedUser.Email, System.StringComparison.OrdinalIgnoreCase) && u.Id != id))
                    return Conflict(new { Message = "A user with this email already exists." });

                user.Name = updatedUser.Name;
                user.Email = updatedUser.Email;
                user.Role = updatedUser.Role;
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the user.", Details = ex.Message });
            }
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                var user = Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                    return NotFound(new { Message = $"User with ID {id} not found." });

                // Remove user safely from ConcurrentBag
                var usersList = Users.ToList();
                if (usersList.Remove(user))
                {
                    // Rebuild the ConcurrentBag without the deleted user
                    var newBag = new ConcurrentBag<User>(usersList);
                    while (!Users.IsEmpty)
                        Users.TryTake(out _);
                    foreach (var u in newBag)
                        Users.Add(u);
                }
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the user.", Details = ex.Message });
            }
        }
    }

    // Enhanced User model with validation
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name can't be longer than 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [StringLength(50, ErrorMessage = "Role can't be longer than 50 characters.")]
        public string Role { get; set; }
    }
}