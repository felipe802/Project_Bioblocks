# Firebase Setup Guide

Este guia descreve como configurar o Firebase localmente para desenvolvimento.

## Estrutura de Pastas

Os arquivos de configuração do Firebase não estão versionados no Git por segurança (contêm API keys). Você deve criar as pastas e adicionar seu próprio arquivo.

```
project_bioblocks/
├── Firebase/
│   └── Dev/
│      └── google-services.json
├── Assets (já existe...)
│

## Passo a Passo

### 1. Clonar o Repositório

```bash
git clone https://github.com/Edutesc/Project_Bioblocks.git
cd project_bioblocks
```

A branch padrão é `dev` (já configurada no GitHub).

### 2. Criar as Pastas de Configuração

```bash
mkdir -p Firebase/Dev
```

### 3. Adicionar o Arquivo do Firebase

#### Para Desenvolvimento (Simulador do Unity)

Obtenha o arquivo `google-services.json` do projeto Firebase **microlearning-dev-79c0c** e coloque em:

```
Firebase/Dev/google-services.json
```

### 4. Abrir o Projeto no Unity

1. Abra o **Unity Hub**
2. Clique em **Add** e selecione a pasta `project_bioblocks`
3. Abra o projeto

Ao entrar no editor:
- `FirebaseEnvironmentSetup` será executado automaticamente
- O arquivo `google-services.json` correto será copiado para `Assets/`
- O console mostrará: `✓ Firebase Dev (microlearning-dev-79c0c) configurado`

### 5. Rodar o App

1. Na cena **Initialization**, clique em **Play**
2. O app detectará automaticamente o ambiente (Dev por padrão)
3. Você será levado à **LoginViewClique em **Registrar** para criar uma nova conta

## Configuração de Ambiente

Por padrão, o projeto é configurado para **Dev**:
- **Arquivo padrão:** `Assets/Resources/EnvironmentConfig.asset`
- **Firebase:** `microlearning-dev-79c0c`

## Solução de Problemas

### Erro: "google-services.json não encontrado"

**Causa:** Arquivo não foi colocado em `Firebase/Dev/` ou está com o nome errado.

**Solução:**
1. Verifique se o arquivo existe em `Firebase/Dev/google-services.json`
2. Verifique o nome exato (case-sensitive)
3. Feche e reabra o Unity Editor

### Erro: "Firebase dependencies unavailable"

**Causa:** Arquivo `.json` está corrompido ou contém projeto Firebase errado.

**Solução:**
1. Verifique que o projeto Firebase é `microlearning-dev-79c0c` (Dev)
2. Baixe novamente o arquivo do Firebase Console
3. Substitua o arquivo

### Erro ao Registrar Conta

**Causa:** Firebase não está configurado corretamente ou não há acesso à internet.

**Solução:**
1. Verifique conexão com a internet
2. Verifique console do Unity: procure por `Firebase disponível` e `Firebase {Dev|Prod} configurado`
3. Se o erro persistir, verifique que `AppContext` foi inicializado com sucesso

## Estrutura do Fluxo de Inicialização

```
Initialization Scene
  ↓
AppContext.InitializeServices()
  ├─ FirebaseEnvironmentSetup valida ambiente
  ├─ Copia google-services.json correto para Assets/
  ├─ Inicializa Firebase (Auth, Firestore)
  └─ Carrega dados do usuário (se autenticado)
  ↓
LoginView (se não autenticado) ou PathwayScene (se autenticado)
```

## Referência Rápida

| Variável | Dev |
|----------|-----|
| **Firebase Project** | `microlearning-dev-79c0c` |
| **Bundle ID (Android)** | `com.edutesc.bioblocks_dev` |
| **Bundle ID (iOS)** | `com.edutesc.bioblocks-dev` | 
| **App Name** | BioBlocks Dev | BioBlocks |
| **Arquivo Padrão** | `Firebase/Dev/google-services.json` |

## Dúvidas?

Se encontrar problemas durante o setup, verifique:
1. Se o arquivo `.json` está no lugar correto
2. Se o projeto Firebase corresponde ao ambiente
3. Se há conexão com a internet
4. Se o Unity foi reaberto após adicionar os arquivos
