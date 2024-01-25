using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlogApp.Extensions;

public static class ModelStateExtension
{
    public static List<string> GetErrors(this ModelStateDictionary modelState)
    {
        var result = new List<string>();

        foreach(var item in modelState.Values)
        {
            foreach(var itemError in item.Errors)
            {
                result.Add(itemError.ErrorMessage);
            }
        }

        return result;
    }
}