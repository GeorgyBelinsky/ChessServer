using ChessServer.Hubs;
using ChessServer.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
       policy =>
       {
           policy.WithOrigins("http://localhost:5173")
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials();
       });
});

builder.Services.AddSingleton<IDictionary<string, UserConnection>>(opts => new Dictionary<string, UserConnection>());

builder.Services.AddSignalR();


var app = builder.Build();

app.UseCors();

app.MapHub<ChessRoomHub>("/game-page");
app.Run();