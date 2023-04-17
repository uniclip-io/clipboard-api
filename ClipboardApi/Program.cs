using Bugsnag;
using ClipboardApi.Middlewares;
using ClipboardApi.Repositories;
using ClipboardApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IClient>(_ => new Client(builder.Configuration["Bugsnag:ApiKey"]));
builder.Services.AddSingleton(_ => new ClipboardRepository(builder.Configuration["MongoDb:Connection"]!));
builder.Services.AddSingleton(_ => new RecordRepository(builder.Configuration["MongoDb:Connection"]!));
builder.Services.AddSingleton(_ => new RabbitMqService(
    builder.Configuration["RabbitMq:Username"]!, 
    builder.Configuration["RabbitMq:Password"]!, 
    builder.Configuration["RabbitMq:Uri"]!
));
builder.Services.AddTransient(s => new ClipboardService(
    s.GetService<ClipboardRepository>()!,
    s.GetService<RecordRepository>()!,
    s.GetService<RabbitMqService>()!
));

var app = builder.Build();
app.Services.GetService<ClipboardService>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<HttpExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();