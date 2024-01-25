using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;
using BlogApp;
using BlogApp.Data;
using BlogApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder();

ConfigureAuthentication(builder);
ConfigureMVC(builder);
ConfigureServices(builder);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

LoadConfiguration(app);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapControllers();
app.UseResponseCompression();
app.UseAuthentication(); 
app.UseAuthorization();

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    Console.WriteLine("Estou em desenvolvimento"); 
}

app.Run();
return;

void LoadConfiguration(WebApplication application)
{
    var smtp = new Configuration.SmtpConfiguration();

    application.Configuration.GetSection("SmtpConfiguration").Bind(smtp);

    Configuration.Smtp = smtp;
}

void ConfigureAuthentication(WebApplicationBuilder webApplicationBuilder)
{
    var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);

    webApplicationBuilder.Services.AddAuthentication(x => 
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });
}

void ConfigureMVC(WebApplicationBuilder builder)
{
    builder.Services.AddMemoryCache();

    builder.Services.AddResponseCompression(options =>
    {
        options.Providers.Add<GzipCompressionProvider>();
    });

    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Optimal;
    });
    
    builder.Services
        .AddControllers()
        .ConfigureApiBehaviorOptions(options => 
    {
        options.SuppressModelStateInvalidFilter = true;
    })
        .AddJsonOptions(x =>
        {
            x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        });
}

void ConfigureServices(WebApplicationBuilder webApplicationBuilder)
{
    webApplicationBuilder.Services.AddDbContext<BlogDataContext>();

    webApplicationBuilder.Services.AddTransient<TokenService>();

    webApplicationBuilder.Services.AddTransient<EmailService>();
}