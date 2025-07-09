using ECommerceApp.Data;
using ECommerceApp.Repository;
using ECommerceApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ECommerceApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    policy =>
                    {
                        policy.WithOrigins("https://ambitious-field-03c216a1e.2.azurestaticapps.net") // React dev server
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });


            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                // This will use the property names as defined in the C# model
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure EF Core with SQL Server
            //builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(builder.Configuration.GetConnectionString("EFCoreDBConnection")));

            //builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            //   .AddEntityFrameworkStores<ApplicationDbContext>()
            //   .AddDefaultTokenProviders();

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 104857600; // 100 MB
            });

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
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "Default"))
                };
            });

            builder.Services.AddAuthorization();

            // Registering the CustomerService
            builder.Services.AddScoped<IDapperDbConnection, DapperDbConnection>();
            builder.Services.AddScoped<CustomerService>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

            //// Registering the AddressService
            //builder.Services.AddScoped<AddressService>();

            // Registering the CategoryService
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

            //// Registering the ProductService
            builder.Services.AddScoped<ProductService>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();


            //// Registering the ShoppingCartService
            builder.Services.AddScoped<ShoppingCartService>();
            builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();


            //// Registering the OrderService
            //builder.Services.AddScoped<OrderService>();

            //// Registering the PaymentService
            //builder.Services.AddScoped<PaymentService>();

            //// Registering the EmailService
            //builder.Services.AddScoped<EmailService>();

            //// Registering the CancellationService
            //builder.Services.AddScoped<CancellationService>();

            //// Registering the RefundService
            //builder.Services.AddScoped<RefundService>();

            //// Registering the FeedbackService
            //builder.Services.AddScoped<FeedbackService>();

            //// Register Refund Processing Background Service
            //builder.Services.AddHostedService<RefundProcessingBackgroundService>();

            //// Register Payment Background Service
            //builder.Services.AddHostedService<PendingPaymentService>();

            builder.Services.AddScoped<ITokenService,TokenService>();


            var app = builder.Build();

            app.UseCors("AllowReactApp"); 

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles(); 


            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();


        }
    }
}