namespace Modeller.McpServer.McpValidatorServer;

public static class McpEndpoints
{
    //public static IEndpointRouteBuilder MapMcpValidator(this IEndpointRouteBuilder app)
    //{
    //    app.MapPost("/mcp/validate", async (
    //        [FromBody] ValidationRequest request,
    //        IMcpModelValidator validator,
    //        ILoggerFactory loggerFactory,
    //        CancellationToken cancellationToken) =>
    //    {
    //        var results = await validator.ValidateAsync(request.Path, cancellationToken);
    //        return Results.Ok(new ValidationResponse(results));
    //    });

    //    return app;
    //}
}
