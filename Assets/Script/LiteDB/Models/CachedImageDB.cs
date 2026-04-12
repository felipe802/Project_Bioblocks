using System;
using LiteDB;

public class CachedImageDB
{
    [BsonId]
    public string ImageUrl { get; set; }
    public string LocalPath { get; set; }
    public DateTime CachedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public long FileSizeBytes { get; set; }
}