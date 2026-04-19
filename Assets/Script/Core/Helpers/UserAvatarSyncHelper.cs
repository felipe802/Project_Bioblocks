using UnityEngine;

/// <summary>
/// Helper estático para sincronizar mudanças de avatar entre ProfileScene e UserTopBar
/// Segue o mesmo padrão do projeto para manter consistência
/// </summary>
public static class UserAvatarSyncHelper
{
    /// <summary>
    /// Notifica a UserTopBar que o avatar foi atualizado
    /// Deve ser chamado após o upload bem-sucedido de uma nova imagem no ProfileImageManager
    /// </summary>

    public static void NotifyAvatarChanged(string newImageUrl, UserHeaderManager userHeader = null)
    {
        if (userHeader == null)
            userHeader = Object.FindFirstObjectByType<UserHeaderManager>();

        if (userHeader != null)
        {
            userHeader.UpdateAvatarFromUrl(newImageUrl);
            Debug.Log($"[AvatarSync] UserTopBar notificada: {newImageUrl}");
        }
        else
        {
            Debug.LogWarning("[AvatarSync] UserTopBarManager não disponível");
        }
    }

    public static void RefreshAvatar(UserHeaderManager userHeader = null)
    {
        if (userHeader == null)
            userHeader = Object.FindFirstObjectByType<UserHeaderManager>();

        if (userHeader != null)
        {
            userHeader.RefreshUserAvatar();
            Debug.Log("[AvatarSync] Avatar atualizado");
        }
        else
        {
            Debug.LogWarning("[AvatarSync] UserTopBarManager não disponível");
        }
    }
}