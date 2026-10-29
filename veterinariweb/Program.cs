using Capa_Datos;
using Capa_Entidades;
using Capa_Negocio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// CONEXIÓN A BASE DE DATOS
// ===============================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? builder.Configuration.GetConnectionString("Connection");

builder.Services.AddDbContext<MiDbContext>(options =>
    options.UseSqlServer(connectionString));


// ===============================
// CONFIGURACIÓN GENERAL
// ===============================
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);


// ===============================
// INYECCIÓN DE DEPENDENCIAS
// ===============================
builder.Services.AddScoped<ProductoDatos>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new ProductoDatos(configuration);
});

builder.Services.AddScoped<ProductoNegocio>();
builder.Services.AddScoped<UsuarioNegocio>();
builder.Services.AddScoped<Venta_Negocio>();
builder.Services.AddScoped<Compra_Negocio>();
builder.Services.AddSingleton<Rol_Negocio>();
builder.Services.AddScoped<CategoriaNegocio>();
builder.Services.AddScoped<InventarioNegocio>();
builder.Services.AddScoped<ProveedorNegocio>();

builder.Services.AddControllers();


// ===============================
// CORS
// ===============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// ===============================
// JWT
// ===============================
var jwtKey = builder.Configuration["Jwt:Key"] ?? "MiClaveMuySegura1234567890123456";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "miApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "miAppUsuarios";

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

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        ),

        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        // ERROR 401
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                mensaje = "Acceso no autorizado. Debe iniciar sesión y proporcionar un token válido."
            });

            return context.Response.WriteAsync(result);
        },

        // ERROR 403
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                mensaje = "Acceso denegado. No tiene permisos para acceder."
            });

            return context.Response.WriteAsync(result);
        }
    };
});


// ===============================
// SWAGGER
// ===============================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Veterinaria M&L",
        Version = "v1",
        Contact = new OpenApiContact
        {
            Name = "Soporte Tecnico",
            Email = "axslpez@gmail.com"
        }
    });

    // ORDENAR CONTROLADORES
    c.OrderActionsBy(apiDesc =>
    {
        var controller =
            (apiDesc.ActionDescriptor as ControllerActionDescriptor)
            ?.ControllerName ?? "";

        if (controller == "Login") return "1_Login";
        if (controller == "Rol") return "2_Rol";
        if (controller == "UsuarioRol") return "3_UsuarioRol";
        if (controller == "Categorias") return "4_Categorias";
        if (controller == "Producto") return "5_Producto";
        if (controller == "Ventas") return "6_Ventas";
        if (controller == "Inventario") return "7_Inventario";
        if (controller == "Compras") return "8_Compras";
        if (controller == "Proveedor") return "9_Proveedor";

        return "99_" + controller;
    });

    // JWT EN SWAGGER
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT con formato: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // XML COMMENTS (Verifica si el archivo existe para no tumbar la app en Azure)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, true);
    }

    c.EnableAnnotations();
});


// ===============================
// CONSTRUIR APP
// ===============================
var app = builder.Build();

// Mostrar detalles de errores si algo falla durante pruebas
app.UseDeveloperExceptionPage();

// ===============================
// MIDDLEWARES
// ===============================
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("PermitirTodo");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();