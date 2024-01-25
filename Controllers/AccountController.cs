using System.Data.Common;
using System.Text.RegularExpressions;
using BlogApp.Data;
using BlogApp.Extensions;
using BlogApp.Models;
using BlogApp.Services;
using BlogApp.ViewModels.Accounts;
using BlogApp.ViewModels.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;

namespace BlogApp.Controllers;

[ApiController]
[Route("v1/accounts")]
public class AccountController : ControllerBase
{
    [HttpPost("")]
    public async Task<IActionResult> Post(
        [FromBody] RegisterViewModel model,
        [FromServices] BlogDataContext context,
        [FromServices] EmailService emailService)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            Slug = model.Email.Replace("@", "-").Replace(".", "-")
        };

        var password = PasswordGenerator.Generate(25, true, true);

        user.PasswordHash = PasswordHasher.Hash(password);

        try
        {

            emailService.Send(user.Name,
                user.Email,
                $"Bem vindo {user.Name} !",
                $"Sua senha é: <strong>{password}</strong>");

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            return Ok(new ResultViewModel<dynamic>(new
            {
                user = user.Email, 
                password
            }));
        }

        catch (DbException)
        {
            return StatusCode(400, new ResultViewModel<string>("05x99 - Este E-mail já está cadastrado"));
        }

        catch (Exception ex) 
        {
            Console.WriteLine(ex.Message);

            return StatusCode(500, new ResultViewModel<string>("05x04 - Falha interna no servidor"));
        }
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginViewModel model,
        [FromServices] BlogDataContext context,
        [FromServices] TokenService tokenService)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        var user = await context
            .Users
            .AsNoTracking()
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Email == model.Email);

        if (user == null)
            return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

        if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
            return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

        try
        {
            var token = tokenService.GenerateToken(user);

            return Ok(new ResultViewModel<string>(token, null));
        }
        catch (Exception e)
        {
            return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
        }
    }

    [Authorize]
    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(
        [FromBody] UploadImageViewModel model,
        [FromServices] BlogDataContext context)
    {
        var fileName = $"{Guid.NewGuid().ToString()}.jpg";

        var data = new Regex(@"^data:image\/[a-z]+;base64,")
            .Replace(model.Base64Image, "");

        var bytes = Convert.FromBase64String(data);

        try
        {
            await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}",bytes);
        }
        catch (Exception)
        {
            return StatusCode(500, new ResultViewModel<string>("05x04 - Falha interna no servidor"));
        }

        var user = await context
            .Users
            .FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

        if (user is null)
        {
            return NotFound(new ResultViewModel<Category>("Usuário não encontrado"));
        }

        user.Image = $"https://localhost:0000/images/{fileName}";

        try
        {
            context.Users.Update(user);

            await context.SaveChangesAsync();
        }
        catch (Exception)
        {
            return StatusCode(500, new ResultViewModel<string>("05x04 - Falha interna no servidor"));
        }

        return Ok(new ResultViewModel<string>("Imagem alterada com sucesso!"));
    }
}