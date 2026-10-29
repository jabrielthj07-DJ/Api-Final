using Capa_Datos;
using Capa_Entidades;
using Capa_Negocio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// 1. CONEXIÓN A LA BASE DE DATOS
// =============================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontró la cadena de conexión 'DefaultConnection'. " +
        "Verifique la configuración del App Service de Azure.");
}

builder.Services.AddDbContext<MiDbContext>(options =>
    options.UseSqlServer(connectionString));

// =============================================
// 2. INYECCIÓN DE DEPENDENCIAS
// =============================================
builder.Services.AddScoped<ProductoDatos>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new ProductoDatos(configuration);
});

builder.Services.AddScoped<ProductoNegocio>();
builder.Services.AddScoped<UsuarioNegocio>();
builder.Services.AddScoped<Venta_Negocio>();
builder.Services.AddScoped<Compra_Negocio>();
builder.Services.AddScoped<Rol_Negocio>(); // MODIFICADO
builder.Services.AddScoped<CategoriaNegocio>();
builder.Services.AddScoped<InventarioNegocio>();
builder.Services.AddScoped<ProveedorNegocio>();

// =============================================
// 3. CONTROLADORES
// =============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// =============================================
// 4. CONFIGURACIÓN JWT
// =============================================

// AGREGADO: leer la configuración de Azure o appsettings.json.
// La clave alternativa permite compatibilidad inicial con el código anterior.
// En producción, configura Jwt:Key en Azure y usa la misma clave
// en el controlador que GENERA el token.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? "MiClaveMuySegura1234567890123456";

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "Falta configurar Jwt:Issuer o Jwt:Audience en la configuración de la API.");
}

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
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

            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    mensaje = "Acceso no autorizado. Debe iniciar sesión y proporcionar un token válido."
                });

                return context.Response.WriteAsync(result);
            },

            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    mensaje = "Acceso denegado. No tiene permisos para acceder."
                });

                return context.Response.WriteAsync(result);
            }
        };
    });

// =============================================
// 5. CORS
// =============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// =============================================
// 6. SWAGGER
// =============================================
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

    // Ordenar controladores
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

    // Autenticación Bearer para Swagger
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

    // AGREGADO: cargar el XML únicamente si existe.
    var xmlFile =
        $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";

    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    c.EnableAnnotations();
});

// =============================================
// 7. CONSTRUIR APLICACIÓN
// =============================================
var app = builder.Build();

// =============================================
// 8. MIDDLEWARES
// =============================================
app.UseHttpsRedirection();

app.UseCors("PermitirTodo");

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Veterinaria M&L v1");

    // Swagger estará disponible en la raíz de la aplicación.
    c.RoutePrefix = "";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();