using System.Collections.Generic;
using QuestionSystem;

/// <summary>
/// Abstração de fonte de questões usada em runtime.
/// Permite trocar entre Firestore/LiteDB (prod/dev) e dados locais (preview mode)
/// sem alterar os consumidores (QuestionLoadManager, etc.).
/// A implementação concreta é escolhida no bootstrap do AppContext
/// com base em QuestionPreviewMode
/// </summary>
public interface IQuestionSource
{
    /// <summary>
    /// Retorna as questões de um banco específico.
    /// A chamada é síncrona — os dados devem estar prontos antes de chamar.
    /// </summary>
    List<Question> GetQuestionsForDatabankName(string databankName);
}
