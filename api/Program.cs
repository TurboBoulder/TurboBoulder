using Microsoft.EntityFrameworkCore;
using TurboBoulder.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using TurboBoulder;
using TurboBoulder.Services;
using TurboBoulder.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TurboBoulder.Middleware;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;

        // add services
        ConfigureServices(builder.Services);

        // configure database
        ConfigureDatabase(builder.Services);

        // configure identity
        ConfigureIdentity(builder.Services);

        // configure authentication
        ConfigureAuth(builder, configuration);


        var app = builder.Build();

        SetupDatabase(app);

        SetupMiddleware(app);

        app.Run();


        static void ConfigureAuth(WebApplicationBuilder builder, ConfigurationManager configuration)
        {
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies["JWT_Cookie"];
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("OnTokenValidated: " + context.SecurityToken);
                            return Task.CompletedTask;
                        }
                    };
                });
            //.AddJwtBearer(options =>
            //{
            //    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            //    {
            //        ValidateIssuer = true,
            //        ValidateAudience = true,
            //        ValidateLifetime = true,
            //        ValidateIssuerSigningKey = true,
            //        ValidIssuer = configuration["Jwt:Issuer"],
            //        ValidAudience = configuration["Jwt:Issuer"],
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
            //    };

            //    options.Events = new JwtBearerEvents
            //    {
            //        OnAuthenticationFailed = context =>
            //        {
            //            Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
            //            return Task.CompletedTask;
            //        },
            //        OnTokenValidated = context =>
            //        {
            //            Console.WriteLine("OnTokenValidated: " + context.SecurityToken);
            //            return Task.CompletedTask;
            //        }
            //    };
            //});
        }

        void ConfigureServices(IServiceCollection services)
        {
            if (builder.Environment.IsDevelopment())
            {
                var client = new HttpClient();
                var certificateBytes = client.GetByteArrayAsync("http://myproject.idawebtemplate.internal:8000/webserver.pfx").Result;

                builder.WebHost.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.ConfigureHttpsDefaults(listenOptions =>
                    {
                        listenOptions.ServerCertificate = new X509Certificate2(certificateBytes, "3s5Y9gRwZk1P7q");
                    });
                });
            }            

            services.AddCors();

            services.AddHealthChecks()
              .AddCheck<DatabaseHealthCheck>("Database");

            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<ITwoFactorAuthenticationService, EmailTwoFactorAuthenticationService>();

            services.Configure<SecuritySettings>(configuration.GetSection("Security"));

            services.AddControllers();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authentication",
                    Description = "Enter JWT Bearer token **_only_**",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // must be lower case
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
        {securityScheme, new string[] { }}
            });
            });
        }

        void ConfigureDatabase(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")
                )
            );
        }

        void ConfigureIdentity(IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        }

        void SetupDatabase(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                try
                {
                    //using (SqlConnection connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
                    //{
                    //    try
                    //    {
                    //        connection.Open();
                    //        Console.WriteLine("SQL Server engine is available");
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Console.WriteLine("Failed to connect to SQL Server engine: " + ex.Message);
                    //        throw new Exception("Cannot connect to the database.");
                    //    }
                    //}

                    //// Check if the database can be connected to
                    //if (!context.Database.CanConnect())
                    //{
                    //    throw new Exception("Cannot connect to the database.");
                    //}

                    // This will create the database and all its tables if they don't exist.
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    var initialUser = new User
                    {
                        UserName = "admin", // or an email address if you're using that
                        Email = "admin@idania.se",
                    };

                    if (userManager.FindByNameAsync(initialUser.UserName).Result == null)
                    {
                        var result = userManager.CreateAsync(initialUser, "admin123ADMIN!").Result;

                        if (!result.Succeeded)
                        {
                            throw new Exception("Failed to create initial user: " + string.Join(", ", result.Errors));
                        }

                        // If the admin role doesn't exist, create it
                        if (!roleManager.RoleExistsAsync("Admin").Result)
                        {
                            var role = new IdentityRole { Name = "Admin" };
                            var roleResult = roleManager.CreateAsync(role).Result;

                            if (!roleResult.Succeeded)
                            {
                                throw new Exception("Failed to create admin role: " + string.Join(", ", roleResult.Errors));
                            }
                        }

                        // Assign the user to the admin role
                        var addToRoleResult = userManager.AddToRoleAsync(initialUser, "Admin").Result;

                        if (!addToRoleResult.Succeeded)
                        {
                            throw new Exception("Failed to add user to admin role: " + string.Join(", ", addToRoleResult.Errors));
                        }
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while setting up the database.");
                    Environment.Exit(666);
                }
            }
        }

        static void SetupMiddleware(WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<HeadersLoggingMiddleware>();
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseRouting();

            app.UseCors(policy =>
                policy.WithOrigins("https://localhost:7020") // replace with your client app URL
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
            );

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHealthChecks("/health");
            app.MapControllers();
        }
    }
}