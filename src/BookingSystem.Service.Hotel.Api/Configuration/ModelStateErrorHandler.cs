using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Service.Hotel.Api.Configuration
{
    public class ModelStateErrorHandler
    {
        public static BadRequestObjectResult HandleInvalidModelState(ActionContext context)
        {
            var errors = new Dictionary<string, string[]>();
            foreach (var entry in context.ModelState)
            {
                if (entry.Value.Errors.Count > 0)
                {
                    var hasFieldErrors = context.ModelState.Any(e => e.Key.StartsWith("$") && e.Value.Errors.Count > 0);

                    bool IsGenericParam(string key) => !key.StartsWith("$") && !key.Contains(".");

                    errors = context.ModelState
                       .Where(kvp => kvp.Value != null && kvp.Value.Errors.Count > 0)
                       .Where(kvp => !(IsGenericParam(kvp.Key) && hasFieldErrors))
                       .ToDictionary(
                           kvp => CleanFieldName(kvp.Key),
                           kvp => kvp.Value!.Errors
                               .Select(e => NormalizeErrorMessage(CleanFieldName(kvp.Key), e.ErrorMessage))
                               .ToArray()
                       );
                }
            }
            var response = new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title = "Validation Failed",
                status = 400,
                errors
            };
            return new BadRequestObjectResult(response);
        }

        private static string CleanFieldName(string key)
        {
            return key.Replace("$.", "");
        }

        private static string NormalizeErrorMessage(string field, string message)
        {
            if (message.Contains("could not be converted to") || message.Contains("is not valid"))
            {
                return field.ToLower() switch
                {
                    "status" => "Status must be 'Active', 'Inactive', or 'UnderMaintenance'",
                    "starrating" => "Star rating must be between 1 and 5",
                    _ => $"Invalid value for {field}"
                };
            }
            return message;
        }
    }
}
