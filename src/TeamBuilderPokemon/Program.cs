using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamBuilderPokemon.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// RequireConfirmedAccount fica falso porque o projeto nao tem um servico de
// e-mail configurado (o template so tem um NoOpEmailSender). Com true, ninguem
// jamais confirmaria a conta e o login nunca funcionaria.
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// O template gerado por "dotnet new mvc --auth Individual" nesta versao do
// SDK vem sem essa linha, o que quebra o login: sem ela, o cookie de
// autenticacao nunca e' lido de volta nas requisicoes seguintes, entao
// User.Identity.IsAuthenticated fica sempre falso e nenhum [Authorize]
// deixa ninguem entrar. Tem que vir depois de UseRouting() e antes de
// UseAuthorization().
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
