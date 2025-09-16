using Blogtify.Client;
using Blogtify.Client.Theming;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton<IHttpContextProxy, WebAssemblyHttpContextProxy>();

builder.Services.AddCommonServices();

await builder.Build().RunAsync();
