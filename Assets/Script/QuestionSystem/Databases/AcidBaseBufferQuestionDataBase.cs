using System.Collections.Generic;
using QuestionSystem;

public class AcidBaseBufferQuestionDatabase : IQuestionDatabase
{
    private bool databaseInDevelopment = false;

    private List<Question> questions = new List<Question>
    {
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Segundo Arrhenius, o que caracteriza um ácido?",
            answers = new string[] {
                "Libera íons H+ em solução aquosa.",
                "Recebe prótons (H+) em solução aquosa.",
                "Libera íons OH- em solução aquosa.",
                "Recebe íons OH- em solução aquosa."
            },
            correctIndex = 0,
            questionNumber = 1,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_001",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A teoria de Arrhenius define ácidos como substâncias que, em solução aquosa, liberam íons H+ (ou H3O+)." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Segundo Arrhenius, o que caracteriza uma base?",
            answers = new string[] {
                "Libera íons H+ em solução aquosa.",
                "Recebe prótons (H+) em solução aquosa.",
                "Libera íons OH- em solução aquosa.",
                "Recebe íons OH- em solução aquosa."
            },
            correctIndex = 2,
            questionNumber = 2,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_002",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Segundo Arrhenius, bases liberam íons hidroxila (OH-) quando dissolvidas em água." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "De acordo com Brønsted-Lowry, o que é um ácido?",
            answers = new string[] {
                "Doador de prótons (H+).",
                "Receptor de prótons (H+).",
                "Doador de íons OH-. ",
                "Receptor de íons OH-."
            },
            correctIndex = 0,
            questionNumber = 3,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_003",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Brønsted-Lowry amplia o conceito: ácido é qualquer espécie química capaz de doar um próton (H+)." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "De acordo com Brønsted-Lowry, o que é uma base?",
            answers = new string[] {
                "Doador de prótons (H+).",
                "Receptor de prótons (H+).",
                "Doador de íons OH-. ",
                "Receptor de íons OH-."
            },
            correctIndex = 1,
            questionNumber = 4,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_004",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Para Brønsted-Lowry, a base é a espécie que aceita o próton (H+) doado pelo ácido." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "A água pode atuar como:",
            answers = new string[] {
                "Apenas ácido.",
                "Apenas base.",
                "Tanto ácido quanto base.",
                "Nem ácido nem base."
            },
            correctIndex = 2,
            questionNumber = 5,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_005",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A água é anfótera, ou seja, pode agir como ácido (doando H+) ou como base (recebendo H+), dependendo do meio." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "O que é a base conjugada do HCl?",
            answers = new string[] {
                "H<sup><size=150%> +</size></sup>",
                "Cl<sup><size=150%> -</size></sup>",
                "H<sub><size=150%>2</size></sub> O",
                "OH<sup><size=150%> -</size></sup>"
            },
            correctIndex = 1,
            questionNumber = 6,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_006",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A base conjugada é o que sobra do ácido após ele doar um próton. HCl doando H+ forma Cl-." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "H<sup><size=150%> +</size></sup>",
                "OH<sup><size=150%> -</size></sup>",
                "NH<sub><size=150%>4</size></sub><sup><size=150%> +</size></sup>",
                "NH<sub><size=150%>2</size></sub><sup><size=150%> -</size></sup>"
            },
            correctIndex = 2,
            questionNumber = 7,
            isImageAnswer = false,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/AcidBaseBufferDB/AcidBaseBufferDB_ImageQuestionContainer7",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_007",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Observe a reação: a espécie que recebe o H+ forma o ácido conjugado. A amônia (NH3) recebe um H+ e vira íon amônio (NH4+)." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Um ácido forte em solução aquosa:",
            answers = new string[] {
                "Se dissocia parcialmente.",
                "Se dissocia completamente.",
                "Não se dissocia.",
                "Forma ligações de hidrogênio."
            },
            correctIndex = 1,
            questionNumber = 8,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 3,
            questionInDevelopment = false,
            globalId = "acidsBase_008",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Ácidos fortes sofrem ionização completa (100%) em água." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Um ácido fraco em solução aquosa:",
            answers = new string[] {
                "Se dissocia completamente.",
                "Se dissocia parcialmente.",
                "Não se dissocia.",
                "Forma ligações iônicas."
            },
            correctIndex = 1,
            questionNumber = 9,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 3,
            questionInDevelopment = false,
            globalId = "acidsBase_009",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Ácidos fracos estabelecem um equilíbrio químico, pois se dissociam apenas parcialmente." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "A constante de equilíbrio (Keq) de uma reação indica:",
            answers = new string[] {
                "A velocidade da reação.",
                "A proporção de reagentes e produtos no equilíbrio.",
                "A energia de ativação da reação.",
                "A concentração dos reagentes."
            },
            correctIndex = 1,
            questionNumber = 10,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_010",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O Keq mede a proporção entre os produtos formados e os reagentes que restaram quando a reação atinge o equilíbrio." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "No equilíbrio, há mais reagentes do que produtos.",
                "No equilíbrio, há mais produtos do que reagentes.",
                "No equilíbrio, reagentes e produtos estão em quantidades iguais.",
                "A reação ocorre apenas no sentido direto."  
            },
            correctIndex = 0,
            questionNumber = 11,
            isImageAnswer = false,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/AcidBaseBufferDB/AcidBaseDB_ImageQuestionContainer11",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_011",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Se a constante for muito pequena (K < 1), o equilíbrio favorece os reagentes, indicando dissociação parcial." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "No equilíbrio, há mais reagentes do que produtos.",
                "No equilíbrio, há mais produtos do que reagentes.",
                "No equilíbrio, reagentes e produtos estão em quantidades iguais.",
                "A reação ocorre apenas no sentido direto."  
            },
            correctIndex = 1,
            questionNumber = 12,
            isImageAnswer = false,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/AcidBaseBufferDB/AcidBaseDB_ImageQuestionContainer12",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_012",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Se a constante for alta (K > 1), a reação tende fortemente a formar produtos." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "A força da base.",
                "A força do ácido.",
                "A velocidade de uma reação ácida.",
                "A velocidade de uma reação básica."
            },
            correctIndex = 1,
            questionNumber = 13,
            isImageAnswer = false,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/AcidBaseBufferDB/AcidBaseDB_ImageQuestionContainer13",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_013",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A constante de acidez (Ka) é específica para medir o grau de ionização (força) de um ácido fraco." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "A força da base.",
                "A força do ácido.",
                "A velocidade de uma reação ácida.",
                "A velocidade de uma reação básica."
            },
            correctIndex = 0,
            questionNumber = 14,
            isImageAnswer = false,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/AcidBaseBufferDB/AcidBaseDB_ImageQuestionContainer14",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_014",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A constante de basicidade (Kb) mede o grau de dissociação de uma base." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Um ácido fraco tem um valor de Ka:",
            answers = new string[] {
                "Alto",
                "Baixo",
                "Próximo a 1",
                "Próximo a 0"
            },
            correctIndex = 1,
            questionNumber = 15,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_015",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Um Ka baixo indica que poucas moléculas do ácido se dissociaram, o que é típico de ácidos fracos." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "O pKa de um ácido é definido como:",
            answers = new string[] {
                "log Ka",
                "-log Ka",
                "1/Ka",
                "10/Ka"
            },
            correctIndex = 1,
            questionNumber = 16,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_016",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Por definição matemática, o pKa é o logaritmo negativo da constante de acidez (pKa = -log Ka)." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Um ácido com um pKa baixo é:",
            answers = new string[] {
                "Fraco",
                "Forte",
                "De força moderada",
                "Indeterminado"
            },
            correctIndex = 1,
            questionNumber = 17,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_017",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Devido ao sinal negativo do logaritmo, quanto menor o pKa, maior o Ka e, portanto, mais forte é o ácido." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Um ácido com um pKa alto é:",
            answers = new string[] {
                "Forte",
                "Fraco",
                "De força moderada",
                "Indeterminado"
            },
            correctIndex = 1,
            questionNumber = 18,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_018",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Um pKa elevado significa um Ka baixo, indicando um ácido fraco." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "A equação de Henderson-Hasselbalch relaciona:",
            answers = new string[] {
                "pH, pKa e a razão entre base conjugada e ácido.",
                "pH, pKa e a concentração de íons H+",
                "pH, pOH e a concentração de íons OH-",
                "pKa, pKb e a concentração de íons H+"
            },
            correctIndex = 0,
            questionNumber = 19,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_019",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A equação de Henderson-Hasselbalch é: pH = pKa + log([A-]/[HA]). Ela relaciona o pH com o pKa e as concentrações." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Em uma solução-tampão, o pH permanece relativamente constante porque:",
            answers = new string[] {
                "O ácido se dissocia completamente.",
                "A base se dissocia completamente.",
                "Há um equilíbrio entre ácido e sua base conjugada.",
                "Não há interações entre o ácido e a base."
            },
            correctIndex = 2,
            questionNumber = 20,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_020",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Tampões funcionam porque o ácido fraco e sua base conjugada coexistem e reagem com H+ ou OH- adicionados, neutralizando-os." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "A faixa de tamponamento de uma solução-tampão é:",
            answers = new string[] {
                "Muito menor que o pKa.",
                "Igual ao pKa.",
                "Aproximadamente ± 1 unidade de pH em relação ao pKa.",
                "Muito maior que o pKa."
            },
            correctIndex = 2,
            questionNumber = 21,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_021",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O poder tamponante máximo ocorre perto do pKa. Geralmente a faixa eficaz é pKa ± 1 unidade de pH." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "O pH do sangue é mantido constante principalmente pelo sistema tampão:",
            answers = new string[] {
                "Fosfato",
                "Acetato",
                "Bicarbonato",
                "Tris"
            },
            correctIndex = 2,
            questionNumber = 22,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_022",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "No plasma sanguíneo, o principal sistema tampão é o do bicarbonato (H2CO3 / HCO3-)." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "O que acontece com o pH do sangue durante o exercício intenso?",
            answers = new string[] {
                "Aumenta.",
                "Diminui.",
                "Permanece constante.",
                "Varia de forma imprevisível."
            },
            correctIndex = 1,
            questionNumber = 23,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_023",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Durante o exercício, a produção de ácido lático e CO2 aumenta a concentração de H+, diminuindo o pH (acidose)." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Como o corpo responde à diminuição do pH sangüíneo durante o exercício?",
            answers = new string[] {
                "Diminui a taxa respiratória.",
                "Aumenta a taxa respiratória.",
                "Mantém a taxa respiratória constante.",
                "Para de respirar."
            },
            correctIndex = 1,
            questionNumber = 24,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_024",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O corpo aumenta a respiração para expulsar mais CO2, o que desloca o equilíbrio e reduz a concentração de H+, elevando o pH." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "O que é pH?",
            answers = new string[] {
                "Uma medida da concentração de OH-",
                "Uma medida da concentração de H+",
                "Uma medida da temperatura",
                "Uma medida da pressão"
            },
            correctIndex = 1,
            questionNumber = 25,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_025",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "pH significa 'potencial hidrogeniônico' e é uma medida direta da concentração de íons H+ (ou H3O+)." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Uma solução com pH 3 é:",
            answers = new string[] {
                "Neutra",
                "Básica",
                "Ácida",
                "Tampão"
            },
            correctIndex = 2,
            questionNumber = 26,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_026",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Na escala de pH em água a 25°C, valores menores que 7 indicam soluções ácidas." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Uma solução com pH 11 é:",
            answers = new string[] {
                "Ácida",
                "Neutra",
                "Básica",
                "Tampão"
            },
            correctIndex = 2,
            questionNumber = 27,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_027",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Valores maiores que 7 na escala de pH indicam soluções básicas ou alcalinas." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Uma solução com pH 7 é:",
            answers = new string[] {
                "Ácida",
                "Neutra",
                "Básica",
                "Tampão"
            },
            correctIndex = 1,
            questionNumber = 28,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_028",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "pH igual a 7 indica neutralidade, onde a concentração de H+ é igual à de OH-." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "O pOH de uma solução é uma medida de:",
            answers = new string[] {
                "Concentração de H+",
                "Concentração de OH-",
                "Acidez",
                "Basicidade"
            },
            correctIndex = 1,
            questionNumber = 29,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_029",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Assim como o pH mede H+, o pOH é a medida do potencial de íons hidroxila (OH-)." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "A relação entre pH e pOH é:",
            answers = new string[] {
                "pH + pOH = 0",
                "pH + pOH = 7",
                "pH + pOH = 14",
                "pH + pOH = 21"
            },
            correctIndex = 2,
            questionNumber = 30,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_030",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Em água a 25°C, a soma de pH e pOH é sempre igual a 14." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "<sup><size=100%>o</size></sup> C?",
            answers = new string[] {
                "10<sup><size=150%>-7</size></sup> ",
                "10<sup><size=150%>-14</size></sup> ",
                "10<sup><size=150%>0</size></sup> ",
                "10<sup><size=150%>14</size></sup> "
            },
            correctIndex = 1,
            questionNumber = 31,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_031",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O produto iônico da água (Kw) a 25°C é 10^-14." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "10<sup><size=150%>-14</size></sup> M",
                "10<sup><size=150%>-7</size></sup> M",
                "10<sup><size=150%>0</size></sup> M",
                "10<sup><size=150%>7</size></sup> M"
            },
            correctIndex = 1,
            questionNumber = 32,
            isImageAnswer = false,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/AcidBaseBufferDB/AcidBaseBufferDB_ImageQuestionContainer32",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_032",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Em solução neutra, [H+] = [OH-]. Como Kw = 10^-14, a concentração de H+ é a raiz quadrada disso (10^-7 M)." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "10<sup><size=150%>-14</size></sup> M",
                "10<sup><size=150%>-7</size></sup> M",
                "10<sup><size=150%>0</size></sup> M",
                "10<sup><size=150%>7</size></sup> M"
            },
            correctIndex = 1,
            questionNumber = 33,
            isImageAnswer = false,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/AcidBaseBufferDB/AcidBaseBufferDB_ImageQuestionContainer33",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_033",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Em solução neutra, a concentração de OH- também é 10^-7 M." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Qual a fórmula para calcular o pH?",
            answers = new string[] {
                "pH = log[H<sup><size=150%>+</size></sup>]",
                "pH = -log[H<sup><size=150%>+</size></sup>]",
                "pH = log[OH<sup><size=150%>-</size></sup>]",
                "pH = -log[OH<sup><size=150%>-</size></sup>]"
            },
            correctIndex = 1,
            questionNumber = 34,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_034",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A fórmula matemática do pH é o logaritmo negativo da concentração de H+: pH = -log[H+]." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Qual a fórmula para calcular o pOH?",
            answers = new string[] {
                "pOH = -log[OH<sup><size=150%>-</size></sup>]",
                "pOH = log[OH<sup><size=150%>-</size></sup>]",
                "pOH = -log[OH<sup><size=150%>+</size></sup>]",
                "pOH = log[OH<sup><size=150%>+</size></sup>]"
            },
            correctIndex = 0,
            questionNumber = 35,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_035",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A fórmula matemática do pOH é o logaritmo negativo da concentração de OH-: pOH = -log[OH-]." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Qual o valor mínimo de pH possível?",
            answers = new string[] {
                "0",
                "7",
                "14",
                "-14"
            },
            correctIndex = 0,
            questionNumber = 36,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_036",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Em soluções aquosas padrão, a escala prática vai de 0 a 14, sendo 0 o extremo de acidez." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Qual o valor máximo de pH possível?",
            answers = new string[] {
                "0",
                "7",
                "14",
                "-14"
            },
            correctIndex = 2,
            questionNumber = 37,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_037",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Na escala de pH comum (0 a 14), 14 representa o extremo de basicidade." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Qual o pH de uma solução neutra?",
            answers = new string[] {
                "0",
                "7",
                "14",
                "Variavel"
            },
            correctIndex = 1,
            questionNumber = 38,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_038",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O pH neutro a 25°C é exatamente 7." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Uma solução com pH abaixo de 7 é:",
            answers = new string[] {
                "Neutra",
                "Básica",
                "Ácida",
                "Tampão"
            },
            correctIndex = 2,
            questionNumber = 39,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_039",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Soluções com pH abaixo de 7 são ácidas." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Uma solução com pH acima de 7 é:",
            answers = new string[] {
                "Ácida",
                "Neutra",
                "Básica",
                "Tampão"
            },
            correctIndex = 2,
            questionNumber = 40,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_040",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Soluções com pH acima de 7 são básicas." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "O processo de neutralização envolve:",
            answers = new string[] {
                "A adição de um ácido a uma base.",
                "A adição de uma base a um ácido.",
                "A reação entre um ácido e uma base, resultando em água e um sal.",
                "Todas as alternativas anteriores."
            },
            correctIndex = 2,
            questionNumber = 41,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 3,
            questionInDevelopment = false,
            globalId = "acidsBase_041",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A neutralização é a reação entre um ácido e uma base, que produzem sal e água." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Durante uma titulação, o ponto de equivalência é atingido quando:",
            answers = new string[] {
                "A concentração de H+ é igual à concentração de OH<sup><size=150%>-</size></sup>. ",
                "O pH é igual a 0.",
                "O pH é igual a 7.",
                "O pH é igual a 14."
            },
            correctIndex = 0,
            questionNumber = 42,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 3,
            questionInDevelopment = false,
            globalId = "acidsBase_042",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Na titulação, o ponto de equivalência ocorre quando a quantidade de ácido e base se equivalem totalmente." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Um indicador de pH é uma substância que:",
            answers = new string[] {
                "Muda de cor em um determinado intervalo de pH.",
                "Muda de cor em qualquer pH.",
                "Mantém o pH constante.",
                "Neutraliza ácidos e bases."
            },
            correctIndex = 0,
            questionNumber = 43,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 3,
            questionInDevelopment = false,
            globalId = "acidsBase_043",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Indicadores são ácidos ou bases orgânicas fracas que possuem cores diferentes dependendo do pH (se estão protonados ou não)." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "O que é uma solução-tampão?",
            answers = new string[] {
                "Uma solução que resiste a mudanças de temperatura.",
                "Uma solução que resiste a mudanças de pressão.",
                "Uma solução que resiste a mudanças de pH.",
                "Uma solução que resiste a mudanças de volume."
            },
            correctIndex = 2,
            questionNumber = 44,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_044",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A solução-tampão resiste a adições de pequenas quantidades de ácidos e bases, mantendo o pH estável." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Uma solução-tampão é tipicamente composta de:",
            answers = new string[] {
                "Um ácido forte e uma base forte.",
                "Um ácido fraco e sua base conjugada.",
                "Um ácido forte e sua base conjugada.",
                "Um ácido fraco e uma base forte."
            },
            correctIndex = 1,
            questionNumber = 45,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_045",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Tampões são normalmente feitos de um ácido fraco e seu sal (base conjugada) ou uma base fraca e seu ácido conjugado." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "A capacidade de tamponamento de uma solução-tampão é máxima em:",
            answers = new string[] {
                "pH = 0",
                "pH = 7",
                "pH = pKa",
                "pH = 14"
            },
            correctIndex = 2,
            questionNumber = 46,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 3,
            questionInDevelopment = false,
            globalId = "acidsBase_046",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A capacidade máxima ocorre quando [HA] = [A-], o que, pela equação de Henderson-Hasselbalch, resulta em pH = pKa." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "A faixa de tamponamento de uma solução-tampão é aproximadamente:",
            answers = new string[] {
                "Igual ao pKa",
                "± 1 unidade de pH em relação ao pKa",
                "± 2 unidades de pH em relação ao pKa",
                "± 3 unidades de pH em relação ao pKa"
            },
            correctIndex = 1,
            questionNumber = 47,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 3,
            questionInDevelopment = false,
            globalId = "acidsBase_047",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Além do bicarbonato, o sistema tampão fosfato é o principal tampão intracelular." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Qual a principal função do sistema tampão do sangue?",
            answers = new string[] {
                "Regular a temperatura corporal",
                "Manter o pH do sangue constante",
                "Regular a pressão sanguínea",
                "Transportar oxigênio"
            },
            correctIndex = 1,
            questionNumber = 48,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "acidsBase_048",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A hiperventilação excessiva elimina muito CO2, o que reduz H+ no sangue, causando aumento do pH (alcalose respiratória)." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "O principal sistema tampão do sangue é o sistema:",
            answers = new string[] {
                "Fosfato",
                "Acetato",
                "Bicarbonato",
                "Hemoglobina"
            },
            correctIndex = 2,
            questionNumber = 49,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 3,
            questionInDevelopment = false,
            globalId = "acidsBase_049",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Se o corpo acumula CO2, ele reage formando ácido carbônico, aumentando H+ e reduzindo o pH (acidose respiratória)." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Durante o exercício intenso, o aumento da produção de ácido lático causa:",
            answers = new string[] {
                "Aumento do pH do sangue",
                "Diminuição do pH do sangue",
                "Aumento da taxa respiratória",
                "Diminuição da taxa respiratória"
            },
            correctIndex = 1,
            questionNumber = 50,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 3,
            questionInDevelopment = false,
            globalId = "acidsBase_050",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A acidose metabólica é causada pelo acúmulo de ácidos não voláteis (ex: ácido lático) ou perda de bicarbonato." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Segundo Arrhenius, um ácido é toda substância que em solução aquosa libera:",
            answers = new string[] {
                "OH⁻",
                "H⁺ (prótons)",
                "Na⁺",
                "Cl⁻"
            },
            correctIndex = 1,
            questionNumber = 51,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_051",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A alcalose metabólica ocorre por acúmulo excessivo de bicarbonato ou perda de ácidos do corpo." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Segundo Arrhenius, uma base é toda substância que em solução aquosa libera:",
            answers = new string[] {
                "H⁺",
                "OH⁻",
                "CO₂",
                "O₂"
            },
            correctIndex = 1,
            questionNumber = 52,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_052",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Em geral, pKa e Ka são medidos em condições controladas, normalmente a 25°C." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "A escala de pH mede:",
            answers = new string[] {
                "A concentração de oxigênio em uma solução",
                "A concentração de prótons (H⁺) em uma solução",
                "A quantidade de sais dissolvidos",
                "A densidade da água"
            },
            correctIndex = 1,
            questionNumber = 53,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_053",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "aplicar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Use pH = -log(10^-4) para encontrar a resposta." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Uma solução com pH menor que 7 é considerada:",
            answers = new string[] {
                "Neutra",
                "Ácida",
                "Básica",
                "Isotônica"
            },
            correctIndex = 1,
            questionNumber = 54,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_054",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "aplicar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A concentração [H+] é 10^(-pH). Portanto, 10^-9 M." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Uma solução com pH maior que 7 é considerada:",
            answers = new string[] {
                "Ácida",
                "Neutra",
                "Básica",
                "Saturada"
            },
            correctIndex = 2,
            questionNumber = 55,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_055",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "aplicar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Se o pH = 5, [H+] = 10^-5 M. Se passou para pH = 6, [H+] = 10^-6 M. A concentração diminuiu 10 vezes." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "O pH de uma solução neutra (como água pura, a 25 °C) é:",
            answers = new string[] {
                "0",
                "7",
                "10",
                "14"
            },
            correctIndex = 1,
            questionNumber = 56,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_056",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "aplicar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Lembre-se: [OH-] = 10^(-pOH) e pOH = 14 - pH." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Os sistemas tampão (buffers) no organismo têm como principal função:",
            answers = new string[] {
                "Regular a temperatura corporal",
                "Transportar oxigênio",
                "Manter o pH estável",
                "Produzir energia imediata"
            },
            correctIndex = 2,
            questionNumber = 57,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_057",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "aplicar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Um ácido forte dissocia-se 100%. Então [H+] = 0,1 M. pH = -log(0,1) = 1." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Qual par funciona como sistema tampão importante no sangue?",
            answers = new string[] {
                "Glicose/Insulina",
                "Hemoglobina/O₂",
                "H₂CO₃/HCO₃⁻ (ácido carbônico/bicarbonato)",
                "DNA/RNA"
            },
            correctIndex = 2,
            questionNumber = 58,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_058",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "aplicar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Uma base forte como NaOH libera 0,01 M de OH-. pOH = -log(0,01) = 2. Logo, pH = 14 - 2 = 12." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Uma solução com alta concentração de íons OH⁻ é classificada como:",
            answers = new string[] {
                "Ácida",
                "Neutra",
                "Básica",
                "Isotônica"
            },
            correctIndex = 2,
            questionNumber = 59,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_059",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A dissociação da água é um processo endotérmico. Em temperaturas mais altas, o Kw aumenta, logo o pH neutro diminui ligeiramente." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Qual indicador muda de cor para identificar se uma solução é ácida ou básica?",
            answers = new string[] {
                "Cloreto de sódio",
                "Fenolftaleína ou papel de tornassol",
                "Glicose",
                "Albumina"
            },
            correctIndex = 1,
            questionNumber = 60,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_060",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A capacidade tamponante depende das concentrações absolutas de [HA] e [A-]. Concentrações maiores significam mais moléculas para neutralizar o H+/OH- adicionado." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Qual das substâncias abaixo é considerada uma base de Arrhenius?",
            answers = new string[] {
                "HCl",
                "NaOH",
                "CO₂",
                "H₂SO₄"
            },
            correctIndex = 1,
            questionNumber = 61,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_061",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O CO2 difunde-se para o sangue e a enzima anidrase carbônica facilita sua hidratação formando ácido carbônico." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "Par conjugado",
                "Par isotópico",
                "Par redox",
                "Par covalente"
            },
            correctIndex = 0,
            questionNumber = 62,
            isImageAnswer = false,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/AcidBaseBufferDB/AcidBaseBufferDB_ImageQuestionContainer62",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_062",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Nos rins, ocorre reabsorção ou excreção de bicarbonato e H+ para regular lentamente o pH a longo prazo." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Qual o pH de uma solução neutra a 25 °C?",
            answers = new string[] {
                "0",
                "7",
                "14",
                "10"
            },
            correctIndex = 1,
            questionNumber = 63,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_063",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Os pulmões fazem a regulação de curto prazo controlando a excreção do CO2." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "5",
                "7",
                "9",
                "11"
            },
            correctIndex = 2,
            questionNumber = 64,
            isImageAnswer = false,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/AcidBaseBufferDB/AcidBaseBufferDB_ImageQuestionContainer64",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_064",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A urina normalmente é ácida (pH 6), pois os rins precisam excretar o excesso de H+ do metabolismo diário." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "O ácido clorídrico (HCl) é classificado como:",
            answers = new string[] {
                "Ácido fraco",
                "Base fraca",
                "Ácido forte",
                "Base forte"
            },
            correctIndex = 2,
            questionNumber = 65,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_065",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Ácidos polipróticos como o ácido fosfórico (H3PO4) podem doar mais de um próton e possuem múltiplos valores de pKa." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "Qual destas soluções apresenta caráter básico?",
            answers = new string[] {
                "pH = 2",
                "pH = 6",
                "pH = 7",
                "pH = 12"
            },
            correctIndex = 3,
            questionNumber = 66,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_066",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O pKa sucessivo de um ácido poliprótico é sempre maior, ou seja, fica cada vez mais difícil retirar o próximo H+." }
        },
        new Question
        {
            questionDatabankName = "AcidBaseBufferQuestionDatabase",
            questionText = "O produto iônico da água a 25 °C (Kw) é:",
            answers = new string[] {
                "1 × 10⁻¹⁴",
                "1 × 10⁻⁷",
                "1 × 10⁻¹",
                "1 × 10⁻¹⁰"
            },
            correctIndex = 0,
            questionNumber = 67,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            globalId = "acidsBase_067",
            topic = "acidsBase",
            subtopic = null,
            displayName = "Ácidos, Bases e Tampões",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O Kw é a constante de equilíbrio para a autoionização da água e vale 1 x 10^-14 a 25 °C." }
        }
    };

    public List<Question> GetQuestions() => questions;
    public QuestionSet GetQuestionSetType() => QuestionSet.acidsBase;
    public string GetDatabankName()  => "AcidBaseBufferQuestionDatabase";
    public string GetDisplayName()   => "Ácidos, Bases e Tampões";
    public bool IsDatabaseInDevelopment() => databaseInDevelopment;
}