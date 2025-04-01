using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SqlConnectC_.Models;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Generators;
using BCrypt.Net;
namespace SqlConnectC_.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Login(string email, string password)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            using var cmd = new MySqlCommand("SELECT * FROM user_tbl WHERE username = @username", conn);
            cmd.Parameters.AddWithValue("@username", email);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                var storedPassword = reader.GetString("password");

                if (BCrypt.Net.BCrypt.Verify(password, storedPassword))
                {
                    TempData["Message"] = "Login successful!";

                  
                    if (email.Contains("Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Index"); 
                    }
                    else if (email.Contains("User", StringComparison.OrdinalIgnoreCase))
                    {
               
                        return RedirectToAction("UserPage"); 

                    }
                    else
                    {
                        return RedirectToAction("Login"); 
                    }
                }
                else
                {
                  
                    TempData["Message"] = "Incorrect password. Please try again.";
                    return View();
                }
            }

         
         
            return View();
        }

        public IActionResult UserPage()
        {
            return View();
        }

        public IActionResult Index(string searchQuery = "", int page = 1, int pageSize = 5)
        {
            List<User> users = new List<User>();

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string sql = "SELECT * FROM user_tbl";

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    sql += " WHERE username LIKE @search OR userID LIKE @search";
                }

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(searchQuery))
                    {
                        cmd.Parameters.AddWithValue("@search", "%" + searchQuery + "%");
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                userID = reader.GetInt32("userID"),
                                username = reader.GetString("username"),
                                password = reader.GetString("password")
                            });
                        }
                    }
                }
            }

            // Apply Pagination
            var paginatedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(users.Count / (double)pageSize);
            ViewBag.SearchQuery = searchQuery; // To retain search input in UI

            return View(paginatedUsers);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        private string connStr = "Server=localhost;Database=bsit2a;User=root;Password=; ";


        public IActionResult InsertUser(AddUser AddUser)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            // Check if the username already exists
            using var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM user_tbl WHERE username = @username", conn);
            checkCmd.Parameters.AddWithValue("@username", AddUser.username);

            int userCount = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (userCount > 0)
            {
                // Username already exists, show error modal
                TempData["ErrorMessage"] = "Username already exists. Please choose a different one.";
                return RedirectToAction("Index");
            }

            // Hash the password using BCrypt before storing it
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(AddUser.password);

            // Insert the new user
            using var cmd = new MySqlCommand("INSERT INTO user_tbl (username, password) VALUES (@username, @password)", conn);
            cmd.Parameters.AddWithValue("@username", AddUser.username);
            cmd.Parameters.AddWithValue("@password", hashedPassword);
            cmd.ExecuteNonQuery();

            TempData["SuccessMessage"] = "User registered successfully!";
            return RedirectToAction("Index");
        }



        public List<User> GetUsers()
        {
            List<User> user_tbl = new List<User>();
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            using var cmd = new MySqlCommand("SELECT * FROM user_tbl", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                user_tbl.Add(new User
                {
                    userID = reader.GetInt32("userID"),
                    username = reader.GetString("username"),
                    password = reader.GetString("password")
                });
            }

            return user_tbl;
        }
        public IActionResult UpdateUser(User user)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            // Hash the password before updating it
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.password);

            using var cmd = new MySqlCommand("UPDATE user_tbl SET username = @username, password = @password WHERE userID = @userID", conn);
            cmd.Parameters.AddWithValue("@username", user.username);
            cmd.Parameters.AddWithValue("@password", hashedPassword);
            cmd.Parameters.AddWithValue("@userID", user.userID);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                TempData["Message"] = "User updated successfully!";
            }
            else
            {
                TempData["Message"] = "Failed to update user.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult deleteUser(int userID)
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            using var cmd = new MySqlCommand("DELETE FROM user_tbl WHERE userID = @userID", conn);
            cmd.Parameters.AddWithValue("@userID", userID);

            int rowsAffected = cmd.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                TempData["Message"] = "User deleted successfully!";
            }
            else
            {
                TempData["Message"] = "Failed to delete user.";
            }

            return RedirectToAction("Index");
        }

    }
}
