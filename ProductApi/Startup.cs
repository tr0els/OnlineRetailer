using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductApi.Data;
using ProductApi.GraphQL;
using ProductApi.Infrastructure;
using ProductApi.Models;
using Prometheus;
using SharedModels;

namespace ProductApi
{
    public class Startup
    {
        // RabbitMQ connection string (I use CloudAMQP as a RabbitMQ server).
        // Remember to replace this connectionstring with your own.
        string cloudAMQPConnectionString =
            "host=rattlesnake.rmq.cloudamqp.com;virtualHost=pfyoxdnf;username=pfyoxdnf;password=Sh-G_0bSs87gBcJ54vJMva1IWeWdQ6pQ";

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // In-memory database:
            services.AddDbContext<ProductApiContext>(opt => opt.UseInMemoryDatabase("ProductsDb"));

            // Register repositories for dependency injection
            services.AddScoped<IRepository<Product>, ProductRepository>();

            // Register database initializer for dependency injection
            services.AddTransient<IDbInitializer, DbInitializer>();

            // Register ProductConverter for dependency injection
            services.AddSingleton<IConverter<Product, ProductDto>, ProductConverter>();
           
            services.AddSwaggerGen();

            services.AddControllers();

            services
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .AddFiltering()
                .AddSorting()
                .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = Environment.IsDevelopment());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Initialize the database
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dbContext = services.GetService<ProductApiContext>();
                var dbInitializer = services.GetService<IDbInitializer>();
                dbInitializer.Initialize(dbContext);
            }

            // Create a message listener in a separate thread.
            Task.Factory.StartNew(() =>
                new MessageListener(app.ApplicationServices, cloudAMQPConnectionString).Start());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseRouting();

            app.UseHttpMetrics();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
                endpoints.MapGraphQL();
            });
        }
    }
}
