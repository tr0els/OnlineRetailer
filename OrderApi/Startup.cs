using System;
using HotChocolate.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderApi.Data;
using OrderApi.GraphQL;
using OrderApi.Infrastructure;
using OrderApi.Models;
using Prometheus;
using SharedModels;

namespace OrderApi
{
    public class Startup
    {
        // Base URL for the product service when the solution is executed using docker-compose.
        // The product service (running as a container) listens on this URL for HTTP requests
        // from other services specified in the docker compose file (which in this solution is
        // the order service).
        /*string productServiceBaseUrl = "http://productapi/products/";

        string customerServiceBaseUrl = "http://customerapi/customers/";*/

        string productServiceBaseUrl = "https://localhost:44398/products/";
        string customerServiceBaseUrl = "https://localhost:44304/customers/";

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
            services.AddPooledDbContextFactory<OrderApiContext>(opt => opt.UseInMemoryDatabase("OrdersDb"));

            // Register repositories for dependency injection
            // Hotchocolate requires this to be transient so dbcontext concurrency issue doesn't arise
            // OrderRepository must also implement IAsyncDisposable to dispose dbcontext back to pool
            services.AddTransient<IRepository<Order>, OrderRepository>();

            // Register database initializer for dependency injection
            services.AddTransient<IDbInitializer, DbInitializer>();

            // Register product service gateway for dependency injection
            services.AddSingleton<IServiceGateway<ProductDto>>(new
                ProductServiceGateway(productServiceBaseUrl));

            services.AddSingleton<IServiceGateway<CustomerStatusDto>>(new
                CustomerServiceGateway(customerServiceBaseUrl));

            // Register MessagePublisher (a messaging gateway) for dependency injection
            services.AddSingleton<IMessagePublisher>(new
                MessagePublisher(cloudAMQPConnectionString));

            services.AddSingleton<IConverter<OrderLine, OrderLineDto>, OrderLineConverter>();

            services.AddSwaggerGen();

            services.AddControllers();

            services
                .AddGraphQLServer()
                .RegisterDbContext<OrderApiContext>(DbContextKind.Pooled)
                .RegisterService<IRepository<Order>>()
                .AddQueryType<Query>()
                .AddProjections()
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
                var dbInitializer = services.GetService<IDbInitializer>();
                // if dbInitializer doesn't implement both IAsyncDisposable and IDisposable, build errors will occur
                dbInitializer.Initialize();
            }

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
