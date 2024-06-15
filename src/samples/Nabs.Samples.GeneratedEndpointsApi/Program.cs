using Nabs.Samples.BusinessLogic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(config =>
{
	config.RegisterServicesFromAssemblyContaining<Person>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
