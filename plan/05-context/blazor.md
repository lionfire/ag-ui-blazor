# Blazor

## Brief Description

Blazor is Microsoft's web UI framework that allows developers to build interactive web applications using C# instead of JavaScript. It comes in two hosting models: Blazor Server (runs on server, uses SignalR) and Blazor WebAssembly (runs in browser via WebAssembly).

## Relevance to Project

**Why this matters for our project**:
- This entire project is built on Blazor - it's the foundation
- We support both Server and WASM hosting models
- Understanding Blazor component model is critical for component development
- Blazor's rendering pipeline affects our streaming performance

**Where it's used in our architecture**:
- All MudBlazor components are Blazor components (.razor files)
- Component lifecycle (OnInitializedAsync, etc.) drives our initialization
- Blazor Server uses SignalR for real-time updates (we leverage this)
- Blazor WASM runs entirely in browser (our offline support layer)

**Impact on implementation**:
- Must follow Blazor component patterns and lifecycle
- StateHasChanged() calls trigger re-renders (performance consideration)
- Dependency injection pattern is Blazor-standard
- Async operations must use proper Blazor patterns

## Interoperability Points

**Integrates with**:
- SignalR: Blazor Server uses SignalR for client-server communication
- .NET Runtime: Blazor WASM requires .NET runtime compiled to WebAssembly
- JavaScript: JSInterop for browser API access
- ASP.NET Core: Blazor Server runs as ASP.NET Core app

**Data flow**:
- Server: User input → SignalR → Server component → Render → SignalR → Browser
- WASM: User input → Component (in browser) → Render → Browser DOM

**APIs and interfaces**:
- ComponentBase: Base class for all components
- IJSRuntime: JavaScript interop
- NavigationManager: Routing and navigation
- HttpClient: HTTP requests (WASM)

## Considerations and Watch Points

### Technical Considerations

**Complexity factors**:
- Two very different hosting models with same component code
- Component lifecycle can be tricky (when to use OnInitialized vs OnParametersSet)
- StateHasChanged() must be called strategically for performance
- SignalR circuit disconnections must be handled gracefully

**Learning curve**:
- Moderate for C# developers, steeper for JavaScript developers
- Component model is similar to React but with C# syntax
- Async patterns require understanding

**Potential conflicts or challenges**:
- Blazor Server: Memory pressure with many concurrent users; each user has server-side circuit
- Blazor WASM: Large initial download size; limited browser APIs
- Both: Component disposal can leak resources if not careful

### Best Practices

**For this project specifically**:
- Use `@code` blocks in .razor files, or separate .razor.cs code-behind
- Call StateHasChanged() only when needed during streaming (not too often)
- Dispose resources in IDisposable.Dispose()
- Use scoped services in Blazor Server (one instance per user circuit)
- Use singleton HttpClient in WASM (shared across app)

**Common patterns**:
- Parameter binding with `[Parameter]`
- Event callbacks with `EventCallback<T>`
- Dependency injection with `[Inject]`
- CSS isolation with `.razor.css` files

### Common Pitfalls

**Watch out for**:
- Forgetting to call StateHasChanged() in async callbacks: Updates won't display
  - **How to avoid**: Always call StateHasChanged() after state changes in async operations
- Memory leaks from event subscriptions: Component disposes but event handler still referenced
  - **How to avoid**: Unsubscribe in Dispose() method
- Accessing JSRuntime before render: Will throw exception
  - **How to avoid**: Use OnAfterRender() for JS interop initialization

**Gotchas**:
- Blazor Server circuits timeout after inactivity (default 3 minutes)
- Parameters are set multiple times (OnParametersSet called frequently)
- @ref only available after component rendered (not in OnInitialized)

### Performance Implications

- Blazor Server: Very low initial load, but requires persistent server connection
- Blazor WASM: Large initial load (~2-5 MB), but runs entirely in browser
- Frequent StateHasChanged() can degrade performance
- Large render trees are expensive to diff (use virtualization for lists)

**Optimization opportunities**:
- Use `@key` directive for list items to optimize diffing
- Implement ShouldRender() to skip unnecessary renders
- Use CSS isolation to scope styles (better performance than inline)
- Lazy load components with `@attribute [Lazy]`

### Security Considerations

- Blazor Server: Server-side code is safe, but validate all user input
- Blazor WASM: All code visible to user (can be decompiled)
- Never expose secrets in WASM (API keys, etc.)
- Use ASP.NET Core authentication for securing endpoints

**Security best practices**:
- Validate input on both client and server
- Use HTTPS always
- Implement proper authentication and authorization
- Don't trust client-side validation alone

### Scalability Factors

- Blazor Server: Scales vertically (more RAM) or horizontally (SignalR backplane with Redis)
- Blazor WASM: Scales infinitely (static files on CDN)
- SignalR circuits consume ~30-100 KB RAM per user

**Scaling strategies**:
- Blazor Server: Use Azure SignalR Service or Redis backplane for multi-server
- Blazor WASM: Serve from CDN, API scales independently
- Consider Hybrid approach: Start with Server, upgrade to WASM for scale

## References

- [Official Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Blazor Performance Best Practices](https://learn.microsoft.com/en-us/aspnet/core/blazor/performance)
- [Blazor Component Lifecycle](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle)
