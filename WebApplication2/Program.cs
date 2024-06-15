using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using WebApplication2.service;
using WebApplication2.service.impl;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<WordPDFConvertService, WordPdfConvertServiceImpl>();

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 1_000_000; // if don't set default value is: 30 MB
});
builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = 1_000_000;
    x.MultipartBodyLengthLimit = 1_000_000; // if don't set default value is: 128 MB
    x.MultipartHeadersLengthLimit = 1_000_000;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();

app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "DotNet_AWS"); });

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();