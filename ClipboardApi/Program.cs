using Bugsnag;
using ClipboardApi.Middlewares;
using ClipboardApi.Repositories;
using ClipboardApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IClient>(_ => new Client(builder.Configuration["Bugsnag_ApiKey"]));
builder.Services.AddSingleton(_ => new ClipboardRepository(builder.Configuration["MongoDb_Connection"]!));
builder.Services.AddSingleton(_ => new RecordRepository(builder.Configuration["MongoDb_Connection"]!));
builder.Services.AddSingleton(_ => new EncryptionService(builder.Configuration["EncryptionKey"]!));
builder.Services.AddSingleton(_ => new RabbitMqService(
    builder.Configuration["RabbitMq_Username"]!, 
    builder.Configuration["RabbitMq_Password"]!, 
    builder.Configuration["RabbitMq_Uri"]!
));
builder.Services.AddTransient(s => new ClipboardService(
    s.GetService<ClipboardRepository>()!,
    s.GetService<RecordRepository>()!,
    s.GetService<RabbitMqService>()!,
s.GetService<EncryptionService>()!
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