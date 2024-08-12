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
        private readonly string _jsonFilePath;
        public BlogController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _jsonFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "DataStorage", "BlogStorage.json");
        }
        private async Task<List<Blog>> LoadBlogsAsync()
        {
            var jsonString = await System.IO.File.ReadAllTextAsync(_jsonFilePath);
            return JsonSerializer.Deserialize<List<Blog>>(jsonString) ?? new List<Blog>();
        }

        [HttpGet("GetBlogs")]
        public async Task<IActionResult> GetBlogs(string searchTerm = "", int pageNumber = 1, int pageSize = 10)
        {
            if (!System.IO.File.Exists(_jsonFilePath))
            {
                return NotFound("Blog Storage file not found.");
            }

            var blogs = await LoadBlogsAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                blogs = blogs.Where(b => b.Username.ToLower().Contains(searchTerm)
                                      || b.Text.ToLower().Contains(searchTerm))
                             .ToList();
            }

            var pagedBlogs = blogs
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalItems = blogs.Count;
            var totalPages = (int)System.Math.Ceiling(totalItems / (double)pageSize);

            var response = new
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                Data = pagedBlogs
            };

            return Ok(response);
        }


        [HttpGet("GetBlog/{id}")]
        public async Task<IActionResult> GetBlog(int id)
        {
            if (!System.IO.File.Exists(_jsonFilePath))
            {
                return NotFound("Blog Storage file not found.");
            }
            if (id < 1)
            {
                return BadRequest("Invalid blog Id.");
            }
            var blogs = await LoadBlogsAsync();

            var blog = blogs.Find(item => item.Id == id);
            return Ok(blog);

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

            var jsonString = await System.IO.File.ReadAllTextAsync(_jsonFilePath);
            var blogs = JsonSerializer.Deserialize<List<Blog>>(jsonString) ?? new List<Blog>();

            newBlog.Id = blogs.Count > 0 ? blogs.Max(item => item.Id) + 1 : 1;
            blogs.Add(newBlog);

            await System.IO.File.WriteAllTextAsync(_jsonFilePath, JsonSerializer.Serialize(blogs));

            return CreatedAtAction(nameof(GetBlogs), new { id = newBlog.Id }, newBlog);
        }




        [HttpPut("UpdateBlog/{id}")]
        public async Task<IActionResult> UpdateBlog(int id, [FromBody] Blog updatedBlog)
        {
            if (updatedBlog == null || id != updatedBlog.Id || id < 1)
            {
                return BadRequest("Invalid blog data or ID mismatch.");
            }

            if (!System.IO.File.Exists(_jsonFilePath))
            {
                return NotFound("Blog data file not found.");
            }

            var jsonString = await System.IO.File.ReadAllTextAsync(_jsonFilePath);
            var blogs = JsonSerializer.Deserialize<List<Blog>>(jsonString) ?? new List<Blog>();
            var blog = blogs.Find(b => b.Id == id);

            if (blog == null)
            {
                return NotFound("Blog not found.");
            }

            blog.Username = updatedBlog.Username;
            blog.DateCreated = updatedBlog.DateCreated;
            blog.Text = updatedBlog.Text;

            await System.IO.File.WriteAllTextAsync(_jsonFilePath, JsonSerializer.Serialize(blogs));

            return NoContent();


        }

        [HttpDelete("DeleteBlog/{id}")]
        public async Task<IActionResult> DeleteBlog(int id)
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

    }
}

