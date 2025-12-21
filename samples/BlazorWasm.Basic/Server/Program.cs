using System.Text.Json;
using BlazorWasm.Basic.Server;
using BlazorWasm.Basic.Shared;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Register the mock chat client
builder.Services.AddSingleton<IChatClient, MockChatClient>();

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
        new AgentInfo("assistant", "Demo AI Assistant")
    };
    return Results.Ok(agents);
});

// POST /api/agents/{agentName}/chat - Non-streaming chat
app.MapPost("/api/agents/{agentName}/chat", async (
    string agentName,
    ChatRequest request,
    IChatClient chatClient,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(agentName))
        return Results.BadRequest("Agent name is required");

    var messages = request.Messages
        .Select(m => new ChatMessage(
            m.Role == "user" ? ChatRole.User : ChatRole.Assistant,
            m.Content))
        .ToList();

    var completion = await chatClient.CompleteAsync(messages, cancellationToken: ct);
    var responseContent = completion.Message?.Text ?? string.Empty;

    return Results.Ok(new ChatCompletionResponse(
        Id: Guid.NewGuid().ToString(),
        Model: "mock-model-v1",
        Content: responseContent
    ));
});

// POST /api/agents/{agentName}/chat/stream - Streaming chat (SSE)
app.MapPost("/api/agents/{agentName}/chat/stream", async (
    string agentName,
    ChatRequest request,
    IChatClient chatClient,
    HttpContext context,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(agentName))
    {
        context.Response.StatusCode = 400;
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
