using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(config =>
{
	config.RegisterServicesFromAssemblyContaining<Nabs.Samples.BusinessLogic.PersonDomain.Root>();
    config.RegisterServicesFromAssemblyContaining<Nabs.Samples.BusinessLogic.CompanyDomain.Root>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    c.EnableAnnotations();
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
