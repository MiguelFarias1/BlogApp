using BlogApp.Extensions;
using BlogApp.ViewModels.Categories;
using Microsoft.Extensions.Caching.Memory;
using Exception = System.Exception;

namespace BlogApp.Controllers;

using Data;
using Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("v1/categories")]
public class CategoryController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Category>>> GetAsync(
        [FromServices] IMemoryCache cache,
        [FromServices] BlogDataContext context)
    {
        try
        {
            var categories = cache.GetOrCreate("CategoriesCache", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                return GetCategories(context);
            });

            return Ok(new ResultViewModel<List<Category>>(categories));
        }

        catch (Exception e)
        {
            return StatusCode(500, new ResultViewModel<List<Category>>("Falha interna no servidor"));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetByIdAsync([FromRoute] int id,
        [FromServices] BlogDataContext context)
    {
        try
        {
            var category = await context
            .Categories
            .FirstOrDefaultAsync(x => x.Id == id);

            if (category is null) 
                return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado"));

            return Ok(category);
        }

        catch (Exception e)
        {
            return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor"));
        }

    }

    [HttpPost("")]
    public async Task<ActionResult> PostAsync([FromBody] EditorCategoryViewModel model,
                            [FromServices] BlogDataContext context)
    {

        if(!ModelState.IsValid) 
            return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));

        try
        {
            var category = new Category()
            {
                Id = 0,
                Name = model.Name,
                Slug = model.Slug.ToLower(),
            };

            await context
            .Categories
            .AddAsync(category);

            await context
            .SaveChangesAsync();

            return Created($"v1/categories/{category.Id}", model);
        }

        catch (DbUpdateException e)
        {
            return StatusCode(500, new ResultViewModel<Category>("Não foi possível incluir a categoria"));
        }

        catch (Exception e)
        {
            return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor"));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> PutAsync([FromRoute] int id,
        [FromBody] EditorCategoryViewModel model,
        [FromServices] BlogDataContext context)
    {

        if (ModelState.IsValid)
        {
            return BadRequest();
        }

        try
        {
            var category = await context
            .Categories
            .FirstOrDefaultAsync(x => x.Id == id);

            if (category is null) return NotFound();

            category.Name = model.Name;
            category.Slug = model.Slug;

            context.Categories.Update(category);
            await context.SaveChangesAsync();

            return Ok(category);
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, new ResultViewModel<Category>("Não foi possível atualizar a categoria"));
        }

        catch (Exception e)
        {
            return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor"));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int id, [FromServices] BlogDataContext context)
    {
        try
        {
            var category = await context
            .Categories
            .FirstOrDefaultAsync(x => x.Id == id);

            if (category is null) return NotFound();

            context.Remove(category);

            await context.SaveChangesAsync();

            return new NoContentResult();
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, new ResultViewModel<Category>("Não foi possível excluir a categoria"));
        }

        catch (Exception e)
        {
            return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor"));
        }
    }
    
    private List<Category> GetCategories([FromServices] BlogDataContext context)
    {
        return context.Categories.ToList();
    }
}