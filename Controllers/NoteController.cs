using System.Formats.Asn1;
using MD_Note_API.Models;
using MD_Note_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Markdig;
using Ganss.Xss;
using System.Text;
using System.Text.Json;

namespace MD_Note_API.Controllers;

[ApiController]
[Route("[controller]")]

public class NoteController : ControllerBase {
    private static string basePath = Directory.GetCurrentDirectory() + "//Data"; 
    private readonly HttpClient _httpClient  = new HttpClient();
    private readonly NoteService _noteService;
    public NoteController(HttpClient httpClient, NoteService noteService) {
        _httpClient = httpClient;
        _noteService = noteService;
    }

    [HttpPost]
    public ActionResult<CreateNoteDto> ECreateNote(CreateNoteDto dto) {
        if (_noteService.CreateNote(dto))
            return Created(string.Empty, dto);
        
        return BadRequest();
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<string>> EUploadMD([FromForm] UploadMdDto dto) {
        var file = dto.File;
        
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");
        
        var safeName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(safeName);

        if (!string.Equals(extension, ".md", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .md file is accepted");

        string filePath = Path.Combine(basePath, safeName);

        using (var stream = new FileStream(filePath, FileMode.Create)) 
            await file.CopyToAsync(stream);
      
        return Ok(safeName);  
    }

    [HttpGet]
    public ActionResult<string[]> EGetAll_MD_FileNames() {
        List<string> result = new List<string>();

        string[] fileNames = Directory.GetFiles(basePath, "*.md");

        foreach (string s in fileNames) {
            string oneName = s.Split('\\').Last();
            result .Add(oneName);
        }

        return Ok(result);
    }

    [HttpGet("{fileName}")]
    public ActionResult<string> EGetOneMDFileThenRenderItToHtml([FromRoute] string fileName) {
        if (string.IsNullOrEmpty(fileName))
            return BadRequest("File name is required in route");

        string fileNameWithExtension = $"{fileName}.md";
        string filePath = Path.Combine(basePath, fileNameWithExtension);

        if (!System.IO.File.Exists(filePath))
            return NotFound();
        else {
            var md = System.IO.File.ReadAllText(filePath);

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            var html = Markdown.ToHtml(md, pipeline);

            var sanitizer = new HtmlSanitizer();
            html = sanitizer.Sanitize(html);

            return Content(html, "text/html");
        }
    }

    [HttpPost("cheackGrammar/{fileName}")]
    public async Task<ActionResult<int>> ECheckGrammar([FromRoute] string fileName) {
        if (string.IsNullOrEmpty(fileName))
            return BadRequest("File name is required in route");

        string fileNameWithExtension = $"{fileName}.md";
        string filePath = Path.Combine(basePath, fileNameWithExtension);

        if (!System.IO.File.Exists(filePath))
            return NotFound(); 
            
        string md = System.IO.File.ReadAllText(filePath);
        md = Markdown.ToPlainText(md);

        var content = new FormUrlEncodedContent(new[] {
            new KeyValuePair<string, string>("text", md),
            new KeyValuePair<string, string>("language", "en-US")
        });

        var response = await _httpClient.PostAsync("https://api.languagetool.org/v2/check", content);

        var result = await response.Content.ReadAsStringAsync();

        return Ok(JsonSerializer.Deserialize<object>(result));

    }
}