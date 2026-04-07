using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using ScientificActivityBusinessLogics.BusinessLogics;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.StoragesContracts;
using ScientificActivityDatabaseImplement.Implements;
using ScientificActivityParsers.Interfaces;
using ScientificActivityParsers.Parsers;
using ScientificActivityParsers.Services;
using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddLog4Net("log4net.config");

// Add services to the container.
builder.Services.AddTransient<IConferenceStorage, ConferenceStorage>();
builder.Services.AddTransient<IGrantStorage, GrantStorage>();
builder.Services.AddTransient<IJournalStorage, JournalStorage>();
builder.Services.AddTransient<IPublicationStorage, PublicationStorage>();
builder.Services.AddTransient<IResearcherInterestStorage, ResearcherInterestStorage>();
builder.Services.AddTransient<IResearcherStorage, ResearcherStorage>();

builder.Services.AddTransient<IConferenceLogic, ConferenceLogic>();
builder.Services.AddTransient<IGrantLogic, GrantLogic>();
builder.Services.AddTransient<IJournalLogic, JournalLogic>();
builder.Services.AddTransient<IPublicationLogic, PublicationLogic>();
builder.Services.AddTransient<IResearcherInterestLogic, ResearcherInterestLogic>();
builder.Services.AddTransient<IResearcherLogic, ResearcherLogic>();

builder.Services.AddHttpClient<IGrantParser, RscfGrantParser>();
builder.Services.AddHttpClient<IConferenceParser, NaKonferenciiConferenceParser>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);

    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add("User-Agent",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36");
    client.DefaultRequestHeaders.Add("Accept",
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
    client.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
    client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
});
builder.Services.AddScoped<IImportService, ImportService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ScientificActivity", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ScientificActivity v1"));
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();
