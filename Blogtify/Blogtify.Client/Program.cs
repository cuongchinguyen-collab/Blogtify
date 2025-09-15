using Blogtify.Client;
using Blogtify.Client.Theming;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton<IHttpContextProxy, WebAssemblyHttpContextProxy>();

builder.Services.AddCommonServices();

await builder.Build().RunAsync();
