using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
  public class UserForRegisterDto
  {
    [Required]
    public string Username { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Your password must be at least 8 characters long (Maximum length is 50)")]
    public string Password { get; set; }
    [Required]
    public string Gender { get; set; }
    [Required]
    public string KnownAs { get; set; }
    [Required]
    public System.DateTime DateOfBirth { get; set; }
    [Required]
    public string City { get; set; }
    [Required]
    public string Country { get; set; }
    [Required]
    public System.DateTime Created { get; set; }
    [Required]
    public System.DateTime LastActive { get; set; }

    public UserForRegisterDto()
    {
      Created = System.DateTime.Now;
      LastActive = System.DateTime.Now;
    }
  }
}