using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;
using Firebase.Firestore;
using System.Linq;

public class QuestionSceneBonusManager
{
    private const string COLLECTION_NAME = "QuestionSceneBonus";
    private const string ACTIVE_BONUSES_FIELD = "ActiveBonuses";
    private FirebaseFirestore db;

    public QuestionSceneBonusManager()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public async Task ActivateBonus(string userId, string bonusType, float durationInSeconds, int multiplier)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("QuestionSceneBonusManager: UserId é nulo ou vazio");
            return;
        }

        try
        {
            long expirationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (long)durationInSeconds;
            Dictionary<string, object> bonusData = new Dictionary<string, object>
        {
            { "BonusType", bonusType },
            { "BonusMultiplier", multiplier },
            { "ExpirationTimestamp", expirationTimestamp },
            { "ActivatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };

            DocumentReference docRef = FirebaseFirestore.DefaultInstance.Collection("QuestionSceneBonus").Document(userId);
            DocumentSnapshot existing = await docRef.GetSnapshotAsync();
            if (existing.Exists)
            {
                Dictionary<string, object> data = existing.ToDictionary();

                if (data.ContainsKey("ActiveBonuses"))
                {
                    List<object> activeBonuses = data["ActiveBonuses"] as List<object>;
                    if (activeBonuses != null)
                    {
                        bool updated = false;
                        List<Dictionary<string, object>> updatedBonuses = new List<Dictionary<string, object>>();

                        foreach (object bonusObj in activeBonuses)
                        {
                            Dictionary<string, object> existingBonus = bonusObj as Dictionary<string, object>;
                            if (existingBonus != null)
                            {
                                if (existingBonus.ContainsKey("BonusType") && existingBonus["BonusType"].ToString() == bonusType)
                                {
                                    updatedBonuses.Add(bonusData);
                                    updated = true;
                                }
                                else
                                {
                                    updatedBonuses.Add(existingBonus);
                                }
                            }
                        }

                        if (!updated)
                        {
                            updatedBonuses.Add(bonusData);
                        }

                        await docRef.UpdateAsync(new Dictionary<string, object>
                    {
                        { "ActiveBonuses", updatedBonuses }
                    });

                        return;
                    }
                }

                await docRef.UpdateAsync(new Dictionary<string, object>
            {
                { "ActiveBonuses", new List<Dictionary<string, object>> { bonusData } }
            });
            }
            else
            {
                await docRef.SetAsync(new Dictionary<string, object>
            {
                { "ActiveBonuses", new List<Dictionary<string, object>> { bonusData } },
                { "UserId", userId }
            });
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"QuestionSceneBonusManager: Erro ao ativar bônus: {e.Message}\n{e.StackTrace}");
            throw;
        }
    }

    public async Task<List<Dictionary<string, object>>> GetActiveBonuses(string userId)
    {
        userId = GetValidUserId(userId);
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("QuestionSceneBonusManager: UserId é nulo ou vazio");
            return new List<Dictionary<string, object>>();
        }

        try
        {
            DocumentReference docRef = db.Collection(COLLECTION_NAME).Document(userId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return new List<Dictionary<string, object>>();
            }

            Dictionary<string, object> data = snapshot.ToDictionary();

            if (!data.ContainsKey(ACTIVE_BONUSES_FIELD))
            {
                return new List<Dictionary<string, object>>();
            }

            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (data[ACTIVE_BONUSES_FIELD] is List<object> bonusList)
            {
                foreach (object bonusObj in bonusList)
                {
                    if (bonusObj is Dictionary<string, object> bonusDict)
                    {
                        if (bonusDict.ContainsKey("ExpirationTimestamp"))
                        {
                            long expirationTimestamp = Convert.ToInt64(bonusDict["ExpirationTimestamp"]);

                            if (currentTimestamp < expirationTimestamp)
                            {
                                result.Add(bonusDict);
                            }
                        }
                    }
                }
            }

            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"QuestionSceneBonusManager: Erro ao obter bônus ativos: {e.Message}");
            return new List<Dictionary<string, object>>();
        }
    }

    public async Task<int> GetCombinedMultiplier(string userId)
    {
        try
        {
            List<Dictionary<string, object>> activeBonuses = await GetActiveBonuses(userId);

            if (activeBonuses.Count == 0)
            {
                return 1;
            }

            int combinedMultiplier = 1;

            foreach (var bonus in activeBonuses)
            {
                if (bonus.ContainsKey("BonusMultiplier"))
                {
                    int multiplier = Convert.ToInt32(bonus["BonusMultiplier"]);
                    combinedMultiplier *= multiplier;
                }
            }

            return combinedMultiplier;
        }
        catch (Exception e)
        {
            Debug.LogError($"QuestionSceneBonusManager: Erro ao calcular multiplicador: {e.Message}");
            return 1;
        }
    }

    public async Task DeactivateAllBonuses(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("QuestionSceneBonusManager: UserId é nulo ou vazio");
            return;
        }

        try
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { ACTIVE_BONUSES_FIELD, new List<object>() },
                { "IsActive", false },
                { "UpdatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };

            DocumentReference docRef = db.Collection(COLLECTION_NAME).Document(userId);
            await docRef.SetAsync(data, SetOptions.MergeAll);
        }
        catch (Exception e)
        {
            Debug.LogError($"QuestionSceneBonusManager: Erro ao desativar bônus: {e.Message}");
        }
    }

    public async Task DeactivateBonus(string userId, string bonusType)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("QuestionSceneBonusManager: UserId é nulo ou vazio");
            return;
        }

        try
        {
            List<Dictionary<string, object>> activeBonuses = await GetActiveBonuses(userId);
            activeBonuses.RemoveAll(b => b.ContainsKey("BonusType") && b["BonusType"].ToString() == bonusType);
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { ACTIVE_BONUSES_FIELD, activeBonuses },
                { "IsActive", activeBonuses.Count > 0 },
                { "UpdatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };

            DocumentReference docRef = db.Collection(COLLECTION_NAME).Document(userId);
            await docRef.SetAsync(data, SetOptions.MergeAll);
        }
        catch (Exception e)
        {
            Debug.LogError($"QuestionSceneBonusManager: Erro ao desativar bônus específico: {e.Message}");
        }
    }

    public async Task UpdateExpirationTimestamp(string userId, float remainingSeconds)
    {
        await UpdateBonusExpirations(userId, remainingSeconds);
    }

    public async Task UpdateBonusExpirations(string userId, float remainingSeconds)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("QuestionSceneBonusManager: UserId é nulo ou vazio");
            return;
        }

        try
        {
            List<Dictionary<string, object>> activeBonuses = await GetActiveBonuses(userId);

            if (activeBonuses.Count == 0)
            {
                return;
            }

            long newExpirationTimestamp = DateTimeOffset.UtcNow.AddSeconds(remainingSeconds).ToUnixTimeSeconds();

            foreach (var bonus in activeBonuses)
            {
                bonus["ExpirationTimestamp"] = newExpirationTimestamp;
            }

            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { ACTIVE_BONUSES_FIELD, activeBonuses },
                { "UpdatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };

            DocumentReference docRef = db.Collection(COLLECTION_NAME).Document(userId);
            await docRef.SetAsync(data, SetOptions.MergeAll);
        }
        catch (Exception e)
        {
            Debug.LogError($"QuestionSceneBonusManager: Erro ao atualizar expiração: {e.Message}");
        }
    }

    public async Task<bool> HasAnyActiveBonus(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        try
        {
            List<Dictionary<string, object>> activeBonuses = await GetActiveBonuses(userId);
            return activeBonuses.Count > 0;
        }
        catch (Exception e)
        {
            Debug.LogError($"QuestionSceneBonusManager: Erro ao verificar bônus ativos: {e.Message}");
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetEarliestExpiringBonus(string userId)
    {
        try
        {
            List<Dictionary<string, object>> activeBonuses = await GetActiveBonuses(userId);

            if (activeBonuses.Count == 0)
            {
                return null;
            }

            Dictionary<string, object> earliestBonus = null;
            long earliestExpiration = long.MaxValue;

            foreach (var bonus in activeBonuses)
            {
                if (bonus.ContainsKey("ExpirationTimestamp"))
                {
                    long expiration = Convert.ToInt64(bonus["ExpirationTimestamp"]);

                    if (expiration < earliestExpiration)
                    {
                        earliestExpiration = expiration;
                        earliestBonus = bonus;
                    }
                }
            }

            return earliestBonus;
        }
        catch (Exception e)
        {
            Debug.LogError($"QuestionSceneBonusManager: Erro ao obter bônus mais próximo de expirar: {e.Message}");
            return null;
        }
    }

    public async Task<float> GetRemainingTime(string userId)
    {
        try
        {
            Dictionary<string, object> earliestBonus = await GetEarliestExpiringBonus(userId);

            if (earliestBonus == null || !earliestBonus.ContainsKey("ExpirationTimestamp"))
            {
                return 0;
            }

            long expirationTimestamp = Convert.ToInt64(earliestBonus["ExpirationTimestamp"]);
            long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            return Math.Max(0, expirationTimestamp - currentTimestamp);
        }
        catch (Exception e)
        {
            Debug.LogError($"QuestionSceneBonusManager: Erro ao obter tempo restante: {e.Message}");
            return 0;
        }
    }

    public async Task<bool> IsBonusActive(string userId, string bonusType = null)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("QuestionSceneBonusManager: UserId é nulo ou vazio");
            return false;
        }

        try
        {
            List<Dictionary<string, object>> activeBonuses = await GetActiveBonuses(userId);

            if (bonusType != null)
            {
                return activeBonuses.Any(b =>
                    b.ContainsKey("BonusType") &&
                    b["BonusType"].ToString() == bonusType);
            }
            else
            {
                return activeBonuses.Count > 0;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"QuestionSceneBonusManager: Erro ao verificar bônus ativo: {e.Message}");
            return false;
        }
    }

    public async Task IncrementBonusCounter(string userId, string bonusName)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("QuestionSceneBonusManager: UserId é nulo ou vazio");
            return;
        }

        try
        {
            DocumentReference docRef = db.Collection(COLLECTION_NAME).Document(userId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            int currentCount = 0;

            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();

                if (data.ContainsKey(bonusName))
                {
                    currentCount = Convert.ToInt32(data[bonusName]);
                }
            }

            currentCount++;
            Dictionary<string, object> updateData = new Dictionary<string, object>
        {
            { bonusName, currentCount }
        };

            await docRef.SetAsync(updateData, SetOptions.MergeAll);

            if (bonusName == "correctAnswerBonusCounter" && currentCount >= 3)
            {
                await docRef.UpdateAsync(new Dictionary<string, object>
            {
                { bonusName, 0 }
            });
                await GrantSpecialBonus(userId);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"QuestionSceneBonusManager: Erro ao incrementar contador de bônus: {e.Message}");
        }
    }

    private async Task GrantSpecialBonus(string userId)
    {
        try
        {
            UserBonusManager userBonusManager = new UserBonusManager();
            await userBonusManager.IncrementBonusCount(userId, "specialBonus", 1, true);
        }
        catch (Exception e)
        {
            Debug.LogError($"QuestionSceneBonusManager: Erro ao conceder SpecialBonus: {e.Message}");
        }
    }

    private string GetValidUserId(string userId)
    {
        if (AppContext.IsReady && AppContext.Auth != null)
        {
            string appContextUserId = AppContext.Auth.CurrentUserId;
            if (!string.IsNullOrEmpty(appContextUserId))
            {
                Debug.Log($"[QuestionSceneBonusManager] Usando userId do AppContext: {appContextUserId}");
                return appContextUserId;
            }
        }

        if (UserDataStore.CurrentUserData != null && 
            !string.IsNullOrEmpty(UserDataStore.CurrentUserData.UserId))
        {
            Debug.Log($"[QuestionSceneBonusManager] Usando userId do UserDataStore: {UserDataStore.CurrentUserData.UserId}");
            return UserDataStore.CurrentUserData.UserId;
        }

        if (!string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning($"[QuestionSceneBonusManager] Nenhuma outra fonte disponível, usando parâmetro: {userId}");
            return userId;
        }

        Debug.LogError("[QuestionSceneBonusManager] ❌ NENHUM userId válido disponível! Verificar:");
        Debug.LogError("  - AppContext.IsReady?");
        Debug.LogError("  - AppContext.Auth.CurrentUserId?");
        Debug.LogError("  - UserDataStore.CurrentUserData?");
        return null;
    }



    



}