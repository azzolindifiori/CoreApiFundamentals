using System.Reflection;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Data.DatabaseRepo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlKata.Compilers;

namespace CoreCodeCamp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CampContext>();
            //services.AddScoped<ICampRepository, CampRepository>();
            services.AddScoped<IDatabaseConnectionFactory>(_ => new SqlServerConnectionFactory(Configuration["ConnectionStrings:CodeCamp"]));
            services.AddScoped<Compiler>(_ => new SqlServerCompiler { UseLegacyPagination = false });
            services.AddScoped<IRepositoryBase, RepositoryBase>();
            services.AddScoped<ICampRepository, CampRepository>();
            services.AddScoped<ITalkRepository, TalkRepository>();
            services.AddScoped<IGenericRepository, GenericRepository>();
            services.AddScoped<ISpeakerRepository, SpeakerRepository>();
                ;
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(cfg =>
            {
                cfg.MapControllers();
            });

            //mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
