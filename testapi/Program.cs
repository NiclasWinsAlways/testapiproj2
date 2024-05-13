using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TestRepo.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
});

// Register DbContext and DbAccess services
builder.Services.AddScoped<Dbcontext>();
builder.Services.AddScoped<DbAccess>();

// Configure SQL Server Database
string conStr = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(conStr))
{
    conStr = @"Server=localhost; Database=MyBookDB; Trusted_Connection=True; TrustServerCertificate=True;";
}
builder.Services.AddDbContext<Dbcontext>(options =>
    options.UseSqlServer(conStr));

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("http://localhost:4200") // Assuming your Angular is served from this URL
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Swagger configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting(); // Ensure UseRouting is called before UseCors and UseAuthorization

// Use the CORS middleware and specify the default policy
app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
