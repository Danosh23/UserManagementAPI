using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
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

app.UseMiddleware<ErrorHandlingMiddleware>();           // 1. Error handling first
app.UseMiddleware<TokenAuthenticationMiddleware>();     // 2. Authentication next
app.UseMiddleware<RequestResponseLoggingMiddleware>();  // 3. Logging last

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();