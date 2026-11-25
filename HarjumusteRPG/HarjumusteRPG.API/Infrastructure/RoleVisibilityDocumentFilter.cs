using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HarjumusteRPG.API.Infrastructure;

/// <summary>Shows in Swagger only the endpoints that the current user (by JWT role) may call.</summary>
public class RoleVisibilityDocumentFilter(IHttpContextAccessor accessor) : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var user = accessor.HttpContext?.User;

        foreach (var api in context.ApiDescriptions)
        {
            var metadata = api.ActionDescriptor.EndpointMetadata;
            var rules = metadata.OfType<IAuthorizeData>().ToList();
            var anonymous = metadata.OfType<IAllowAnonymous>().Any();

            if (CanSee(user, rules, anonymous))
                continue;

            var path = "/" + api.RelativePath!.Split('?')[0].TrimEnd('/');
            if (!swaggerDoc.Paths.TryGetValue(path, out var item))
                continue;

            item.Operations?.Remove(new HttpMethod(api.HttpMethod!));
            if (item.Operations is { Count: 0 })
                swaggerDoc.Paths.Remove(path);

            // remove empty section headers (for example "Streaks" for a normal user)
            if (swaggerDoc.Tags is not null)
            {
                var usedTags = swaggerDoc.Paths.Values
                    .Where(p => p.Operations is not null)
                    .SelectMany(p => p.Operations!.Values)
                    .Where(o => o.Tags is not null)
                    .SelectMany(o => o.Tags!)
                    .Select(t => t.Name)
                    .ToHashSet();

                foreach (var tag in swaggerDoc.Tags.Where(t => !usedTags.Contains(t.Name)).ToList())
                    swaggerDoc.Tags.Remove(tag);
            }
        }
    }

    private static bool CanSee(ClaimsPrincipal? user, List<IAuthorizeData> rules, bool anonymous)
    {
        // register and login: only for guests, a logged-in user does not need them
        if (anonymous || rules.Count == 0)
            return user?.Identity?.IsAuthenticated != true;

        // guest: nothing else
        if (user?.Identity?.IsAuthenticated != true) return false;

        // the Service account (Worker) sees only the Service endpoints
        if (user.IsInRole("Service"))
            return rules.Any(r => SplitRoles(r.Roles).Contains("Service"));

        // User and Admin: every role restriction must be satisfied
        return rules.All(r =>
        {
            var roles = SplitRoles(r.Roles);
            return roles.Count == 0 || roles.Any(user.IsInRole);
        });
    }

    private static List<string> SplitRoles(string? roles) =>
        (roles ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}