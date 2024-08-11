using System.ComponentModel.DataAnnotations;

namespace BlogManagementWebApi.Model
{
public class Blog
{
    [Required(ErrorMessage = "Id is required")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Creation date is required")]
    public DateTime DateCreated { get; set; }

    [Required(ErrorMessage = "Text is required")]
    public string Text { get; set; }
}

}
