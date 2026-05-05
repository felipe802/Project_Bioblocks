# Status do Fork — `felipe802/Project_Bioblocks`

> **Gerado em:** 05/05/2026  
> **Branch analisado:** `main` do fork (`felipe802/Project_Bioblocks`)  
> **Referência (upstream):** `main` do repositório original (`Edutesc/Project_Bioblocks`)

---

## ✅ Resultado: 3 commits *ahead*, 0 commits *behind*

O `main` do seu fork está **3 commits à frente** do `main` do upstream e totalmente atualizado (não está atrás de nenhum commit do original).

### Endpoint correto para comparação via API GitHub

```
GET https://api.github.com/repos/felipe802/Project_Bioblocks/compare/Edutesc:main...main
```

> **Por que o endpoint anterior `/compare/main...HEAD` retornou vazio?**  
> `main...HEAD` compara o branch `main` com ele mesmo (HEAD já aponta para `main`), resultando em comparação vazia. O correto é usar o repositório pai/upstream como base: `Edutesc:main...main`.

Link direto de comparação no GitHub:  
[https://github.com/felipe802/Project_Bioblocks/compare/Edutesc:main...main](https://github.com/felipe802/Project_Bioblocks/compare/Edutesc:main...main)

---

## 📋 Os 3 commits *ahead*

### Commit 1 de 3

| Campo        | Valor |
|-------------|-------|
| **SHA curto**   | `ebd5ce7d` |
| **SHA completo** | `ebd5ce7d6067fc578a19a52c4885260661a8a658` |
| **Mensagem**    | `Merge pull request #37 from Edutesc/dev — Dev` |
| **Autor**       | Luciano Puzer (`Edutesc`) |
| **Data**        | 17/03/2026 às 19:24 UTC |
| **Link**        | [ver no GitHub](https://github.com/felipe802/Project_Bioblocks/commit/ebd5ce7d6067fc578a19a52c4885260661a8a658) |

**Estatísticas:** `+10.313` adições / `-995` deleções — **30 arquivos** alterados

**Principais arquivos alterados:**

| Arquivo | Status | Mudanças |
|---------|--------|----------|
| `Assets/Firebase/Plugins/Firebase.App.pdb.meta` | adicionado | +89 |
| `Assets/Firebase/Plugins/Firebase.Auth.pdb.meta` | adicionado | +89 |
| `Assets/Firebase/Plugins/Firebase.Firestore.pdb.meta` | adicionado | +89 |
| `Assets/Firebase/Plugins/Firebase.Platform.pdb.meta` | adicionado | +89 |
| `Assets/Firebase/Plugins/Firebase.Storage.pdb.meta` | adicionado | +89 |
| `Assets/Firebase/Plugins/Firebase.TaskExtension.pdb.meta` | adicionado | +89 |
| `Assets/Firebase/Plugins/iOS/Firebase.App.pdb.meta` | adicionado | +89 |
| `Assets/Firebase/Plugins/iOS/Firebase.Auth.pdb.meta` | adicionado | +89 |
| `Assets/Firebase/Plugins/iOS/Firebase.Firestore.pdb.meta` | adicionado | +89 |
| `Assets/Firebase/Plugins/iOS/Firebase.Storage.pdb.meta` | adicionado | +89 |
| `Assets/Firebase/Editor/Firebase.Editor.pdb.meta` | adicionado | +16 |
| `Assets/Plugins/Android/FirebaseApp.androidlib.meta` | adicionado | +32 |
| `Assets/Firebase/m2repository/**` (vários `.meta`) | modificado | — |

**Resumo do que foi feito:**  
Este commit incorpora o PR #37 (branch `dev` do Edutesc). Adicionou os arquivos `.pdb.meta` necessários para o correto funcionamento do **Firebase SDK** nos targets **Android** e **iOS**: Firebase App, Auth, Firestore, Platform, Storage e TaskExtension. Também adicionou o arquivo de biblioteca Android `FirebaseApp.androidlib.meta` e atualizou os metadados (`.meta`) dos diretórios do Firebase.

---

### Commit 2 de 3

| Campo        | Valor |
|-------------|-------|
| **SHA curto**   | `5da3e56d` |
| **SHA completo** | `5da3e56d9f9c2d1545dd130da6f71a1c6f894f3f` |
| **Mensagem**    | `Merge branch 'Edutesc:main' into main` |
| **Autor**       | Felipe Junqueira Leite (`felipe802`) |
| **Data**        | 19/04/2026 às 19:17 UTC |
| **Link**        | [ver no GitHub](https://github.com/felipe802/Project_Bioblocks/commit/5da3e56d9f9c2d1545dd130da6f71a1c6f894f3f) |

**Estatísticas:** `+93.837` adições / `-30.590` deleções — **130+ arquivos** alterados

**Principais arquivos alterados:**

| Arquivo | Status | Mudanças |
|---------|--------|----------|
| `.firebaserc` | adicionado | +5 |
| `.gitignore` | modificado | +2 / -12 |
| `Assets/Editor/QuestionCacheDiagnostic.cs` | adicionado | +74 |
| `Assets/Editor/Tests/FirestoreQuestionDatabaseTests.cs` | adicionado | +237 |
| `Assets/Editor/Tests/ImageCacheServiceTests.cs` | adicionado | +263 |
| `Assets/Editor/Tests/LevelCalculatorTests.cs` | adicionado | +262 |
| `Assets/Editor/Tests/Helpers/FakeAuthRepository.cs` | adicionado | +141 |
| `Assets/Editor/Tests/Helpers/FakeFirestoreQuestionRepository.cs` | adicionado | +99 |
| `Assets/Editor/Tests/Helpers/FakeFirestoreRepository.cs` | adicionado | +189 |
| `Assets/Editor/Tests/Helpers/FakeLiteDBManager.cs` | adicionado | +57 |
| `Assets/Editor/Tests/Helpers/FakePlayerLevelService.cs` | adicionado | +54 |
| `Assets/Editor/Tests/Helpers/FakeQuestionDatabase.cs` | adicionado | +99 |
| `Assets/Editor/Tests/Helpers/FakeQuestionLocalRepository.cs` | adicionado | +153 |
| `Assets/Editor/Tests/Helpers/FakeQuestionSyncService.cs` | adicionado | +88 |
| `Assets/Editor/Tests/Helpers/FakeStorageRepository.cs` | adicionado | +57 |
| `Assets/Editor/Tests/Helpers/QuestionTestHelpers.cs` | adicionado | +186 |

**Resumo do que foi feito:**  
Grande merge que sincronizou o fork com as mudanças do upstream Edutesc. Incorporou:
- **Configuração Firebase:** adicionou `.firebaserc` com as configurações dos projetos Firebase.
- **Ferramenta de diagnóstico:** `QuestionCacheDiagnostic.cs` — utilitário de editor Unity para diagnóstico do cache de questões.
- **Suite extensa de testes unitários:** novos testes para `FirestoreQuestionDatabase`, `ImageCacheService` e `LevelCalculator`.
- **Helpers de teste (Fakes):** 10 novos helpers (`FakeAuthRepository`, `FakeFirestoreRepository`, `FakeLiteDBManager`, `FakePlayerLevelService`, `FakeQuestionDatabase`, `FakeQuestionLocalRepository`, `FakeQuestionSyncService`, `FakeStorageRepository`, `FakeFirestoreQuestionRepository`, `QuestionTestHelpers`) para isolamento nos testes.

---

### Commit 3 de 3

| Campo        | Valor |
|-------------|-------|
| **SHA curto**   | `ecee770a` |
| **SHA completo** | `ecee770aabbfffc3b932f1e5ebbcb3551d8bdad0` |
| **Mensagem**    | `Merge branch 'Edutesc:main' into main` |
| **Autor**       | Felipe Junqueira Leite (`felipe802`) |
| **Data**        | 05/05/2026 às 18:01 UTC |
| **Link**        | [ver no GitHub](https://github.com/felipe802/Project_Bioblocks/commit/ecee770aabbfffc3b932f1e5ebbcb3551d8bdad0) |

**Estatísticas:** `+31.996` adições / `-14.068` deleções — **130+ arquivos** alterados

**Principais arquivos alterados:**

| Arquivo | Status | Mudanças |
|---------|--------|----------|
| `.firebaserc` | modificado | +8 / -3 |
| `.gitattributes` | modificado | +8 |
| `.gitignore` | modificado | +12 |
| `Assets/Editor/EnvironmentSetup.cs` | adicionado | +95 |
| `Assets/Editor/Tests/AuthFlowTests.cs` | adicionado | +532 |
| `Assets/Editor/Tests/AvatarCatalogTests.cs` | adicionado | +252 |
| `Assets/Editor/Tests/ProfileManagerDeleteAccountTests.cs` | adicionado | +464 |
| `Assets/Editor/Tests/QuestionLoadManagerTests.cs` | modificado | +149 / -91 |
| `Assets/Editor/Tests/PlayerLevelServiceTests.cs` | modificado | +98 / -14 |
| `Assets/Editor/Tests/PlayerLevelConfigTests.cs` | modificado | +79 |
| `Assets/Editor/Tests/LiteDBTests.cs` | modificado | +17 / -84 |
| `Assets/Editor/Tests/QuestionSetManagerTests.cs` | modificado | +6 / -5 |
| `Assets/Editor/Tests/Helpers/FakeAuthRepository.cs` | modificado | +56 / -25 |
| `Assets/Editor/Tests/Helpers/FakeFirestoreRepository.cs` | modificado | +115 / -5 |
| `Assets/Editor/Tests/Helpers/FakeLiteDBManager.cs` | modificado | -1 |
| `Assets/Editor/Tests/Helpers/FakeNavigationService.cs` | adicionado | +47 |
| `Assets/Editor/Tests/Helpers/FakeStatisticsProvider.cs` | adicionado | +45 |
| `Assets/Editor/Tests/Helpers/FakeAnsweredQuestionsManagerForAuth.cs` | adicionado | +28 |
| `Assets/Editor/Tests/Helpers/FakeStorageRepository.cs` | **removido** | -57 |
| `Assets/Images/Buttons/CancelButton.png` | adicionado | — |
| `Assets/Images/Buttons/CloseButton.png` | adicionado | — |

**Resumo do que foi feito:**  
Merge mais recente com o upstream, incorporando:
- **Configuração de ambiente:** `EnvironmentSetup.cs` (95 linhas) — script de editor Unity para configuração do ambiente Firebase/Google Services.
- **Atualização de configurações:** `.firebaserc`, `.gitattributes` e `.gitignore` expandidos.
- **Novos testes:** `AuthFlowTests` (532 linhas) — testes do fluxo de autenticação; `AvatarCatalogTests` (252 linhas); `ProfileManagerDeleteAccountTests` (464 linhas) — testes de exclusão de conta.
- **Atualizações nos testes existentes:** `QuestionLoadManagerTests`, `PlayerLevelServiceTests`, `PlayerLevelConfigTests`, `LiteDBTests`, `QuestionSetManagerTests`.
- **Novos helpers:** `FakeNavigationService`, `FakeStatisticsProvider`, `FakeAnsweredQuestionsManagerForAuth`.
- **Remoção:** `FakeStorageRepository.cs` (removido/refatorado).
- **Assets de imagem:** adicionou botões `CancelButton.png` e `CloseButton.png` na pasta `Assets/Images/Buttons/`.

---

## 🔍 Como identificar commits *ahead* na linha de comando

```bash
# Adicionar o upstream (se ainda não estiver configurado):
git remote add upstream https://github.com/Edutesc/Project_Bioblocks.git

# Buscar o upstream:
git fetch upstream

# Ver commits que estão no seu main mas não no upstream:
git log --oneline upstream/main..main

# Ver resumo dos arquivos alterados em relação ao upstream:
git diff --stat upstream/main..main
```

---

## 📌 Contexto

Este fork (`felipe802/Project_Bioblocks`) é derivado do repositório original `Edutesc/Project_Bioblocks`.  
Os 3 commits *ahead* são merges periódicos que Felipe fez para sincronizar o upstream com possíveis contribuições locais, mantendo o histórico de merge no fork.  
Como não há commits *behind*, o fork está completamente atualizado com o upstream — os commits *ahead* existem apenas na linha de histórico do fork (não no upstream).
