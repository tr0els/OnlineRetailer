using GraphQL.Server.Ui.Voyager;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQLGateway
{
    public class Startup
    {
        public const string Customers = "customers";
        public const string Orders = "orders";
        public const string Products = "products";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            /*services.AddHttpClient(Customers, c => c.BaseAddress = new Uri("http://customerapi/graphql/"));
            services.AddHttpClient(Orders, c => c.BaseAddress = new Uri("http://orderapi/graphql/"));
            services.AddHttpClient(Products, c => c.BaseAddress = new Uri("http://productapi/graphql/"));*/

            services.AddHttpClient(Customers, c => c.BaseAddress = new Uri("https://localhost:44304/graphql/"));
            services.AddHttpClient(Orders, c => c.BaseAddress = new Uri("https://localhost:44309/graphql/"));
            services.AddHttpClient(Products, c => c.BaseAddress = new Uri("https://localhost:44398/graphql/"));

            /*services.AddGraphQLServer()
                .AddRemoteSchema(Customers)
                .AddRemoteSchema(Orders)
                .AddRemoteSchema(Products);*/

            services.AddGraphQLServer()
                .AddRemoteSchema(Customers)
                .AddRemoteSchema(Orders)
                .AddRemoteSchema(Products)
                .AddTypeExtensionsFromFile("./stitching.graphql");

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGraphQL();
            });

            app.UseGraphQLVoyager(new VoyagerOptions()
            {
                GraphQLEndPoint = "/graphql"
            }, "/graphql-voyager");
        }
    }
}
