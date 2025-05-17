using Microsoft.EntityFrameworkCore;
using Airport.Data;
using Airport.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // SQL Server подключение (закомментировано для использования SQLite)
    /*
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
    */
    
    // SQLite подключение (раскомментируйте эту строку для использования SQLite)
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLiteConnection"));
});

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

// Add Services
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Корректно настраиваем параметры жизненного цикла приложения
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
    serverOptions.AllowSynchronousIO = false;
});

// Добавляем корректную обработку процесса завершения
builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

// Регистрация обработчиков сигналов для корректного завершения приложения
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    AppDomain.CurrentDomain.ProcessExit += (s, e) => 
    {
        Console.WriteLine("Процесс завершается, освобождаем ресурсы...");
        Thread.Sleep(500); // Даем время на освобождение ресурсов
    };
}

// Применение миграций/создание базы данных автоматически
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Только создаем базу данных, если её нет, но не удаляем существующую
        context.Database.EnsureCreated();
        
        // Инициализируем базу данных тестовыми данными только если таблица пользователей пуста
        if (!context.Users.Any())
        {
            DbInitializer.Initialize(context);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Произошла ошибка при миграции или инициализации БД.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Сначала регистрируем маршруты MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Затем регистрируем Razor Pages
app.MapRazorPages();

app.Run();
