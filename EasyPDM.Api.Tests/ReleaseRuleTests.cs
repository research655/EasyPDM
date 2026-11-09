using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace EasyPDM.Api.Tests;

[Collection("EasyPDM database")]
public class ReleaseRuleTests
{
    [Fact]
    public async Task Second_pdf_upload_deletes_the_first_file()
    {
        await using var factory = new EasyPDMWebApplicationFactory();
        using var client = factory.CreateClient();
        await client.LoginAsync("admin", "admin");
        var projectId = await client.CreateProjectAsync($"Pdf {Guid.NewGuid()}");
        var itemId = await client.CreateNodeAsync(projectId, "Plate", "part");

        await UploadPdfAsync(client, itemId, "first.pdf", "first");
        var firstId = (await PdfAttachmentsAsync(client, itemId)).Single();
        var filesAfterFirst = CountStoredFiles(factory.StorageRoot);

        await UploadPdfAsync(client, itemId, "second.pdf", "second");

        var ids = await PdfAttachmentsAsync(client, itemId);
        Assert.Single(ids);
        Assert.NotEqual(firstId, ids[0]);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/attachments/{firstId}/file")).StatusCode);
        Assert.Equal("second", await client.GetStringAsync($"/api/attachments/{ids[0]}/file"));
        Assert.Equal(filesAfterFirst, CountStoredFiles(factory.StorageRoot));
    }

    [Fact]
    public async Task Author_cannot_release_and_admin_can()
    {
        await using var factory = new EasyPDMWebApplicationFactory();
        using var admin = factory.CreateClient();
        await admin.LoginAsync("admin", "admin");
        var projectId = await admin.CreateProjectAsync($"Rel {Guid.NewGuid()}");
        var authorName = "author" + Guid.NewGuid().ToString("N")[..8];
        var authorId = await admin.CreateUserAsync(authorName, "test-pass-1");
        await admin.GrantProjectAccessAsync(projectId, authorId);

        using var author = factory.CreateClient();
        await author.LoginAsync(authorName, "test-pass-1");
        var itemId = await author.CreateNodeAsync(projectId, "Plate", "part");

        (await author.PatchAsJsonAsync($"/api/items/{itemId}/status", new { status = "sprawdzany" }))
            .EnsureSuccessStatusCode();

        var selfRelease = await author.PatchAsJsonAsync($"/api/items/{itemId}/status", new { status = "wydany" });
        Assert.Equal(HttpStatusCode.Forbidden, selfRelease.StatusCode);

        (await admin.PatchAsJsonAsync($"/api/items/{itemId}/status", new { status = "wydany" }))
            .EnsureSuccessStatusCode();

        var ownId = await admin.CreateNodeAsync(projectId, "Admin plate", "part");
        (await admin.PatchAsJsonAsync($"/api/items/{ownId}/status", new { status = "sprawdzany" }))
            .EnsureSuccessStatusCode();
        (await admin.PatchAsJsonAsync($"/api/items/{ownId}/status", new { status = "wydany" }))
            .EnsureSuccessStatusCode();
    }

    private static async Task UploadPdfAsync(HttpClient client, Guid itemId, string fileName, string content)
    {
        using var form = new MultipartFormDataContent
        {
            { new ByteArrayContent(Encoding.UTF8.GetBytes(content)), "file", fileName },
            { new StringContent("pdf"), "role" },
        };
        (await client.PostAsync($"/api/items/{itemId}/attachments", form)).EnsureSuccessStatusCode();
    }

    private static async Task<List<Guid>> PdfAttachmentsAsync(HttpClient client, Guid itemId)
    {
        var response = await client.GetAsync($"/api/items/{itemId}/attachments");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        return body.EnumerateArray()
            .Where(a => a.GetProperty("role").ValueKind != System.Text.Json.JsonValueKind.Null
                && a.GetProperty("role").GetString() == "pdf")
            .Select(a => a.GetProperty("id").GetGuid())
            .ToList();
    }

    private static int CountStoredFiles(string root) =>
        Directory.Exists(root) ? Directory.GetFiles(root, "*", SearchOption.AllDirectories).Length : 0;
}
