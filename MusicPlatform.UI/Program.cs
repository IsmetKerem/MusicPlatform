using MusicPlatform.UI.Options;
using MusicPlatform.UI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection(ApiSettings.SectionName));

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                 ?? "http://localhost:5014";

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITokenStore, CookieTokenStore>();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddTransient<AuthTokenHandler>();

// API istemcisi: her istege token ekler, 401'de otomatik refresh dener
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();