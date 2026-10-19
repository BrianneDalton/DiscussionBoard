using DiscussionBoard.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DiscussionBoard.Tests.Infrastructure
{
    public sealed class ApplicationFixture : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private SqliteConnection? _connection;

        public ProjectFixture Project { get; } = new();

        public async ValueTask InitializeAsync()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            await _connection.OpenAsync();

            _ = Server;

            await using AsyncServiceScope scope = Services.CreateAsyncScope();
            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();
        }

        public new async ValueTask DisposeAsync()
        {
            base.Dispose();

            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }
        }

        public HttpClient CreateAuthenticatedClient()
        {
            return CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    if (_connection is null)
                    {
                        throw new InvalidOperationException("The test SQLite connection has not been initialized.");
                    }

                    options.UseSqlite(_connection);
                });

                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                        TestAuthenticationHandler.SchemeName,
                        _ => { });
            });
        }
    }
}
