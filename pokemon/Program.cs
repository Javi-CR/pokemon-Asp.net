using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using pokemon.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(connectionString + ";TrustServerCertificate=True"));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configurar la identidad y ajustar los requisitos de la contraseña
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    // Ajusta los requisitos de la contraseña según tus necesidades
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddRoles<IdentityRole>() // Agregar soporte para roles
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Crear roles y usuarios por defecto
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        await CreateRolesAndUsersAsync(roleManager, userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating roles and users.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

static async Task CreateRolesAndUsersAsync(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
{
    string[] roleNames = { "administrador", "enfermeria", "entrenador" };
    IdentityResult roleResult;

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Error al crear el rol '{roleName}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }
    }

    // Crear usuarios con el rol "administrador"
    var adminUsers = new List<(string Email, string UserName, string Password)>
{
    ("admin@pokemon.com", "Admin", "123456"),
    ("javier@pokemon.com", "J-Admin", "123456")
};

    foreach (var userInfo in adminUsers)
    {
        var user = await userManager.FindByEmailAsync(userInfo.Email);
        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = userInfo.UserName,
                Email = userInfo.Email,
                EmailConfirmed = true // Confirmar el correo electrónico al crear el usuario
            };
            var result = await userManager.CreateAsync(user, userInfo.Password);
            if (result.Succeeded)
            {
                var roleAddResult = await userManager.AddToRoleAsync(user, "administrador");
                if (!roleAddResult.Succeeded)
                {
                    throw new Exception($"Error al asignar el rol 'administrador' al usuario '{userInfo.UserName}': {string.Join(", ", roleAddResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                throw new Exception($"Error al crear el usuario '{userInfo.UserName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }

    // Crear usuarios con el rol "enfermeria"
    var enfermeriaUsers = new List<(string Email, string UserName, string Password)>
{
    ("enfermeria@pokemon.com", "Enfermeria", "123456"),
    ("javier@enfermeria.com", "J-Enfermero", "123456")
};

    foreach (var userInfo in enfermeriaUsers)
    {
        var user = await userManager.FindByEmailAsync(userInfo.Email);
        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = userInfo.UserName,
                Email = userInfo.Email,
                EmailConfirmed = true // Confirmar el correo electrónico al crear el usuario
            };
            var result = await userManager.CreateAsync(user, userInfo.Password);
            if (result.Succeeded)
            {
                var roleAddResult = await userManager.AddToRoleAsync(user, "enfermeria");
                if (!roleAddResult.Succeeded)
                {
                    throw new Exception($"Error al asignar el rol 'enfermeria' al usuario '{userInfo.UserName}': {string.Join(", ", roleAddResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                throw new Exception($"Error al crear el usuario '{userInfo.UserName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
