using System.Collections.Generic;

namespace QuestionSystem
{
    /// <summary>
    /// Representa o conjunto de dicas disponíveis para uma questão.
    /// Cada campo corresponde a uma aba na cena de hints.
    /// Campos nulos = aba desabilitada na UI.
    /// </summary>
    [System.Serializable]
    public class QuestionHint
    {
        public string text;       // Explicação em texto (aba "Explicação")
        public string imagePath;  // Caminho da imagem  (aba "Imagem")
        public string videoUrl;   // URL do vídeo       (aba "Vídeo")
        public string link;       // URL de referência  (aba "Referência")

        // Verdadeiro se ao menos um tipo de hint está disponível.
        // Útil para decidir se o botão "Ver Dica" aparece na QuestionScene.
        public bool HasAnyHint =>
            !string.IsNullOrEmpty(text)      ||
            !string.IsNullOrEmpty(imagePath) ||
            !string.IsNullOrEmpty(videoUrl)  ||
            !string.IsNullOrEmpty(link);

        // Retorna quais tipos de hint estão disponíveis.
        // O controller da cena de hints usa isso para montar as abas.
        public List<HintType> GetAvailableTypes()
        {
            var types = new List<HintType>();
            if (!string.IsNullOrEmpty(text))      types.Add(HintType.Text);
            if (!string.IsNullOrEmpty(imagePath)) types.Add(HintType.Image);
            if (!string.IsNullOrEmpty(videoUrl))  types.Add(HintType.Video);
            if (!string.IsNullOrEmpty(link))      types.Add(HintType.Link);
            return types;
        }
    }

    public enum HintType
    {
        Text,
        Image,
        Video,
        Link
    }
}