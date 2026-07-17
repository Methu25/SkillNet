using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SkillNet.Application.DTOs;
using SkillNet.Application.Interfaces;
using SkillNet.WebApi.Controllers;

namespace SkillNet.WebApi.Filters
{
    public class RegistrationEmailFilter : IAsyncActionFilter
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<RegistrationEmailFilter> _logger;

        public RegistrationEmailFilter(
            IEmailService emailService,
            ILogger<RegistrationEmailFilter> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var isRegistration = context.Controller is AuthController &&
                string.Equals(context.ActionDescriptor.RouteValues["action"], "Register", StringComparison.Ordinal);
            var request = isRegistration && context.ActionArguments.TryGetValue("request", out var argument)
                ? argument as RegisterRequest
                : null;

            var executed = await next();
            if (request == null || executed.Exception != null ||
                executed.Result is not ObjectResult result ||
                (result.StatusCode ?? StatusCodes.Status200OK) >= StatusCodes.Status300MultipleChoices)
            {
                return;
            }

            try
            {
                var delivery = await _emailService.SendAsync(
                    request.Email,
                    "Welcome to SkillNet",
                    $"Welcome {request.FirstName}!\n\nYour SkillNet registration was successful.",
                    "Registration Welcome");
                if (!delivery.Succeeded)
                {
                    _logger.LogWarning(
                        "Registration succeeded but welcome email was not delivered for {Email}: {Reason}",
                        request.Email,
                        delivery.ErrorMessage);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Registration succeeded but the welcome email failed for {Email}.",
                    request.Email);
            }
        }
    }
}
