using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels.Accounts;

public class RegisterViewModel
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string Name { get; set;}
    
    [Required(ErrorMessage = "O e-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "e-mail inválido")]
    public string Email { get; set; }
    
    
}