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
            questionHint = new QuestionHint { text = "Segundo Arrhenius, a definição foca na água: um ácido é aquela substância química que, quando dissolvida em solução aquosa, sofre ionização e libera íons H+ (ou hidroxônio, H3O+)." }
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
            questionHint = new QuestionHint { text = "Para Arrhenius, o caráter básico depende de quem doa hidroxilas. Uma base é qualquer composto que se dissocia em água liberando o ânion OH- (hidroxila)." }
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
            questionHint = new QuestionHint { text = "Brønsted e Lowry expandiram a teoria para fora da água. Para eles, um ácido é qualquer molécula ou íon capaz de doar um próton (H+) para outra espécie química durante a reação." }
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
            questionHint = new QuestionHint { text = "Na visão de Brønsted-Lowry, a base não precisa ter OH-. Ela é definida como qualquer substância química que atua como receptora de um próton (H+) doado por um ácido." }
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
            questionHint = new QuestionHint { text = "Substâncias anfóteras, ou anfotéricas, têm um comportamento duplo. A água, por exemplo, pode doar um H+ agindo como ácido, ou receber um H+ agindo como base, dependendo do pH do meio." }
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
            questionHint = new QuestionHint { text = "O conceito de pares conjugados é fundamental. Quando um ácido (como o HCl) perde seu próton (H+), a espécie resultante (Cl-) tem a capacidade de receber o próton de volta, atuando como base conjugada." }
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
            questionHint = new QuestionHint { text = "Observe o caminho inverso: quando uma base reage e captura um próton (H+), a nova estrutura formada fica protonada e ganha a capacidade de doar esse próton, tornando-se o ácido conjugado." }
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
            questionHint = new QuestionHint { text = "Um ácido forte (como o HCl ou H2SO4) é aquele que se desintegra completamente na água. Isso significa que quase 100% de suas moléculas sofrem ionização imediata, liberando muitos íons H+." }
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
            questionHint = new QuestionHint { text = "Diferente dos ácidos fortes, os ácidos fracos não se ionizam por completo. Eles estabelecem um equilíbrio dinâmico, onde a maior parte da molécula continua intacta (não dissociada) na solução." }
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
            questionHint = new QuestionHint { text = "A constante de equilíbrio (Keq) quantifica o estado final de uma reação reversível. Ela nos dá a exata proporção matemática entre as concentrações dos produtos e as dos reagentes quando a reação se estabiliza." }
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
            questionHint = new QuestionHint { text = "Se a constante Keq é menor que 1, significa que o numerador (produtos) é menor que o denominador (reagentes). Portanto, o equilíbrio tende fortemente à esquerda, não ocorrendo dissociação total." }
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
            questionHint = new QuestionHint { text = "Se Keq é bem maior que 1, a reação é muito favorável no sentido direto. Isso indica que quase todos os reagentes foram convertidos e o sistema se estabiliza com alta concentração de produtos." }
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
            questionHint = new QuestionHint { text = "A constante de acidez (Ka) é a medida oficial da força de um ácido. Trata-se da constante de equilíbrio aplicada especificamente para a reação de dissociação do ácido em água." }
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
            questionHint = new QuestionHint { text = "Assim como o Ka mede ácidos, o Kb (constante de basicidade) mede bases fracas. Ele reflete a constante de equilíbrio na reação em que a base captura um próton da água gerando OH-." }
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
            questionHint = new QuestionHint { text = "Um Ka muito baixo indica que o numerador (produtos iônicos dissociados) é minúsculo. Isso comprova experimentalmente que poucas moléculas se ionizaram, característica de um ácido fraco." }
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
            questionHint = new QuestionHint { text = "Por uma questão matemática para evitar usar potências de 10 negativas, definimos o pKa como sendo o logaritmo negativo da constante de dissociação ácida: pKa = -log Ka." }
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
            questionHint = new QuestionHint { text = "O 'p' indica o uso de logaritmo negativo. Por conta dessa inversão matemática, quanto mais alto for o valor do Ka (ácido mais forte), menor e mais negativo será o valor numérico do pKa." }
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
            questionHint = new QuestionHint { text = "Um valor alto de pKa significa que o Ka daquele ácido é muito pequeno (ex: 10^-8). Logo, um pKa alto indica que o ácido não gosta de doar seu próton, sendo classificado como fraco." }
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
            questionHint = new QuestionHint { text = "A famosa equação de Henderson-Hasselbalch é usada justamente para cálculos de sistemas tampão. Ela correlaciona o pH do ambiente com o pKa do ácido e a proporção entre a forma dissociada e não dissociada." }
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
            questionHint = new QuestionHint { text = "As soluções-tampão resistem a variações de pH. Elas funcionam por conter tanto um ácido fraco (que neutraliza OH- intruso) quanto sua base conjugada (que neutraliza H+ intruso) em altas concentrações." }
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
            questionHint = new QuestionHint { text = "Um tampão funciona melhor quando a concentração de ácido é igual à de base, momento em que o pH é igual ao pKa. Na prática, a faixa de proteção tamponante é de +1 ou -1 unidade em volta do pKa." }
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
            questionHint = new QuestionHint { text = "O plasma humano precisa de uma resposta rápida. Esse papel recai sobre o tampão bicarbonato (H2CO3 / HCO3-), pois o ácido carbônico pode ser rapidamente convertido e expelido como gás carbônico pelos pulmões." }
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
            questionHint = new QuestionHint { text = "Em atividades anaeróbicas intensas, as células musculares lançam ácido lático na circulação. A dissociação desse ácido gera uma carga massiva de H+, o que derruba o pH do sangue (acidose)." }
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
            questionHint = new QuestionHint { text = "O CO2 é o equivalente gasoso do ácido. Quando você respira mais rápido (hiperventilação), o corpo 'sopra' o CO2 embora. Isso desloca o equilíbrio, consome os íons H+ sobrando e faz o pH subir de volta." }
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
            questionHint = new QuestionHint { text = "O termo pH significa 'potencial Hidrogeniônico'. É a régua universal da química que mede a concentração exata de íons de hidrogênio livres em uma solução aquosa." }
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
            questionHint = new QuestionHint { text = "Lembrando da escala padrão em água a 25 °C (de 0 a 14), o ponto central neutro é 7. Qualquer valor inferior a isso representa uma abundância de íons H+, caracterizando um ambiente ácido." }
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
            questionHint = new QuestionHint { text = "Na mesma escala de 0 a 14, se o pH for superior a 7, os íons hidroxila (OH-) estão em maior número que os de hidrogênio. Isso define quimicamente uma solução básica ou alcalina." }
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
            questionHint = new QuestionHint { text = "O pH é 7 (neutro) quando o sistema atinge o equilíbrio perfeito, em que as concentrações molares de hidrogênio (H+) e hidroxila (OH-) estão em quantidades rigorosamente iguais na água." }
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
            questionHint = new QuestionHint { text = "A mesma lógica logarítmica se aplica ao OH-. O pOH (potencial hidroxiliônico) expressa a acidez do lado das bases, sendo uma medida direta da concentração de íons OH-." }
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
            questionHint = new QuestionHint { text = "Na autoionização da água sob temperatura de 25 °C, existe uma relação matemática fechada de Kw: a soma numérica entre o pH e o pOH de qualquer solução será sempre e obrigatoriamente 14." }
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
            questionHint = new QuestionHint { text = "A constante Kw representa o produto iônico da água. Na temperatura de 25 °C, a multiplicação das concentrações [H+] x [OH-] gera sempre o valor constante de 10 elevado a -14." }
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
            questionHint = new QuestionHint { text = "Se a solução for pura e neutra, [H+] e [OH-] devem ser idênticos. Como o produto deles dá 10^-14, a única possibilidade matemática é que cada um seja igual a 10^-7 molar." }
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
            questionHint = new QuestionHint { text = "O mesmo raciocínio do H+ se aplica ao ânion hidroxila: na neutralidade pura a 25 °C, não há predomínio, e a concentração de OH- será de 10^-7 mol por litro." }
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
            questionHint = new QuestionHint { text = "A definição estrita e oficial do pH introduzida por Sørensen é a base do logaritmo decimal: pH = -log10[H+], onde os colchetes indicam a concentração molar do hidrogênio." }
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
            questionHint = new QuestionHint { text = "Igualmente à definição de Sørensen para o ácido, o pOH é calculado aplicando-se o logaritmo negativo na base 10 sobre a concentração dos íons hidroxila: pOH = -log10[OH-]." }
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
            questionHint = new QuestionHint { text = "Se usarmos concentrações molares de referência (como 1 mol/L para ácidos fortes), a escala vai bater no limite teórico inferior de 0 para a acidez máxima rotineira." }
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
            questionHint = new QuestionHint { text = "Na outra ponta dessa escala de referência usual baseada em 1 mol/L (ex: hidróxido de sódio concentrado), o pH máximo da basicidade rotineira atingirá o topo marcando 14." }
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
            questionHint = new QuestionHint { text = "O número 7 é o marco exato no meio da escala logarítmica e atesta a neutralidade para soluções aquosas comuns submetidas a 25 °C de temperatura ambiente." }
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
            questionHint = new QuestionHint { text = "Para facilitar o dia a dia laboratorial: viu um valor medido no peagômetro que cravou abaixo de 7? Não tenha dúvida de que há excesso de prótons e a solução é ácida." }
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
            questionHint = new QuestionHint { text = "Da mesma forma, qualquer leitura eletrônica que entregue um número superior a 7 atesta um excesso de hidroxilas (OH-), revelando que o meio está sob domínio básico." }
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
            questionHint = new QuestionHint { text = "As reações de neutralização são aquelas que anulam a agressividade das partes envolvidas: os H+ do ácido encontram os OH- da base, gerando moléculas de água e um sal dissolvido." }
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
            questionHint = new QuestionHint { text = "Em uma titulação, a técnica exige gotejar cuidadosamente um titulante. Quando chegamos ao exato ponto de equivalência, significa que todos os mols da base anularam exatamente os mols do ácido original." }
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
            questionHint = new QuestionHint { text = "Os indicadores (como fenolftaleína) não são enfeites. Eles são ácidos ou bases orgânicas gigantes e fracas, cujas estruturas moleculares dobram e mudam de cor radicalmente ao ganharem ou perderem um H+." }
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
            questionHint = new QuestionHint { text = "Um sistema tampão atua como uma 'esponja' química. Se você adicionar ácido, ele absorve. Se adicionar base, ele doa H+. Isso resulta em uma forte resistência contra mudanças bruscas de pH." }
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
            questionHint = new QuestionHint { text = "Você não pode montar um tampão misturando ácido forte, pois ele reage sem retorno. A química do tampão exige moléculas reversíveis, ou seja, um ácido ou base fraca combinado com seu sal conjugado." }
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
            questionHint = new QuestionHint { text = "Analise a equação: pH = pKa + log([A-]/[HA]). Se as concentrações do ácido e da base forem iguais, o termo log se torna log(1), que vale zero. O resultado é o pico da eficiência: pH igual ao pKa." }
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
            questionHint = new QuestionHint { text = "Enquanto o bicarbonato toma conta do sangue extracelular, o interior de nossas células usa o sistema tampão fosfato (HPO4 / H2PO4). Seu pKa próximo a 6.8 o torna ideal para a proteção do citosol." }
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
            questionHint = new QuestionHint { text = "A ventilação excessiva dos pulmões funciona como uma bomba sugadora de CO2. Removendo esse componente, o ácido carbônico se converte de volta, o H+ sanguíneo desaparece, e o pH sobe na alcalose." }
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
            questionHint = new QuestionHint { text = "Se ocorre uma hipoventilação pulmonar (asma grave, por exemplo), o CO2 se acumula e força a água a criar ácido carbônico no sangue. O banho de novos íons H+ abaixa o pH gerando acidose." }
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
            questionHint = new QuestionHint { text = "Acidose metabólica é sistêmica e não tem culpa primária no pulmão. Ela ocorre quando perdemos as reservas de bicarbonato nos rins, ou quando produzimos muito lixo ácido no metabolismo." }
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
            questionHint = new QuestionHint { text = "Já a alcalose metabólica pode ocorrer após crises de vômito severas, onde o estômago ejeta todo o seu conteúdo ácido, deixando o resto do organismo subitamente rico em excessos de bases." }
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
            questionHint = new QuestionHint { text = "Termodinâmica importa: as constantes de dissociação (pKa, Ka, Kw) foram padronizadas na literatura. Qualquer variação de temperatura muda esses números. A maioria das tabelas crava o padrão em 25 °C." }
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
            questionHint = new QuestionHint { text = "Aqui é pura matemática. Se a concentração [H+] for igual a 10^(-4) mol/L, usando a fórmula pH = -log[H+], o próprio expoente nos dará o número 4 diretamente como pH." }
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
            questionHint = new QuestionHint { text = "Fórmula reversa: como pH é o logaritmo negativo da concentração mola, para um pH batendo a marca de 9, a verdadeira concentração em mols de hidrogênio livres será 10 elevado a -9 M." }
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
            questionHint = new QuestionHint { text = "A escala de pH não é linear, ela é logarítmica (potências de 10). Portanto, pular uma unidade (ex: de 5 para 6) significa que os íons reduziram exatamente em uma escala de 10 vezes." }
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
            questionHint = new QuestionHint { text = "Lembre-se da regra de ouro do equilíbrio: pH + pOH = 14. Se você obteve pOH = 8, então a conta 14 - 8 te mostrará imediatamente que o pH real daquela mistura será 6." }
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
            questionHint = new QuestionHint { text = "Um ácido forte derrama 100% dos seus mols como íons de H+. Se a solução era de 0,1 Molar, o sistema recebe 10^-1 de hidrogênio livres. O cálculo do log negativo te dá pH = 1." }
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
            questionHint = new QuestionHint { text = "NaOH joga bases fortíssimas. 0,01 Molar gera 10^-2 mols de OH-. Isso reflete um pOH igual a 2. Para fechar o ciclo de 14, subtraímos esse valor e confirmamos um pH altamente básico de 12." }
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
            questionHint = new QuestionHint { text = "O comportamento neutro se altera! Em temperaturas muito mais quentes, as moléculas da água tremem e sofrem maior autoionização (aumenta o Kw), fazendo o ponto da neutralidade cair para algo como 6.1." }
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
            questionHint = new QuestionHint { text = "Ter proporções iguais (pH = pKa) gera eficácia, mas não diz o limite. A verdadeira capacidade de resistir (quanto ácido posso jogar?) dependerá das concentrações absolutas em mols dos componentes." }
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
            questionHint = new QuestionHint { text = "Para fazer do bicarbonato um tampão genial, o sangue possui os glóbulos vermelhos lotados com uma das enzimas mais rápidas que a natureza inventou: a anidrase carbônica." }
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
            questionHint = new QuestionHint { text = "Os rins são a resposta lenta e teimosa do corpo. Eles compensam anomalias metabólicas e respiratórias longas secretando H+ diretamente na urina e conservando valioso bicarbonato de volta ao sangue." }
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
            questionHint = new QuestionHint { text = "Os pulmões, ao invés de excretarem íons líquidos, resolvem a acidez soprando-a para o ambiente na forma de gás carbônico. É o ajuste de curto prazo mais potente que temos." }
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
            questionHint = new QuestionHint { text = "Enquanto o corpo luta para estabilizar o plasma alcalino em ~7.4, toda a porcaria ácida não volátil tem que ir para algum lugar. Por isso, as reações renais forçam a urina a ser tipicamente mais ácida (pH ~6)." }
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
            questionHint = new QuestionHint { text = "Um ácido poliprótico possui múltiplos hidrogênios desprendíveis em sua fórmula (como o fosfórico H3PO4). Eles se ionizam em estágios e cada molécula ejetada de H+ apresenta um pKa exclusivo." }
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
            questionHint = new QuestionHint { text = "Quanto mais você tira prótons de um ácido poliprótico, mais a carga negativa da molécula segura os prótons remanescentes. Ou seja, ionizações seguintes requerem muito mais força, logo seu pKa aumenta muito." }
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
            questionHint = new QuestionHint { text = "O Produto Iônico, apelidado de Kw (onde o w significa water), é a constante universal da autoionização da água sob 25 °C. Ele dita as regras e sempre cravou a marca estática de 1 x 10^-14." }
        }
    };

    public List<Question> GetQuestions() => questions;
    public QuestionSet GetQuestionSetType() => QuestionSet.acidsBase;
    public string GetDatabankName()  => "AcidBaseBufferQuestionDatabase";
    public string GetDisplayName()   => "Ácidos, Bases e Tampões";
    public bool IsDatabaseInDevelopment() => databaseInDevelopment;
}