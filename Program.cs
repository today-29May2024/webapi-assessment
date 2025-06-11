using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
.ConfigureApiBehaviorOptions(options => {
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
                .Select(e => e.Value?.Errors.Select(i => i.ErrorMessage));

        var errorResponse = new
        {
            result = 0,
            resultmessage = errors
        };

        return new BadRequestObjectResult(errorResponse)
        {
            ContentTypes = { "application/json" }
        };
    };
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

app.UseAuthorization();

app.MapControllers();

app.Run();
