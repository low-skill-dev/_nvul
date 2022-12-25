using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using nvul_server.Services;
using System.Text;

namespace image_indexator_backend
{
	class Program
	{
		public static void Main(string[] args)
		{
			WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
			builder.Configuration
				.AddEnvironmentVariables()
				.Build();

			builder.Host.ConfigureLogging(opts =>
			{
				opts.AddConsole();
			});

			builder.Services.AddControllers()
				.AddJsonOptions(conf => conf.JsonSerializerOptions.PropertyNameCaseInsensitive=true)
				.AddNewtonsoftJson();
			builder.Services.AddSingleton<NvulConfigurationProvider>();

			if (builder.Environment.IsDevelopment())
			{
				builder.Services.AddEndpointsApiExplorer();
				builder.Services.AddSwaggerGen(opts =>
				{
					opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
					{
						In = ParameterLocation.Header,
						Description = "Please insert JWT with Bearer into field",
						Name = "Authorization",
						Type = SecuritySchemeType.Http,
						Scheme = "Bearer",
						BearerFormat = "JWT"
					});
					opts.AddSecurityRequirement(new OpenApiSecurityRequirement {
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						}
					},
					new string[] { }
				}});
				});
			}

			WebApplication app = builder.Build();

			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseCors(opts =>
			{
				opts.AllowAnyOrigin();
				opts.AllowAnyMethod();
				opts.AllowAnyHeader();
			});

			app.UseRouting();
			app.MapControllers();

			app.Run();
		}
	}
}