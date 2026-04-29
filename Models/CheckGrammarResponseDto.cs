namespace MD_Note_API.Models;

public class CheckGrammarResponseDto {
    public bool HasErrors {get; set;}
    public List<string> Errors {get; set;}
}