var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Mapeo de Razor Pages
app.MapRazorPages();

// Esta es la ruta personalizada para que /comentarios/unidad-45 no de error 404
app.MapGet("/{nombreAutobus}", (string nombreAutobus, HttpContext context) =>
{
    context.Response.Redirect($"/?ruta={nombreAutobus}");
    return Task.CompletedTask;
});

app.MapGet("/{nombreAutobus}/comentarios", (string nombreAutobus, HttpContext context) =>
{
    context.Response.Redirect($"/?comentarios={nombreAutobus}");
    return Task.CompletedTask;
});

app.Run();