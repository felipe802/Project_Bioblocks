using System;
using System.Threading.Tasks;

public class ImageUploadConfig
{
    public string ImagePath          { get; set; }  
    public string DestinationFolder  { get; set; }  
    public string FileNamePrefix     { get; set; }  
    public int    MaxSizeBytes       { get; set; } 
    public string OldImageUrl        { get; set; }  
    public Action<string> OnProgress { get; set; }  
    public Func<string, Task>   OnCompleted { get; set; }
    public Action<string> OnFailed   { get; set; } 
}