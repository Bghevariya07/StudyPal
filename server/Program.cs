using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using server.Models;
using server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IMongoClient, MongoClient>(_ => new MongoClient(builder.Configuration["MongoDB:AtlasURI"]));

// Add MongoDB Identity Configuration
var mongoDbIdentityConfig = new MongoDbIdentityConfiguration
{
    MongoDbSettings = new MongoDbSettings
    {
        ConnectionString = builder.Configuration["MongoDB:AtlasURI"],
        DatabaseName = builder.Configuration["MongoDB:DatabaseName"]
    },
    IdentityOptionsAction = options =>
    {
        // TODO: Change based on password requirement
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 1;
        options.Password.RequireLowercase = false;
        options.Password.RequiredUniqueChars = 0;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    }
};

builder.Services.ConfigureMongoDbIdentity<User, Role, Guid>(mongoDbIdentityConfig)
    .AddUserManager<UserManager<User>>()
    .AddSignInManager<SignInManager<User>>()
    .AddRoleManager<RoleManager<Role>>();
builder.Services.AddScoped<JWTTokenService>();
builder.Services.AddScoped<AuthService>();

// Add GroupChatService
builder.Services.AddSingleton(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    var database = mongoClient.GetDatabase(builder.Configuration["MongoDB:DatabaseName"]);
    return new GroupChatService(database);
});

// Add ChatService with GroupChatService dependency
builder.Services.AddSingleton(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    var database = mongoClient.GetDatabase(builder.Configuration["MongoDB:DatabaseName"]);
    var groupChatService = sp.GetRequiredService<GroupChatService>();
    return new ChatService(database, groupChatService);
});

builder.Services.AddSingleton<UserService>(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    return new UserService(mongoClient, builder.Configuration["MongoDB:DatabaseName"], "UserProfiles");
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])
        )
    };
});

var app = builder.Build();
app.UseCors(x => x
    .SetIsOriginAllowed(origin => true)
    .AllowAnyMethod()
    .AllowAnyHeader());
// app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
Console.WriteLine("listening"); // doesn't work 😔
System.Diagnostics.Debug.WriteLine("listening");  // doesn't work 😔
app.Run();
