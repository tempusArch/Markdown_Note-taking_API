using Markdig;
using MD_Note_API.Models;

namespace MD_Note_API.Services;

public class NoteService {
    private static string basePath = Directory.GetCurrentDirectory() + "//Data";

    public bool CreateNote(CreateNoteDto dto) {
        try {
            string counterFilePath = Path.Combine(basePath, "counter.txt"); 
            int number = File.Exists(counterFilePath) ? int.Parse(File.ReadAllText(counterFilePath)) : 1;

            string fileName = $"{number}.md";
            string filePath = Path.Combine(basePath, fileName);

            if (createFile(fileName, filePath)) {
                File.WriteAllText(filePath, dto.Content);
                
                number++;
                File.WriteAllText(counterFilePath, number.ToString());

                return true;
            } else
                return false;

        } catch (Exception e) {
            Console.WriteLine($"Failed at creating. Erro - " + e.Message);
            return false;
        }
    }



    public bool UpdateNote(string s, CreateNoteDto dto) {
        try {
            string fileName = $"{s}.md";
            string filePath = Path.Combine(basePath, fileName);

            if (File.Exists(filePath)) {
                File.WriteAllText(filePath, dto.Content);
                Console.WriteLine($"{fileName} updated successfully.");
                return true;
            } else {
                Console.WriteLine("File does not exist.");
                return false;
            }

        } catch (Exception e) {
            Console.WriteLine("Failed at updating. Error - " + e.Message);
            return false;
        }
    }

    public bool DeleteBlog(string s) {
        try {
            string fileName = $"{s}.json";
            string filePath = Path.Combine(basePath, fileName);

            if (File.Exists(filePath)) {
                File.Delete(filePath);
                Console.WriteLine($"{fileName} deleted successfully.");
                return true;
            } else {
                Console.WriteLine("File does not exist.");
                return false;
            }

        } catch (Exception e) {
            Console.WriteLine("Failed at deleting. Error - " + e.Message);
            return false;
        } 
    }

    #region helper methods
    private bool createFile(string fileName, string filePath) {
        try {
            if (!File.Exists(filePath)) 
                using (FileStream i = File.Create(filePath))
                    Console.WriteLine($"{fileName} created successfully.");
            
            return true;
        } catch (Exception e) {
            Console.WriteLine($"Failed at creating {fileName}. Error - " + e.Message);
            return false;
        }
    }
    #endregion
}