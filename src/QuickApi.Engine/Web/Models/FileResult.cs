namespace QuickApi.Engine.Web.Models;

public record FileResult(byte[] FileContent, string? ContentType, string FileDownloadName);