using System;

public class PendingUploadDB
{
    public string Id           { get; set; } // userId — um por usuário
    public string LocalPath    { get; set; } // caminho local da imagem salva
    public string OldImageUrl  { get; set; } // URL antiga para deletar após upload
    public string UserId       { get; set; }
    public DateTime CreatedAt  { get; set; }
}