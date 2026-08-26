using ClyvoVetApi.Data;
using ClyvoVetApi.Exceptions;
using ClyvoVetApi.Repositories;
using ClyvoVetApi.Repositories.Interfaces;
using ClyvoVetApi.Services;
using ClyvoVetApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "ClyvoVet API",
            Version = "v1",
            Description = "API para gestão da jornada contínua de saúde do pet - FIAP Challenge 2026"
        };
        return Task.CompletedTask;
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

// Registro de Repositórios
builder.Services.AddScoped<IDonoRepository, DonoRepository>();
builder.Services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
builder.Services.AddScoped<IMedicamentoRepository, MedicamentoRepository>();
builder.Services.AddScoped<IConsultaMedicamentoRepository, ConsultaMedicamentoRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IConsultaRepository, ConsultaRepository>();
builder.Services.AddScoped<IVacinaRepository, VacinaRepository>();

// Registro de Serviços
builder.Services.AddScoped<IDonoService, DonoService>();
builder.Services.AddScoped<IFuncionarioService, FuncionarioService>();
builder.Services.AddScoped<IMedicamentoService, MedicamentoService>();
builder.Services.AddScoped<IConsultaMedicamentoService, ConsultaMedicamentoService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IConsultaService, ConsultaService>();
builder.Services.AddScoped<IVacinaService, VacinaService>();
builder.Services.AddScoped<IIntelligenceService, IntelligenceService>();
builder.Services.AddHostedService<AlertScheduler>();


// Tratamento Global de Exceções (RFC 7807)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Ativa o middleware de interceptação global de exceções
app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();