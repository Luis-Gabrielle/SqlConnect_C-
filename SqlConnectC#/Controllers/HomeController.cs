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

        public IActionResult Index()
        {
            return View(GetUsers());
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

            using var cmd = new MySqlCommand("INSERT INTO user_tbl (username, password) VALUES (@username, @password) ", conn);
            cmd.Parameters.AddWithValue("@username", AddUser.username);
            cmd.Parameters.AddWithValue("@password", AddUser.password);
            cmd.ExecuteNonQuery();
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
