using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
builder.Environment.EnvironmentName = Environments.Development;

var configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
if (!File.Exists(configPath))
{
    configPath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
}

builder.Configuration.AddJsonFile(configPath, optional: false, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<MongoDB.Driver.IMongoClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new MongoDB.Driver.MongoClient(config["MongoDB:ConnectionString"]);
});

builder.Services.AddScoped<MongoDB.Driver.IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<MongoDB.Driver.IMongoClient>();
    var config = sp.GetRequiredService<IConfiguration>();
    return client.GetDatabase(config["MongoDB:DatabaseName"]);
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowNextJs",
        policy =>
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    );
});

builder
    .Services.AddHttpClient<url.Services.NestRipClient>(client =>
    {
        var baseUrl = builder.Configuration["NestRip:BaseUrl"] ?? "https://nest.rip/api/";
        client.BaseAddress = new Uri(baseUrl);
    })
    .ConfigurePrimaryHttpMessageHandler(
        (IServiceProvider sp) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var resolutionIp = config["NestRip:ResolutionIp"];
            if (string.IsNullOrEmpty(resolutionIp))
            {
                return new SocketsHttpHandler();
            }
            return new SocketsHttpHandler
            {
                ConnectCallback = async (context, cancellationToken) =>
                {
                    var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        var ip = IPAddress.Parse(resolutionIp);
                        await socket.ConnectAsync(ip, context.DnsEndPoint.Port, cancellationToken);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                },
            };
        }
    );

var serverPort = builder.Configuration.GetValue<int>("Server:Port", 4616);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(serverPort);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowNextJs");
app.UseAuthorization();
app.MapControllers();

app.Run();