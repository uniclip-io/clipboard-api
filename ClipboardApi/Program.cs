using ClipboardApi.Repositories;
using ClipboardApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(_ => new ClipboardRepository(builder.Configuration["ConnectionStrings:MongoDb"]!));
builder.Services.AddSingleton(_ => new RecordRepository(builder.Configuration["ConnectionStrings:MongoDb"]!));
builder.Services.AddSingleton(_ => new RabbitMqService(builder.Configuration["ConnectionStrings:RabbitMq"]!));
builder.Services.AddTransient(s => new ClipboardService(
    s.GetService<ClipboardRepository>(),
    s.GetService<RecordRepository>(),
    s.GetService<RabbitMqService>()
));

var app = builder.Build();
app.Services.GetService<ClipboardService>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();