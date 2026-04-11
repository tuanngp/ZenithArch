using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RynorArch.E2E.Tests;

public sealed class EndpointSemanticsTests
{
    [Fact]
    public async Task Post_returns_created_and_get_returns_ok_for_created_trip()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var payload = CreateTripPayload("E2E-Create");
        var postResponse = await client.PostAsJsonAsync("/api/trips", payload);

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdId = await ReadCreatedIdAsync(postResponse);
        Assert.NotEqual(Guid.Empty, createdId);

        var getResponse = await client.GetAsync($"/api/trips/{createdId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var body = await getResponse.Content.ReadAsStringAsync();
        Assert.Contains("E2E-Create", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Get_returns_not_found_for_missing_id()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/trips/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_returns_bad_request_when_route_id_mismatch()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var postResponse = await client.PostAsJsonAsync("/api/trips", CreateTripPayload("E2E-Mismatch"));
        var createdId = await ReadCreatedIdAsync(postResponse);

        var command = UpdateTripPayload(Guid.NewGuid(), "E2E-Mismatch-Updated");
        var putResponse = await client.PutAsJsonAsync($"/api/trips/{createdId}", command);

        Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
    }

    [Fact]
    public async Task Put_returns_not_found_for_missing_resource()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var missingId = Guid.NewGuid();
        var command = UpdateTripPayload(missingId, "E2E-Missing-Update");

        var response = await client.PutAsJsonAsync($"/api/trips/{missingId}", command);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_returns_no_content_for_existing_then_not_found()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var postResponse = await client.PostAsJsonAsync("/api/trips", CreateTripPayload("E2E-Delete"));
        var createdId = await ReadCreatedIdAsync(postResponse);

        var firstDelete = await client.DeleteAsync($"/api/trips/{createdId}");
        var secondDelete = await client.DeleteAsync($"/api/trips/{createdId}");

        Assert.Equal(HttpStatusCode.NoContent, firstDelete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, secondDelete.StatusCode);
    }

    [Fact]
    public async Task Delete_returns_not_found_for_missing_resource()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync($"/api/trips/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<Guid> ReadCreatedIdAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        if (!document.RootElement.TryGetProperty("id", out var idProperty))
        {
            throw new InvalidOperationException("Create response does not contain id field.");
        }

        return idProperty.GetGuid();
    }

    private static object CreateTripPayload(string token)
    {
        var date = DateTime.UtcNow.Date;
        return new
        {
            isDeleted = false,
            createdAt = date,
            createdBy = (string?)null,
            lastModifiedAt = (DateTime?)null,
            lastModifiedBy = (string?)null,
            title = $"{token} Title",
            description = $"{token} Description",
            destination = $"{token} Destination",
            startDate = date.AddDays(5),
            endDate = date.AddDays(9),
            budget = 1234.56m,
            coverImageUrl = "https://example.com/e2e.jpg",
            isPublic = true,
        };
    }

    private static object UpdateTripPayload(Guid id, string token)
    {
        var date = DateTime.UtcNow.Date;
        return new
        {
            id,
            isDeleted = false,
            createdAt = date,
            createdBy = (string?)null,
            lastModifiedAt = date,
            lastModifiedBy = (string?)null,
            title = $"{token} Title",
            description = $"{token} Description",
            destination = $"{token} Destination",
            startDate = date.AddDays(10),
            endDate = date.AddDays(12),
            budget = 2222.22m,
            coverImageUrl = "https://example.com/e2e-update.jpg",
            isPublic = true,
        };
    }
}
