using CustomerApi.Data;
using CustomerApi.GraphQL;
using CustomerApi.Infrastructure;
using CustomerApi.Models;
using HotChocolate.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerApi
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
            // services.AddDbContext<CustomerApiContext>(opt => opt.UseInMemoryDatabase("CustomersDb"));

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            services.AddPooledDbContextFactory<CustomerApiContext>(options =>
            {
                options
                .UseLoggerFactory(loggerFactory)
                .UseSqlite("Data Source=customers.db");
            });

            // Register repositories for dependency injection
            // Hotchocolate requires this to be transient so dbcontext concurrency issue doesn't arise
            // CustomerRepository must also implement IAsyncDisposable to dispose dbcontext back to pool
            services.AddTransient<IRepository<Customer>, CustomerRepository>();

            // Register database initializer for dependency injection
            services.AddTransient<IDbInitializer, DbInitializer>();

            services.AddSwaggerGen();

            services.AddControllers();

            services
                .AddGraphQLServer()
                .RegisterDbContext<CustomerApiContext>(DbContextKind.Pooled)
                .RegisterService<IRepository<Customer>>()
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
                var dbInitializer = services.GetService<IDbInitializer>();
                // if dbInitializer doesn't implement both IAsyncDisposable and IDisposable, build errors will occur
                dbInitializer.Initialize();
            }

            // Create a message listener in a separate thread.
            Task.Factory.StartNew(() =>
                new MessageListener(app.ApplicationServices, cloudAMQPConnectionString).Start());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

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
