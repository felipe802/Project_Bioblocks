using Firebase.Firestore;

[System.Serializable]
[FirestoreData(UnknownPropertyHandling = UnknownPropertyHandling.Ignore)]
public class Ranking
{
    [FirestoreProperty("nickName")]
    public string userName { get; set; }

    [FirestoreProperty("score")]
    public int userScore { get; set; }

    [FirestoreProperty("weekScore")]
    public int userWeekScore { get; set; }

    [FirestoreProperty("profileImageUrl")]
    public string profileImageUrl { get; set; }

    // Construtor vazio obrigatório para o FirestoreData desserializar
    public Ranking() { }

    public Ranking(string userName, int userScore, int userWeekScore, string profileImageUrl)
    {
        this.userName = userName;
        this.userScore = userScore;
        this.userWeekScore = userWeekScore;
        this.profileImageUrl = profileImageUrl;
    }
}