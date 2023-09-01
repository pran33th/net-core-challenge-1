using FixWithCustomSerialization.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FixWithCustomSerialization.Controllers;

[ApiController]
[Route("[controller]")]
public class MediaController : ControllerBase
{
    private readonly ILogger<MediaController> _logger;
    private readonly IDBService DBService;

    public MediaController(ILogger<MediaController> logger, IDBService iDBService)
    {
        _logger = logger;
        DBService = iDBService;
    }

    [HttpPost]
    public async Task<IActionResult> UpdateMedia([FromBody] MediaFile media)
    {

        await DBService.UpdateMediaAsync(media);
        return Ok(media);

        /* todo: Implement the method
         * 
         * 1. media will be contained in the ImageDataB64 prop. It should have been already saved to the webroots folder by the "read" method of MediaFileJsonConverter
         * 2. Save the media object in Mongo db
         * 3. return the MediaFile object
         * 4. We expect the "write" method of MediaFileJsonConverter to create the public URL
         * 
         * */
    }

    [HttpGet("{Name}")]
    public async Task<IActionResult> GetMedia(string Name)
    {
        var mediaFile = await DBService.GetMediaAsync(Name);
        return Ok(mediaFile);

        /*
         * return _mongoDb.getcollection<MediaFile>.Find(m=>m.Name==Name).SingleAsync();
         * 
         * 1. We expect the "write" method of MediaFileJsonConverter to create the public URL
         * 
         */
    }
}

/// <summary>
/// todo: implement MediaFileJsonConverter  

/// </summary>
/// 
[JsonConverter(typeof(MediaFileJsonConverter))]
public class MediaFileJsonConverter : JsonConverter<MediaFile>
{
    public override MediaFile Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException();
        }

        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException();
        }

        MediaFile mediaFile = new MediaFile();
        string path = Path.Combine(@"wwwroot", "Files");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        while (reader.Read())
        {

            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return mediaFile;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                if (propertyName == "Name")
                {
                    string? mediaName = reader.GetString();
                    if (mediaName is not null)
                    {
                        mediaFile.Name = mediaName;
                    }
                    else
                    {
                        throw new JsonException();
                    }
                }

                if (propertyName == "ImageDataB64")
                {
                    string? mediaData = reader.GetString();
                    if (mediaData is not null)
                    {
                        path = Path.Combine(path, mediaFile.Name);
                        Byte[] byteArray = Convert.FromBase64String(mediaData);
                        Stream stream = new MemoryStream(byteArray);
                        FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        stream.Position = 0;
                        stream.CopyTo(fileStream);
                        stream.Dispose();
                        fileStream.Dispose();

                    }
                    else
                    {
                        throw new JsonException();
                    }

                }
            }
        }

        throw new JsonException();

    }

    public override void Write(Utf8JsonWriter writer, MediaFile mediaFile, JsonSerializerOptions options)
    {
        var serverUrl = "localhost:5290";
        writer.WriteStartObject();

        writer.WriteString("Name", mediaFile.Name);

        var PublicUrl = serverUrl + "\\Files\\" + mediaFile;

        writer.WriteString("PublicUrl", PublicUrl);

        writer.WriteEndObject();
    }
}

public class MediaFile
{
    /// <summary>
    /// todo: We want media name to be Unique, Please enforce using MongoDb Unique Index
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// todo: donot Save in database
    /// </summary>
    public string ImageDataB64 { get; set; } = "";

    /// <summary>
    /// todo: donot Save in database, Generate dynamically using 
    /// </summary>
    public string PublicUrl { get; set; } = "";


}



