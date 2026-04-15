using System;
using System.Collections.Generic;
using LiteDB;
using QuestionSystem;

/// <summary>
/// Modelo de persistência local (LiteDB) para questões baixadas do Firestore.
/// Segue o mesmo padrão de UserDataDB: campos planos + métodos FromDomain/ToDomain.
/// </summary>
public class QuestionDB
{
    // ── Chave primária ────────────────────────────────────────────────────────
    /// <summary>globalId do Firestore (ex: "acidsBase_001"). Usado como _id no LiteDB.</summary>
    [BsonId]
    public string GlobalId { get; set; }

    // ── Campos de domínio (espelho de Question) ───────────────────────────────
    public string   QuestionDatabankName  { get; set; }
    public string   QuestionText          { get; set; }
    public string[] Answers               { get; set; }
    public int      CorrectIndex          { get; set; }
    public int      QuestionNumber        { get; set; }
    public bool     IsImageAnswer         { get; set; }
    public bool     IsImageQuestion       { get; set; }
    public string   QuestionImagePath     { get; set; }
    public int      QuestionLevel         { get; set; }
    public bool     QuestionInDevelopment { get; set; }

    // ── Campos novos do Firestore ─────────────────────────────────────────────
    public string       Topic        { get; set; }
    public string       DisplayName  { get; set; }
    public string       Subtopic     { get; set; }
    public string       BloomLevel   { get; set; }
    public List<string> ConceptTags  { get; set; }
    public List<string> Prerequisites{ get; set; }

    // QuestionHint armazenada de forma plana para evitar objeto aninhado no LiteDB
    public string HintImagePath { get; set; }
    public string HintLink      { get; set; }
    public string HintText      { get; set; }
    public string HintVideoUrl  { get; set; }

    // ── Metadados de cache ────────────────────────────────────────────────────
    /// <summary>Momento (local) em que este documento foi salvo no LiteDB.</summary>
    public DateTime CachedAt { get; set; }

    // ── Construtor padrão ─────────────────────────────────────────────────────
    public QuestionDB()
    {
        ConceptTags   = new List<string>();
        Prerequisites = new List<string>();
    }

    // ── Conversão ─────────────────────────────────────────────────────────────

    public static QuestionDB FromDomain(Question q)
    {
        return new QuestionDB
        {
            GlobalId              = string.IsNullOrEmpty(q.globalId) ? $"{q.questionDatabankName}_{q.questionNumber:D3}" : q.globalId,
            QuestionDatabankName  = q.questionDatabankName  ?? "",
            QuestionText          = q.questionText          ?? "",
            Answers               = q.answers               ?? new string[0],
            CorrectIndex          = q.correctIndex,
            QuestionNumber        = q.questionNumber,
            IsImageAnswer         = q.isImageAnswer,
            IsImageQuestion       = q.isImageQuestion,
            QuestionImagePath     = q.questionImagePath     ?? "",
            QuestionLevel         = q.questionLevel,
            QuestionInDevelopment = q.questionInDevelopment,
            Topic                 = q.topic                 ?? "",
            DisplayName           = q.displayName           ?? "",
            Subtopic              = q.subtopic              ?? "",
            BloomLevel            = q.bloomLevel            ?? "unclassified",
            ConceptTags           = q.conceptTags           ?? new List<string>(),
            Prerequisites         = q.prerequisites         ?? new List<string>(),
            HintImagePath         = q.questionHint?.imagePath ?? "",
            HintLink              = q.questionHint?.link      ?? "",
            HintText              = q.questionHint?.text      ?? "",
            HintVideoUrl          = q.questionHint?.videoUrl  ?? "",
            CachedAt              = DateTime.Now
        };
    }

    public Question ToDomain()
    {
        return new Question
        {
            globalId              = GlobalId,
            questionDatabankName  = QuestionDatabankName,
            questionText          = QuestionText,
            answers               = Answers ?? new string[0],
            correctIndex          = CorrectIndex,
            questionNumber        = QuestionNumber,
            isImageAnswer         = IsImageAnswer,
            isImageQuestion       = IsImageQuestion,
            questionImagePath     = QuestionImagePath,
            questionLevel         = QuestionLevel,
            questionInDevelopment = QuestionInDevelopment,
            topic                 = Topic,
            displayName           = DisplayName,
            subtopic              = Subtopic,
            bloomLevel            = BloomLevel,
            conceptTags           = ConceptTags   ?? new List<string>(),
            prerequisites         = Prerequisites ?? new List<string>(),
            questionHint = new QuestionHint
            {
                imagePath = HintImagePath,
                link      = HintLink,
                text      = HintText,
                videoUrl  = HintVideoUrl
            }
        };
    }
}
