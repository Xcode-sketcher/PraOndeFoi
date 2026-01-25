using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using PraOndeFoi.Data;
using PraOndeFoi.Repository;
using PraOndeFoi.Services;
using Supabase;
using Quartz;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddSingleton<Client>(provider =>
{
    var url = builder.Configuration["Supabase:Url"];
    var key = builder.Configuration["Supabase:AnonKey"];
    if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
    {
        throw new InvalidOperationException("Configuração do Supabase ausente.");
    }
    return new Client(url, key);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024;
});

const long uploadMaxBytes = 49L * 1024 * 1024;

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = uploadMaxBytes;
    options.ValueLengthLimit = (int)uploadMaxBytes;
    options.MultipartHeadersLengthLimit = 64 * 1024;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = uploadMaxBytes;
});

builder.Services.AddScoped<IContaService, ContaService>();
builder.Services.AddScoped<IFinancasService, FinancasService>();
builder.Services.AddScoped<IExportacaoService, ExportacaoService>();
builder.Services.AddScoped<IImportacaoService, ImportacaoService>();
builder.Services.AddScoped<IContaCacheService, ContaCacheService>();
builder.Services.AddScoped<IAnexoStorageService, SupabaseAnexoStorageService>();
builder.Services.AddScoped<IContaRepository, ContaRepository>();
builder.Services.AddScoped<IFinancasRepository, FinancasRepository>();

var intervaloRecorrencias = builder.Configuration.GetValue<int?>("Jobs:Recorrencias:IntervalMinutes") ?? 60;
intervaloRecorrencias = Math.Max(1, intervaloRecorrencias);

builder.Services.AddQuartz(options =>
{
    var jobKey = new JobKey("RecorrenciasJob");
    options.AddJob<RecorrenciasJob>(job => job.WithIdentity(jobKey));

    options.AddTrigger(trigger => trigger
        .ForJob(jobKey)
        .WithIdentity("RecorrenciasJob-trigger")
        .StartNow()
        .WithSimpleSchedule(schedule => schedule
            .WithInterval(TimeSpan.FromMinutes(intervaloRecorrencias))
            .RepeatForever()));
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Authentication:Authority"];
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Authentication:ValidIssuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Authentication:ValidAudience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

