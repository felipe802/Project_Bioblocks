using Firebase.Firestore;

[System.Serializable]
[FirestoreData]
public class Ranking
{
    [FirestoreProperty("userId")]
    public string UserId { get; set; } 

    [FirestoreProperty("nickName")]
    public string userName { get; set; }

    [FirestoreProperty("score")]
    public int userScore { get; set; }

    [FirestoreProperty("weekScore")]
    public int userWeekScore { get; set; }

    [FirestoreProperty("profileImageUrl")]
    public string profileImageUrl { get; set; }

    // Construtores existentes mantidos para compatibilidade
    public Ranking(string userName, int userScore, string profileImageUrl) { ... }
    public Ranking(string userName, int userScore, int userWeekScore, string profileImageUrl) { ... }

    // Construtor vazio necessário para o FirestoreData desserializar
    public Ranking() { }
}