using System;
using MDR_FuiPortal.Server;
using MDR_FuiPortal.Shared;
using Microsoft.Fast.Components.FluentUI;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var options = new WebApplicationOptions { WebRootPath = "wwwroot" };
var builder = WebApplication.CreateBuilder(options);

// ------------------------------------------------------------
// Services
// ------------------------------------------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddFluentUIComponents();

builder.Services.AddSingleton<ICredentials, Credentials>();
builder.Services.AddSingleton<ILookUpRepo, LookUpRepo>();
builder.Services.AddSingleton<ITreeRepo, TreeRepo>();

builder.Services.Configure<MailConfigModel>(
    builder.Configuration.GetSection(MailConfigModel.SectionName)
);

builder.Services.AddScoped<IObjectRepo, ObjectRepo>();
builder.Services.AddScoped<IStudyRepo, StudyRepo>();
builder.Services.AddScoped<IMailRepo, MailRepo>();

builder.Services.AddSwaggerGen();

// ------------------------------------------------------------
// OpenTelemetry (Option A - SDK in code)
// ------------------------------------------------------------
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r =>
    {
        var serviceName =
            Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "mdr-fuiportal";

        r.AddService(serviceName: serviceName)
         .AddEnvironmentVariableDetector();
    })
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    })
    .WithMetrics(m =>
    {
        m.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter();
    });

var app = builder.Build();

// ------------------------------------------------------------
// HTTP pipeline
// ------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
