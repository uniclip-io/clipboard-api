using ClipboardApi.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(_ => new ClipboardRepository(builder.Configuration["ConnectionStrings:MongoDb"]!));
builder.Services.AddSingleton(_ => new RecordRepository(builder.Configuration["ConnectionStrings:MongoDb"]!));
builder.Services.AddScoped<ClipboardApi.Services.ClipboardService, ClipboardApi.Services.ClipboardService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();