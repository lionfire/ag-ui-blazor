using BlazorWasm.Full.Server;
using BlazorWasm.Full.Shared;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Register the mock chat clients for each agent
builder.Services.AddKeyedSingleton<IChatClient>("assistant", (_, _) => new MockChatClient("assistant"));
builder.Services.AddKeyedSingleton<IChatClient>("coder", (_, _) => new MockChatClient("coder"));
builder.Services.AddKeyedSingleton<IChatClient>("researcher", (_, _) => new MockChatClient("researcher"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();

// API Endpoints for AG-UI Protocol

// GET /api/agents - List available agents
app.MapGet("/api/agents", () =>
{
    var agents = new[]
    {
        new AgentInfo("assistant", "Demo AI Assistant", "A helpful general-purpose AI assistant"),
        new AgentInfo("coder", "Coding Assistant", "Specialized in programming and software development"),
        new AgentInfo("researcher", "Research Assistant", "Specialized in research and analysis")
    };
    return Results.Ok(agents);
});

// POST /api/agents/{agentName}/chat - Non-streaming chat
app.MapPost("/api/agents/{agentName}/chat", async (
    string agentName,
    ChatRequest request,
    IServiceProvider services,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(agentName))
        return Results.BadRequest("Agent name is required");

    var chatClient = services.GetKeyedService<IChatClient>(agentName);
    if (chatClient == null)
        return Results.NotFound($"Agent '{agentName}' not found");

    var messages = request.Messages
        .Select(m => new ChatMessage(
            m.Role == "user" ? ChatRole.User : ChatRole.Assistant,
            m.Content))
        .ToList();

    var completion = await chatClient.CompleteAsync(messages, cancellationToken: ct);
    var responseContent = completion.Message?.Text ?? string.Empty;

    return Results.Ok(new ChatCompletionResponse(
        Id: Guid.NewGuid().ToString(),
        Model: $"mock-{agentName}-v1",
        Content: responseContent
    ));
});

// POST /api/agents/{agentName}/chat/stream - Streaming chat (SSE)
app.MapPost("/api/agents/{agentName}/chat/stream", async (
    string agentName,
    ChatRequest request,
    IServiceProvider services,
    HttpContext context,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(agentName))
    {
        context.Response.StatusCode = 400;
        return;
    }

    var chatClient = services.GetKeyedService<IChatClient>(agentName);
    if (chatClient == null)
    {
        context.Response.StatusCode = 404;
        return;
    }

    context.Response.ContentType = "text/event-stream";
    context.Response.Headers.CacheControl = "no-cache";
    context.Response.Headers.Connection = "keep-alive";

    var messages = request.Messages
        .Select(m => new ChatMessage(
            m.Role == "user" ? ChatRole.User : ChatRole.Assistant,
            m.Content))
        .ToList();

    await foreach (var update in chatClient.CompleteStreamingAsync(messages, cancellationToken: ct))
    {
        if (ct.IsCancellationRequested) break;

        var text = update.Text ?? string.Empty;
        await context.Response.WriteAsync($"data: {text}\n\n", ct);
        await context.Response.Body.FlushAsync(ct);
    }

    await context.Response.WriteAsync("data: [DONE]\n\n", ct);
    await context.Response.Body.FlushAsync(ct);
});

// Fallback to index.html for SPA routing
app.MapFallbackToFile("index.html");

app.Run();

// Request/Response DTOs
public record ChatRequest(List<ChatMessageDto> Messages);
public record ChatMessageDto(string Role, string Content);
public record ChatCompletionResponse(string Id, string Model, string Content);
