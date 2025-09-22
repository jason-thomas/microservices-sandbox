using Cnp.Sdk;
using cnp_api_wrapper.Configuration;
using cnp_api_wrapper.Handlers;
using cnp_api_wrapper.Mappings;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddOptions<CnpOptions>()
    .Bind(builder.Configuration.GetSection("CnpOptions"))
    .ValidateDataAnnotations();

builder.Services.AddSingleton<ICnpOnline>(sp =>
{
    var options = sp.GetRequiredService<IOptions<CnpOptions>>().Value;
    return new CnpOnline(options.ToDictionary());
});

builder.Services.AddScoped<EcheckDebitMapper>();
builder.Services.AddScoped<EcheckCreditMapper>();
builder.Services.AddScoped<EcheckRedepositMapper>();
builder.Services.AddScoped<EcheckVoidMapper>();

builder.Services.AddScoped<IEcheckDebitHandler, EcheckDebitHandler>();
builder.Services.AddScoped<IEcheckCreditHandler, EcheckCreditHandler>();
builder.Services.AddScoped<IEcheckRedepositHandler, EcheckRedepositHandler>();
builder.Services.AddScoped<IEcheckVoidHandler, EcheckVoidHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
