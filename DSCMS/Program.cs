using DSCMS;

var builder = WebApplication.CreateBuilder(args);

// Create startup instance and configure services (keeping your existing logic)
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline (keeping your existing logic)
startup.Configure(app, app.Environment);

app.Run();
