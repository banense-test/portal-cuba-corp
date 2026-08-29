using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PortalCubaCorp.Domain;
using PortalCubaCorp.Infrastructure;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace PortalCubaCorp.Tests;

/// <summary>
/// Performance/load tests for NFR-001 and NFR-002.
/// Binding condition #1 from stakeholder sanction (Transition T2).
///
/// NFR-001: Page load &lt; 3 seconds on corporate network.
/// NFR-002: Clock in/out response &lt; 1 second.
///
/// These tests use WebApplicationFactory with in-memory database and
/// mock authentication (MockAuthHandler) to measure actual HTTP response
/// times. Measured values are reported via ITestOutputHelper.
///
/// Issue #37: CR: Materialize NFR-001/NFR-002 performance test code.
///
/// Mock auth: MockAuthHandler — expiry 2027-01-31, owner STK-003.
/// </summary>
public class PerformanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public PerformanceTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _output = output;
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Remove existing DbContext options registrations
                var dbDescriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<PortalDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions)).ToList();
                foreach (var d in dbDescriptors)
                    services.Remove(d);

                // Add in-memory database for testing
                services.AddDbContext<PortalDbContext>(options =>
                    options.UseInMemoryDatabase("PerfTestDb"));

                // Register mock auth handler scheme
                services.AddAuthentication(MockAuthHandler.AuthScheme)
                    .AddScheme<AuthenticationSchemeOptions, MockAuthHandler>(
                        MockAuthHandler.AuthScheme, _ => { });

                // Override the default schemes set by Program.cs (Cookie + OIDC)
                // PostConfigure runs after all Configure calls, so it overrides
                // the defaults set by AddAuthentication in Program.cs
                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultScheme = MockAuthHandler.AuthScheme;
                    options.DefaultChallengeScheme = MockAuthHandler.AuthScheme;
                    options.DefaultAuthenticateScheme = MockAuthHandler.AuthScheme;
                    options.DefaultSignInScheme = MockAuthHandler.AuthScheme;
                    options.DefaultSignOutScheme = MockAuthHandler.AuthScheme;
                    options.DefaultForbidScheme = MockAuthHandler.AuthScheme;
                });
            });
        });
    }

    /// <summary>
    /// Ensures the in-memory database is created and seeded with a published news item.
    /// </summary>
    private async Task EnsureDatabaseCreatedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortalDbContext>();
        await db.Database.EnsureCreatedAsync();
        if (!db.NewsItems.Any())
        {
            db.NewsItems.Add(new NewsItem
            {
                Id = Guid.NewGuid(),
                Title = "Welcome to the Portal",
                Body = "This is a test news item for performance testing.",
                Category = NewsCategory.General,
                Status = NewsStatus.Published,
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AuthorId = "test-author"
            });
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// NFR-001: Page load must be under 3 seconds.
    /// Measures the full HTTP pipeline: middleware → auth → page handler → render.
    /// Runs 5 iterations after warmup and reports all measured values.
    /// </summary>
    [Fact]
    public async Task NFR001_PageLoad_Under3Seconds()
    {
        await EnsureDatabaseCreatedAsync();

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true
        });

        var measurements = new List<long>();

        // Warmup — first request includes JIT and service initialization
        try { await client.GetAsync("/"); } catch { /* warmup may redirect, that's fine */ }

        // Measure 5 iterations
        for (int i = 0; i < 5; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await client.GetAsync("/");
            stopwatch.Stop();
            measurements.Add(stopwatch.ElapsedMilliseconds);

            // Accept 200 (OK) or 302 (redirect to login if auth not fully wired)
            Assert.True(response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Redirect ||
                response.StatusCode == HttpStatusCode.Found,
                $"NFR-001: Unexpected status code {response.StatusCode} on iteration {i + 1}");
        }

        var maxMs = measurements.Max();
        var avgMs = (long)measurements.Average();
        var allValues = string.Join(", ", measurements.Select(m => $"{m}ms"));

        _output.WriteLine($"NFR-001 Page Load Results:");
        _output.WriteLine($"  Iterations: 5 (after 1 warmup)");
        _output.WriteLine($"  Measured values: [{allValues}]");
        _output.WriteLine($"  Average: {avgMs}ms");
        _output.WriteLine($"  Maximum: {maxMs}ms");
        _output.WriteLine($"  Threshold: 3000ms (NFR-001)");
        _output.WriteLine($"  Result: {(maxMs < 3000 ? "PASS" : "FAIL")}");

        // NFR-001: < 3 seconds (3000ms) — all iterations must pass
        Assert.True(maxMs < 3000,
            $"NFR-001 FAIL: Page load max {maxMs}ms exceeds 3000ms threshold. " +
            $"Measured: [{allValues}]");
    }

    /// <summary>
    /// NFR-002: Clock in/out response must be under 1 second.
    /// Measures the full HTTP pipeline: middleware → auth → API handler → service → persistence.
    /// Runs 5 iterations after warmup and reports all measured values.
    /// </summary>
    [Fact]
    public async Task NFR002_ClockInResponse_Under1Second()
    {
        await EnsureDatabaseCreatedAsync();

        var client = _factory.CreateClient();
        var measurements = new List<long>();

        // Warmup
        var warmupRequest = new
        {
            timestamp = DateTime.UtcNow,
            clockType = "In",
            idempotencyKey = $"warmup-{Guid.NewGuid()}"
        };
        try { await client.PostAsJsonAsync("/api/clocking", warmupRequest); } catch { /* warmup */ }

        // Measure 5 iterations
        for (int i = 0; i < 5; i++)
        {
            var request = new
            {
                timestamp = DateTime.UtcNow,
                clockType = "In",
                idempotencyKey = $"perf-test-{i}-{Guid.NewGuid()}"
            };

            var stopwatch = Stopwatch.StartNew();
            var response = await client.PostAsJsonAsync("/api/clocking", request);
            stopwatch.Stop();
            measurements.Add(stopwatch.ElapsedMilliseconds);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        var maxMs = measurements.Max();
        var avgMs = (long)measurements.Average();
        var allValues = string.Join(", ", measurements.Select(m => $"{m}ms"));

        _output.WriteLine($"NFR-002 Clock In/Out Response Results:");
        _output.WriteLine($"  Iterations: 5 (after 1 warmup)");
        _output.WriteLine($"  Measured values: [{allValues}]");
        _output.WriteLine($"  Average: {avgMs}ms");
        _output.WriteLine($"  Maximum: {maxMs}ms");
        _output.WriteLine($"  Threshold: 1000ms (NFR-002)");
        _output.WriteLine($"  Result: {(maxMs < 1000 ? "PASS" : "FAIL")}");

        // NFR-002: < 1 second (1000ms) — all iterations must pass
        Assert.True(maxMs < 1000,
            $"NFR-002 FAIL: Clock response max {maxMs}ms exceeds 1000ms threshold. " +
            $"Measured: [{allValues}]");
    }
}