using BlogManagementWebApi.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BlogManagementWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BlogController> _logger;
        private readonly string _jsonFilePath;
        public BlogController(IWebHostEnvironment webHostEnvironment, ILogger<BlogController> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _jsonFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "DataStorage", "BlogStorage.json");
        }

        [HttpGet("GetBlogs")]
        public async Task<IActionResult> GetBlogs()
        {
            if (!System.IO.File.Exists(_jsonFilePath))
            {
                return NotFound("Blog Storage file not found.");
            }

            try
            {
                var jsonString = await System.IO.File.ReadAllTextAsync(_jsonFilePath);

                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    _logger.LogInformation("Blog data file is empty.");
                    return Ok(new List<Blog>()); 
                }

                var blogs = JsonSerializer.Deserialize<List<Blog>>(jsonString) ?? new List<Blog>();

                return Ok(blogs);
            }
            catch (JsonException ex)
            {
                return StatusCode(500, "Error processing blog data. Please try again later.");
            }
            catch (IOException ex)
            {
                return StatusCode(500, "Error accessing the data file. Please try again later.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }



        [HttpPost("CreateBlog")]
        public async Task<IActionResult> CreateBlog([FromBody] Blog newBlog)
        {
            if (newBlog == null)
            {
                return BadRequest("Blog data is Empty.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var jsonString = await System.IO.File.ReadAllTextAsync(_jsonFilePath);
                var blogs = JsonSerializer.Deserialize<List<Blog>>(jsonString) ?? new List<Blog>();

                newBlog.Id = blogs.Count > 0 ? blogs.Max(item => item.Id) + 1 : 1;
                blogs.Add(newBlog);

                await System.IO.File.WriteAllTextAsync(_jsonFilePath, JsonSerializer.Serialize(blogs));

                return CreatedAtAction(nameof(GetBlogs), new { id = newBlog.Id }, newBlog);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }


        [HttpPut("UpdateBlog/{id}")]
        public async Task<IActionResult> UpdateBlog(int id, [FromBody] Blog updatedBlog)
        {
            if (updatedBlog == null || id != updatedBlog.Id)
            {
                return BadRequest("Invalid blog data or ID mismatch.");
            }

            try
            {
                if (!System.IO.File.Exists(_jsonFilePath))
                {
                    return NotFound("Blog data file not found.");
                }

                var jsonString = await System.IO.File.ReadAllTextAsync(_jsonFilePath);
                var blogs = JsonSerializer.Deserialize<List<Blog>>(jsonString) ?? new List<Blog>();
                var blog = blogs.Find(b => b.Id == id);

                if (blog == null)
                {
                    return NotFound($"Blog with ID {id} not found.");
                }

                blog.Username = updatedBlog.Username;
                blog.DateCreated = updatedBlog.DateCreated;
                blog.Text = updatedBlog.Text;

                await System.IO.File.WriteAllTextAsync(_jsonFilePath, JsonSerializer.Serialize(blogs));

                return NoContent(); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpDelete("DeleteBlog/{id}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            try
            {
                if (!System.IO.File.Exists(_jsonFilePath))
                {
                    return NotFound("Blog data file not found.");
                }

                var jsonString = await System.IO.File.ReadAllTextAsync(_jsonFilePath);
                var blogs = JsonSerializer.Deserialize<List<Blog>>(jsonString) ?? new List<Blog>();
                var blog = blogs.Find(b => b.Id == id);

                if (blog == null)
                {
                    return NotFound($"Blog with ID {id} not found.");
                }

                blogs.Remove(blog);
                await System.IO.File.WriteAllTextAsync(_jsonFilePath, JsonSerializer.Serialize(blogs));

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }


    }
}
