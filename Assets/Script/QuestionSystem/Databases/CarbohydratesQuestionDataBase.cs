using UnityEngine;
using System.Collections.Generic;
using QuestionSystem;

public class CarbohydratesQuestionDatabase : MonoBehaviour, IQuestionDatabase
{

    [Header("Development Settings")]
    [SerializeField] private bool databaseInDevelopment = false;
    
    private List<Question> questions = new List<Question>
    {
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual a fórmula geral dos monossacarídeos?",
            answers = new string[] {
                "(CH<sub>2</sub> O)<sub>n</sub>",
                "C<sub>n</sub> H<sub>2n</sub> O<sub>n</sub>",
                "C<sub>n</sub> H<sub>2n-2</sub> O<sub>n</sub>",
                "C<sub>n</sub> H<sub>2n+2</sub> O<sub>n</sub>"
            },
            correctIndex = 0,
            questionNumber = 1,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Os monossacarídeos seguem geralmente a fórmula (CH2O)n, que mostra uma proporção típica entre carbono, hidrogênio e oxigênio.\nÉ como uma “receita básica” que se repete, variando só o tamanho da molécula.\nCuidado: algumas alternativas parecem parecidas, mas alteram essa proporção clássica.\nNem toda fórmula com C, H e O está correta — a proporção importa!",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "O que diferencia uma aldose de uma cetose?",
            answers = new string[] {"Grupo funcional aldeído vs. cetona", "Número de átomos de carbono", "Presença de oxigênio", "Solubilidade em água"},
            correctIndex = 0,
            questionNumber = 2,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "A diferença está no grupo funcional: aldoses têm aldeído, cetoses têm cetona.\nPense nisso como mudar a “posição da função” na molécula.\nIsso altera propriedades químicas importantes, mesmo com mesma fórmula.\nPegadinha: não tem relação com tamanho nem solubilidade.",
            bloomLevel = "compreender"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual das estruturas representa uma cetose?",
            answers = new string[] {
                "AnswerImages/CarbohydrateDB/D-aldotriose",
                "AnswerImages/CarbohydrateDB/D-aldotetrose",
                "AnswerImages/CarbohydrateDB/D-cetotetrose",
                "AnswerImages/CarbohydrateDB/D-aldopentose"
            },
            correctIndex = 2,
            questionNumber = 3,
            isImageAnswer = true,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Cetoses possuem grupo cetona, geralmente no carbono interno.\nNa nomenclatura, “ceto-” já indica isso.\nObserve a posição do grupo carbonila na estrutura.\nPegadinha: “aldo-” indica aldeído — não confundir!",
            bloomLevel = "aplicar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual estrutura representa uma aldotetrose",
            answers = new string[] {
                "AnswerImages/CarbohydrateDB/D-aldotriose",
                "AnswerImages/CarbohydrateDB/D-aldotetrose",
                "AnswerImages/CarbohydrateDB/D-cetotetrose",
                "AnswerImages/CarbohydrateDB/D-aldopentose"
            },
            correctIndex = 1,
            questionNumber = 4,
            isImageAnswer = true,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "“Aldo” indica presença de aldeído e “tetrose” indica 4 carbonos.\nEntão você procura uma cadeia com 4 C e grupo aldeído.\nÉ como ler o nome químico como uma descrição.\nPegadinha: triose (3C) e pentose (5C) estão ali pra confundir.",
            bloomLevel = "aplicar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "AnswerImages/CarbohydrateDB/L-glicose",
                "AnswerImages/CarbohydrateDB/D-glicose",
                "AnswerImages/CarbohydrateDB/D-galactose",
                "AnswerImages/CarbohydrateDB/D-frutose"
            },
            correctIndex = 0,
            questionNumber = 5,
            isImageAnswer = true,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/CarbohydrateDB/carbohydrateDB_ImageQuestionContainer5",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "A forma L ou D depende da orientação do carbono quiral mais distante do grupo carbonila.\nÉ como um “espelho” da molécula.\nCompare com o padrão da glicose conhecida.\nPegadinha: D ≠ direita na tela — é uma convenção estrutural.",
            bloomLevel = "analisar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "O que são epímeros?",
            answers = new string[] {
                "Diastereoisômeros que diferem em um único centro quiral",
                "Enanciômeros que diferem em todos os centros quirais",
                "Isômeros que diferem no número de átomos de carbono",
                "Isômeros que diferem no tipo de ligação"},
            correctIndex = 0,
            questionNumber = 6,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Epímeros diferem em apenas UM carbono quiral.\nÉ como duas moléculas quase iguais, mudando só um detalhe.\nIsso pode mudar propriedades importantes.\nPegadinha: não são espelhos completos (isso seria enantiômero).",
            bloomLevel = "compreender"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "AnswerImages/CarbohydrateDB/L-glicose",
                "AnswerImages/CarbohydrateDB/D-glicose",
                "AnswerImages/CarbohydrateDB/L-frutose",
                "AnswerImages/CarbohydrateDB/D-tagatose"
            },
            correctIndex = 3,
            questionNumber = 7,
            isImageAnswer = true,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/CarbohydrateDB/carbohydrateDB_ImageQuestionContainer7",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "A tagatose é uma cetose, então procure o grupo cetona.\nAlém disso, observe a configuração D.\nCompare com glicose/frutose para eliminar alternativas.\nPegadinha: L/D e tipo (aldo/ceto) aparecem misturados.",
            bloomLevel = "analisar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "O que são anômeros?",
            answers = new string[] {
                "Isômeros cíclicos que diferem na configuração do carbono anomérico",
                "Isômeros de cadeia aberta",
                "Isômeros que diferem na posição do grupo hidroxila",
                "Isômeros que diferem no tipo de ligação"
            },
            correctIndex = 0,
            questionNumber = 8,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Anômeros diferem apenas no carbono anomérico após ciclagem.\nIsso gera formas alfa e beta.\nÉ como virar só uma “pecinha” da molécula.\nPegadinha: não confundir com epímeros (outros carbonos quirais).",
            bloomLevel = "compreender"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "AnswerImages/CarbohydrateDB/L-glicose",
                "AnswerImages/CarbohydrateDB/alfa-galactopiranose",
                "AnswerImages/CarbohydrateDB/beta-galactosamina",
                "AnswerImages/CarbohydrateDB/alfa-glicopiranose"
            },
            correctIndex = 3,
            questionNumber = 9,
            isImageAnswer = true,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/CarbohydrateDB/carbohydrateDB_ImageQuestionContainer9",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Procure a forma cíclica da glicose (piranose).\nDepois veja se o OH do carbono anomérico está em posição alfa.\nAlfa geralmente aponta para baixo na projeção.\nPegadinha: galactose é parecida, mas muda em um carbono.",
            bloomLevel = "analisar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual o principal dissacarídeo encontrado na cana-de-açúcar?",
            answers = new string[] {"Maltose", "Lactose", "Sacarose", "Celobiose"},
            correctIndex = 2,
            questionNumber = 10,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "A sacarose é o açúcar comum da cana e do açúcar de mesa.\nEla é formada por glicose + frutose.\nÉ o que usamos no dia a dia no café.\nPegadinha: lactose é do leite e maltose vem do amido.",
            bloomLevel = "lembrar"
        }, 
        // 10
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual dissacarídeo é o açúcar do leite?",
            answers = new string[] {"Maltose", "Lactose", "Sacarose", "Celobiose"},
            correctIndex = 1,
            questionNumber = 11,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "A lactose é o açúcar característico do leite.\nEla é formada por glicose + galactose.\nPor isso, intolerância à lactose envolve dificuldade em quebrar essa molécula.\nPegadinha: sacarose é açúcar de mesa, não do leite.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual o tipo de ligação que une os monossacarídeos em um dissacarídeo?",
            answers = new string[] {"Ligação peptídica", "Ligação glicosídica", "Ligação éster", "Ligação fosfodiéster"},
            correctIndex = 1,
            questionNumber = 12,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Monossacarídeos se unem por ligações glicosídicas.\nÉ como um “elo” entre açúcares formando cadeias maiores.\nEssa ligação envolve o carbono anomérico.\nPegadinha: peptídica é de proteínas, fosfodiéster é de DNA/RNA.",
            bloomLevel = "compreender"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "monossacarídeo",
                "dissacarídeo",
                "oligossacarídeo",
                "polissacarídeo"
            },
            correctIndex = 1,
            questionNumber = 13,
            isImageAnswer = false,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/CarbohydrateDB/carbohydrateDB_ImageQuestionContainer13",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Dissacarídeos são formados por dois monossacarídeos ligados.\nVisualmente, você deve identificar duas unidades conectadas.\nÉ como juntar dois blocos de Lego.\nPegadinha: polissacarídeos têm muitas unidades, não só duas.",
            bloomLevel = "aplicar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "1-beta-4",
                "1-alfa-4",
                "1-beta-2",
                "1-alfa-2"
            },
            correctIndex = 0,
            questionNumber = 14,
            isImageAnswer = false,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/CarbohydrateDB/carbohydrateDB_ImageQuestionContainer14",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Observe os carbonos envolvidos na ligação e a orientação do OH.\n“1→4” indica quais carbonos estão ligados.\n“beta” depende da posição espacial do grupo.\nPegadinha: alfa vs beta muda totalmente a estrutura final!",
            bloomLevel = "analisar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual o principal polissacarídeo de reserva energética em plantas?",
            answers = new string[] {"Celulose", "Quitina", "Amido", "Glicogênio"},
            correctIndex = 2,
            questionNumber = 15,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Plantas armazenam energia principalmente na forma de amido.\nÉ como o “estoque de energia” vegetal.\nEncontrado em alimentos como batata e arroz.\nPegadinha: celulose é estrutural, não reserva.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual o principal polissacarídeo de reserva energética em animais?",
            answers = new string[] {"Celulose", "Quitina", "Amido", "Glicogênio"},
            correctIndex = 3,
            questionNumber = 16,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Animais armazenam glicose na forma de glicogênio.\nEle fica principalmente no fígado e músculos.\nÉ como uma “bateria de energia rápida”.\nPegadinha: amido é reserva vegetal, não animal.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual a principal diferença estrutural entre amilose e amilopectina?",
            answers = new string[] {"Ramificação", "Tipo de ligação glicosídica", "Tipo de monossacarídeo", "Peso molecular"},
            correctIndex = 0,
            questionNumber = 17,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "A amilose é linear, enquanto a amilopectina é ramificada.\nIsso afeta como a molécula ocupa espaço.\nImagine uma corda vs uma árvore com galhos.\nPegadinha: ambas têm ligações alfa, então não é esse o diferencial principal.",
            bloomLevel = "compreender"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual o tipo de ligação glicosídica predominante na celulose?",
            answers = new string[] {"alfa(1 -> 4)", "alfa(1 -> 6)", "beta(1 -> 4)", "beta(1 -> 6)"},
            correctIndex = 2,
            questionNumber = 18,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "A celulose possui ligações beta(1→4).\nIsso gera uma estrutura rígida e resistente.\nÉ como fibras bem alinhadas formando parede celular.\nPegadinha: alfa(1→4) é do amido, não da celulose.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual o tipo de ligação glicosídica predominante no amido?",
            answers = new string[] {"alfa(1 -> 4)", "alfa(1 -> 6)", "beta(1 -> 4)", "beta(1 -> 6)"},
            correctIndex = 0,
            questionNumber = 19,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "O amido possui principalmente ligações alfa(1→4).\nIsso permite formar estruturas mais flexíveis.\nFacilita a digestão por enzimas humanas.\nPegadinha: beta(1→4) é resistente (celulose).",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual enzima hidrolisa o amido?",
            answers = new string[] {"Celulase", "Quitina", "Amilase", "Lactase"},
            correctIndex = 2,
            questionNumber = 20,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "A amilase quebra o amido em açúcares menores.\nEla está presente na saliva e no pâncreas.\nPor isso a digestão de carboidratos começa na boca.\nPegadinha: lactase atua na lactose, não no amido.",
            bloomLevel = "lembrar"
        },
        // 20
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual enzima hidrolisa a celulose?",
            answers = new string[] {"Celulase", "Quitina", "Amilase", "Lactase"},
            correctIndex = 0,
            questionNumber = 21,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "A celulose é quebrada pela enzima celulase.\nHumanos não produzem essa enzima, por isso não digerimos celulose.\nAlguns microrganismos fazem isso (ex: em ruminantes).\nPegadinha: amilase não quebra celulose.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "O que são glicoproteínas?",
            answers = new string[] {
                "Proteínas ligadas a carboidratos",
                "Proteínas ligadas a lipídeos",
                "Proteínas ligadas a ácidos nucleicos",
                "Proteínas ligadas a outras proteínas"
            },
            correctIndex = 0,
            questionNumber = 22,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "Glicoproteínas são proteínas com cadeias de carboidratos ligadas.\nElas aparecem muito na membrana celular.\nFuncionam como reconhecimento e sinalização.\nPegadinha: não confundir com glicolipídeos (lipídios + carboidratos).",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "O que são glicosaminoglicanos?",
            answers = new string[] {
                "Homopolissacarídeos que presentes no citoplasma celular.",
                "Heteropolissacarídeos que compõe a matriz extracelular.",
                "São glicoses ligadas a aminas presentes no sangue",
                "São dissacarídeos ligados à proteínas"
            },
            correctIndex = 1,
            questionNumber = 23,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Glicosaminoglicanos são heteropolissacarídeos da matriz extracelular.\nEles ajudam a dar suporte e retenção de água aos tecidos.\nSão como uma “esponja estrutural”.\nPegadinha: não são simples dissacarídeos nem ficam no citoplasma.",
            bloomLevel = "compreender"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "O que são proteoglicanos?",
            answers = new string[] {
                "Glicosaminoglicanos ligados a proteínas",
                "Glicoproteínas ligadas a lipídeos",
                "Glicolipídeos ligados a proteínas",
                "Proteínas ligadas a ácidos nucleicos"
            },
            correctIndex = 0,
            questionNumber = 24,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Proteoglicanos são proteínas ligadas a glicosaminoglicanos.\nEles formam estruturas grandes na matriz extracelular.\nAjudam na hidratação e resistência dos tecidos.\nPegadinha: glicoproteínas têm menos carboidrato que proteoglicanos.",
            bloomLevel = "compreender"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Onde são encontradas os glicosaminoglicanos?",
            answers = new string[] {"Matriz extracelular", "Membrana celular", "Citoplasma", "Todas as alternativas acima"},
            correctIndex = 0,
            questionNumber = 25,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Eles são encontrados principalmente na matriz extracelular.\nEssa região dá suporte aos tecidos fora das células.\nFuncionam como preenchimento e estrutura.\nPegadinha: não ficam dentro da célula como principal localização.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Identifique abaixo um componente estrutural dos glicosaminoglicanos.",
            answers = new string[] {
                "AnswerImages/CarbohydrateDB/alfa-glicopiranose",
                "AnswerImages/CarbohydrateDB/beta-glicopiranose",
                "AnswerImages/CarbohydrateDB/beta-galactosamina",
                "AnswerImages/CarbohydrateDB/D-aldopentose"
            },
            correctIndex = 2,
            questionNumber = 26,
            isImageAnswer = true,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Glicosaminoglicanos contêm açúcares modificados com grupos amina.\nA galactosamina é um exemplo clássico.\nProcure por estruturas com nitrogênio ligado.\nPegadinha: glicose simples não tem esse grupo amina.",
            bloomLevel = "aplicar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Indique abaixo a estrutura do ácido hialurônico, que é um glicosaminoglicano cujo monômero é formado pelo ácido glicurônico e pelo ácido N-acetil-glicosamina.",
            answers = new string[] {
                "AnswerImages/CarbohydrateDB/dermatan",
                "AnswerImages/CarbohydrateDB/condroitin",
                "AnswerImages/CarbohydrateDB/queratan",
                "AnswerImages/CarbohydrateDB/hialuronico"
            },
            correctIndex = 3,
            questionNumber = 27,
            isImageAnswer = true,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "O ácido hialurônico é formado por unidades repetidas específicas.\nEle não possui sulfatação, diferente de outros GAGs.\nÉ muito abundante em tecidos conjuntivos.\nPegadinha: dermatan e condroitin são parecidos, mas diferentes na composição.",
            bloomLevel = "analisar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual a função do ácido hialurônico?",
            answers = new string[] {"Lubrificação", "Suporte estrutural", "Viscosidade", "Todas as alternativas acima"},
            correctIndex = 3,
            questionNumber = 28,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "O ácido hialurônico atua em lubrificação, suporte e viscosidade.\nEle retém água e forma um gel nos tecidos.\nMuito importante em articulações e pele.\nPegadinha: não tem só uma função — são várias ao mesmo tempo.",
            bloomLevel = "compreender"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "A heparina é um glicosaminoglicano com uma das maiores densidades de carga negativa dentre todas as biomoléculas dos organismos vivos. Indique abaixo o monômero que forma a heparina.",
            answers = new string[] {
                "AnswerImages/CarbohydrateDB/dermatan",
                "AnswerImages/CarbohydrateDB/condroitin",
                "AnswerImages/CarbohydrateDB/heparina",
                "AnswerImages/CarbohydrateDB/hialuronico"
            },
            correctIndex = 2,
            questionNumber = 29,
            isImageAnswer = true,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "A heparina é formada por unidades altamente sulfatadas.\nIsso explica sua carga negativa elevada.\nEssa característica é essencial para sua função biológica.\nPegadinha: não confundir com outros GAGs menos carregados.",
            bloomLevel = "analisar"
        },
        new Question {
        questionDatabankName = "CarbohydratesQuestionDatabase",
        questionText = "Qual é uma das principais função da heparina?",
        answers = new string[] {"Anticoagulante", "Lubrificante", "Suporte estrutural", "Hormônio"},
        correctIndex = 0,
        questionNumber = 30,
        isImageAnswer = false,
        isImageQuestion = false,
        questionImagePath = "",
        questionLevel = 1,
        questionInDevelopment = false,
        textHint = "A heparina atua como anticoagulante.\nEla impede a formação de coágulos no sangue.\nÉ muito usada em contextos médicos.\nPegadinha: apesar de ser um GAG, sua função principal não é estrutural.",
        bloomLevel = "lembrar"
    },
        // 30
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "1-beta-4",
                "1-alfa-4",
                "1-beta-3",
                "1-alfa-3"
            },
            correctIndex = 2,
            questionNumber = 31,
            isImageAnswer = false,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/CarbohydrateDB/carbohydrateDB_ImageQuestionContainer31",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Observe quais carbonos estão ligados e a orientação do grupo OH.\nO padrão “1→3” indica os carbonos envolvidos.\nO termo beta depende da posição espacial da ligação.\nPegadinha: alfa vs beta muda completamente a geometria da molécula.",
            bloomLevel = "analisar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual a principal função do condroitin?",
            answers = new string[] {"Suporte estrutural em cartilagens", "hormônio", "Neurotransmissor", "Todas as alternativas acima"},
            correctIndex = 0,
            questionNumber = 32,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "O condroitin ajuda a dar resistência e elasticidade às cartilagens.\nEle faz parte da matriz extracelular.\nFunciona como um “amortecedor” biológico.\nPegadinha: não atua como hormônio nem neurotransmissor.",
            bloomLevel = "compreender"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "1-beta-4",
                "1-alfa-4",
                "1-beta-3",
                "1-alfa-3"
            },
            correctIndex = 0,
            questionNumber = 33,
            isImageAnswer = false,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/CarbohydrateDB/carbohydrateDB_ImageQuestionContainer33",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Identifique os carbonos que fazem a ligação e a orientação beta.\nO padrão 1→4 é muito comum em estruturas como celulose.\nObserve a geometria da ligação.\nPegadinha: alfa(1→4) aparece muito no amido — não confundir.",
            bloomLevel = "analisar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "AnswerImages/CarbohydrateDB/L-glicose",
                "AnswerImages/CarbohydrateDB/D-frutose",
                "AnswerImages/CarbohydrateDB/alfa-galactopiranose",
                "AnswerImages/CarbohydrateDB/D-galactose"
            },
            correctIndex = 1,
            questionNumber = 34,
            isImageAnswer = true,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/CarbohydrateDB/carbohydrateDB_ImageQuestionContainer34",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "A frutose é uma cetose, então procure o grupo cetona.\nEla geralmente forma estruturas cíclicas diferentes da glicose.\nCompare a posição do grupo carbonila.\nPegadinha: glicose e galactose são aldoses, não cetoses.",
            bloomLevel = "analisar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "AnswerImages/CarbohydrateDB/L-glicose",
                "AnswerImages/CarbohydrateDB/D-frutose",
                "AnswerImages/CarbohydrateDB/D-tagatose",
                "AnswerImages/CarbohydrateDB/D-glicose"
            },
            correctIndex = 3,
            questionNumber = 35,
            isImageAnswer = true,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/CarbohydrateDB/carbohydrateDB_ImageQuestionContainer35",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "A glicose é uma aldose com 6 carbonos (hexose).\nProcure o grupo aldeído na forma aberta ou padrão típico na cíclica.\nCompare com frutose (cetose).\nPegadinha: tagatose e frutose confundem por serem cetoses.",
            bloomLevel = "analisar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "AnswerImages/CarbohydrateDB/beta-galactopiranose",
                "AnswerImages/CarbohydrateDB/alfa-galactopiranose",
                "AnswerImages/CarbohydrateDB/beta-glicopiranose",
                "AnswerImages/CarbohydrateDB/alfa-glicopiranose"
            },
            correctIndex = 2,
            questionNumber = 36,
            isImageAnswer = true,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/CarbohydrateDB/carbohydrateDB_ImageQuestionContainer36",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Procure a glicose em forma beta (OH do carbono anomérico para cima).\nA diferença alfa/beta está só nesse carbono.\nCompare a orientação do grupo OH.\nPegadinha: galactose muda em outro carbono, não no anomérico.",
            bloomLevel = "analisar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Identifique abaixo a estrutura da glicose, que também pode ser nomeada como beta-D-glicopiranose",
            answers = new string[] {
                "AnswerImages/CarbohydrateDB/beta-galactopiranose",
                "AnswerImages/CarbohydrateDB/alfa-galactopiranose",
                "AnswerImages/CarbohydrateDB/alfa-glicopiranose",
                "AnswerImages/CarbohydrateDB/beta-glicopiranose"
            },
            correctIndex = 3,
            questionNumber = 37,
            isImageAnswer = true,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Beta-D-glicopiranose é a forma cíclica da glicose com OH anomérico para cima.\n“D” indica a configuração da molécula.\n“Piranose” indica um anel de 6 membros.\nPegadinha: alfa muda só a orientação do OH.",
            bloomLevel = "aplicar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "AnswerImages/CarbohydrateDB/beta-galactopiranose",
                "AnswerImages/CarbohydrateDB/alfa-galactopiranose",
                "AnswerImages/CarbohydrateDB/alfa-glicopiranose",
                "AnswerImages/CarbohydrateDB/beta-glicopiranose"
            },
            correctIndex = 2,
            questionNumber = 38,
            isImageAnswer = true,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/CarbohydrateDB/carbohydrateDB_ImageQuestionContainer38",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Identifique a glicose e depois observe se está na forma alfa.\nAlfa significa OH do carbono anomérico voltado para baixo.\nCompare com a forma beta.\nPegadinha: galactose parece muito com glicose.",
            bloomLevel = "analisar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual a diferença entre um homopolissacarídeo e um heteropolissacarídeo?",
            answers = new string[] {
                "Um homopolissacarídeo contém apenas um tipo de monossacarídeo, um heteropolissacarídeo contém múltiplos tipos",
                "Um homopolissacarídeo é linear, um heteropolissacarídeo é ramificado",
                "Um homopolissacarídeo é solúvel em água, um heteropolissacarídeo é insolúvel",
                "Um homopolissacarídeo é um polímero, um heteropolissacarídeo é um monômero"
            },
            correctIndex = 0,
            questionNumber = 39,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 3,
            questionInDevelopment = false,
            textHint = "Homopolissacarídeos têm um único tipo de monossacarídeo repetido.\nHeteropolissacarídeos têm diferentes tipos combinados.\nÉ como repetir uma mesma peça vs misturar várias.\nPegadinha: não depende de forma (linear/ramificada).",
            bloomLevel = "compreender"
        },
        new Question {
    questionDatabankName = "CarbohydratesQuestionDatabase",
    questionText = "Dê exemplos de homopolissacarídeos.",
    answers = new string[] {
        "Amido, glicogênio, celulose",
        "Sacarose, lactose, maltose",
        "Glicose, frutose, galactose",
        "Todos os polissacarídeos são homopolissacarídeos"},
    correctIndex = 0,
    questionNumber = 40,
    isImageAnswer = false,
    isImageQuestion = false,
    questionImagePath = "",
    questionLevel = 3,
    questionInDevelopment = false,
    textHint = "Amido, glicogênio e celulose são feitos só de glicose.\nPor isso são homopolissacarídeos.\nEles variam na forma e função, mas não no tipo de monômero.\nPegadinha: dissacarídeos e monossacarídeos não entram nessa classificação.",
    bloomLevel = "lembrar"
},
        // 40
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Dê exemplos de heteropolissacarídeos.",
            answers = new string[] {
                "Amido, glicogênio, celulose",
                "Sacarose, lactose, maltose",
                "Peptídeoglicano, glicosaminoglicanos",
                "Glicose, frutose, galactose"
            },
            correctIndex = 2,
            questionNumber = 41,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "Heteropolissacarídeos são formados por diferentes monossacarídeos.\nExemplos incluem glicosaminoglicanos e peptidoglicanos.\nEles têm funções estruturais importantes.\nPegadinha: amido e glicogênio são homogêneos (só glicose).",
            bloomLevel = "lembrar"
        },
        new Question
        {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual a principal diferença entre a estrutura da amilose e da celulose?",
            answers = new string[] {
                "A amilose é ramificada, a celulose é linear.",
                "A amilose possui ligações alfa(1 -> 4), a celulose possui ligações beta(1 -> 4).",
                "A amilose é um polímero de frutose, a celulose é um polímero de glicose.",
                "A amilose é insolúvel em água, a celulose é solúvel em água."
            },
            correctIndex = 1,
            questionNumber = 42,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 3,
            questionInDevelopment = false,
            textHint = "A diferença chave está na ligação: alfa na amilose e beta na celulose.\nIsso muda completamente a forma da molécula.\nAmilose é mais flexível; celulose é rígida.\nPegadinha: ambas são feitas de glicose.",
            bloomLevel = "compreender"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "AnswerImages/AminoacidsDB/leucina",
                "AnswerImages/AminoacidsDB/prolina",
                "AnswerImages/AminoacidsDB/triptofano",
                "AnswerImages/AminoacidsDB/asparagina"
            },
            correctIndex = 3,
            questionNumber = 43,
            isImageAnswer = true,
            isImageQuestion = true,
            questionImagePath = "AnswerImages/SugarDB/sugar_question_43",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "Aqui você deve reconhecer o grupo funcional característico do aminoácido.\nA asparagina possui grupo amida na cadeia lateral.\nObserve presença de nitrogênio adicional.\nPegadinha: leucina e prolina são apolares e bem diferentes estruturalmente.",
            bloomLevel = "analisar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "",
            answers = new string[] {
                "AnswerImages/AminoacidsDB/leucina",
                "AnswerImages/AminoacidsDB/serina",
                "AnswerImages/AminoacidsDB/triptofano",
                "AnswerImages/AminoacidsDB/asparagina"
            },
            correctIndex = 1,
            questionNumber = 44,
            isImageAnswer = true,
            isImageQuestion = true,
            questionImagePath = "AnswerImages/SugarDB/sugar_question_44",
            questionLevel = 2,
            questionInDevelopment = false,
            textHint = "A serina possui um grupo hidroxila (-OH) na cadeia lateral.\nIsso a torna polar.\nProcure uma estrutura com OH extra.\nPegadinha: triptofano é muito maior e aromático.",
            bloomLevel = "analisar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Os carboidratos são moléculas formadas, principalmente, por:",
            answers = new string[] {
                "Hidrogênio, enxofre e oxigênio",
                "Carbono, hidrogênio e oxigênio",
                "Carbono, nitrogênio e fósforo",
                "Oxigênio, fósforo e magnésio",
            },
            correctIndex = 1,
            questionNumber = 45,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "Carboidratos são formados principalmente por carbono, hidrogênio e oxigênio.\nEsses três elementos aparecem na fórmula geral.\nÉ uma das definições básicas da classe.\nPegadinha: nitrogênio aparece em proteínas, não é padrão aqui.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "A fórmula geral mais comum dos monossacarídeos é:",
            answers = new string[] {
                "CnH2n-2On",
                "CnHnOn",
                "CnH2n+2On",
                "CnH2nOn"
            },
            correctIndex = 3,
            questionNumber = 46,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "A forma mais comum é CnH2nOn.\nIsso reflete a proporção típica entre os elementos.\nEquivale à ideia de (CH2O)n.\nPegadinha: pequenas mudanças na fórmula tornam a alternativa incorreta.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "A ligação que une os monossacarídeos para formar dissacarídeos e polissacarídeos chama-se:",
            answers = new string[] {
                "Ligação glicosídica",
                "Ligação peptídica",
                "Ligação dissulfeto",
                "Ligação fosfodiéster"
            },
            correctIndex = 0,
            questionNumber = 47,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "A ligação glicosídica conecta açúcares formando cadeias.\nEla envolve o carbono anomérico.\nÉ essencial para formar di e polissacarídeos.\nPegadinha: peptídica é ligação de proteínas.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual polissacarídeo é a principal reserva energética dos animais?",
            answers = new string[] {
                "Amido",
                "Quitina",
                "Glicogênio",
                "Celulose"
            },
            correctIndex = 2,
            questionNumber = 48,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "O glicogênio é a forma de armazenamento de glicose em animais.\nEle é altamente ramificado.\nPermite liberação rápida de energia.\nPegadinha: amido é reserva vegetal.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "A quitina é um polissacarídeo estrutural encontrado em:",
            answers = new string[] {
                "Fígado humano",
                "Algas vermelhas",
                "Plantas superiores",
                "Parede celular de fungos e exoesqueleto de artrópodes"
            },
            correctIndex = 3,
            questionNumber = 49,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "A quitina forma estruturas rígidas em fungos e artrópodes.\nEla dá proteção e sustentação.\nÉ semelhante à celulose, mas com nitrogênio.\nPegadinha: não está em plantas (lá é celulose).",
            bloomLevel = "lembrar"
        },
        new Question {
    questionDatabankName = "CarbohydratesQuestionDatabase",
    questionText = "A sacarose é formada pela união de quais monossacarídeos?",
    answers = new string[] {
        "Glicose + Frutose",
        "Glicose + Galactose",
        "Glicose + Glicose",
        "Frutose + Galactose"
    },
    correctIndex = 0,
    questionNumber = 50,
    isImageAnswer = false,
    isImageQuestion = false,
    questionImagePath = "",
    questionLevel = 1,
    questionInDevelopment = false,
    textHint = "A sacarose é formada por glicose + frutose.\nÉ o açúcar de mesa comum.\nEssa combinação é clássica em bioquímica.\nPegadinha: glicose + galactose forma lactose.",
    bloomLevel = "lembrar"
},
        // 50
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "A glicose, a frutose e a galactose são exemplos de:",
            answers = new string[] {
                "Oligossacarídeos",
                "Monossacarídeos",
                "Dissacarídeos",
                "Polissacarídeos"
            },
            correctIndex = 1,
            questionNumber = 51,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "Glicose, frutose e galactose são unidades básicas de carboidratos.\nPor isso são monossacarídeos.\nEles não precisam ser quebrados para gerar energia.\nPegadinha: dissacarídeos são formados por dois monossacarídeos.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "A celulose difere do amido principalmente porque:",
            answers = new string[] {
                "É solúvel em água, enquanto o amido não é",
                "Possui ligações β(1→4), enquanto o amido possui α(1→4)",
                "É formada por frutose, enquanto o amido é formado por glicose",
                "É composta por galactose, e não glicosedeos"
            },
            correctIndex = 1,
            questionNumber = 52,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "A diferença está no tipo de ligação: beta na celulose, alfa no amido.\nIsso muda completamente a estrutura.\nCelulose forma fibras rígidas, amido é mais acessível.\nPegadinha: ambos são feitos de glicose.",
            bloomLevel = "compreender"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual das funções abaixo NÃO está relacionada aos carboidratos?",
            answers = new string[] {
                "Estrutura celular",
                "Fornecimento de energia",
                "Armazenamento de energia",
                "Catálise enzimática direta"
            },
            correctIndex = 3,
            questionNumber = 53,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "Carboidratos atuam como energia, reserva e estrutura.\nMas não catalisam reações diretamente.\nEssa função é típica de enzimas (proteínas).\nPegadinha: alguns carboidratos participam de reconhecimento, mas não catalisam.",
            bloomLevel = "compreender"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Os carboidratos também são conhecidos como:",
            answers = new string[] {
                "Lípidos",
                "Glicídios",
                "Aminoácidos",
                "Nucleotídeos"
            },
            correctIndex = 1,
            questionNumber = 54,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "Carboidratos também são chamados de glicídios.\nEsse é um termo equivalente em bioquímica.\nPode aparecer em provas e textos.\nPegadinha: lipídios e aminoácidos são outras classes.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "A fórmula geral mais comum dos monossacarídeos é:",
            answers = new string[] {
                "Cn(H₂O)n",
                "CnH₂nOn",
                "Cn(H₂O)2n",
                "CnHnOn"
            },
            correctIndex = 1,
            questionNumber = 55,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "A fórmula mais usada é CnH2nOn.\nEla representa a proporção típica entre os elementos.\nEquivale à ideia de (CH2O)n.\nPegadinha: outras fórmulas parecem corretas, mas não seguem essa proporção.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual dos seguintes é um monossacarídeo?",
            answers = new string[] {
                "Sacarose",
                "Maltose",
                "Glicose",
                "Amido"
            },
            correctIndex = 2,
            questionNumber = 56,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "A glicose é um monossacarídeo simples.\nEla é a principal fonte de energia celular.\nNão precisa ser quebrada para ser usada.\nPegadinha: sacarose e maltose são dissacarídeos.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "A sacarose, encontrada no açúcar de mesa, é formada por:",
            answers = new string[] {
                "Glicose + Frutose",
                "Glicose + Galactose",
                "Frutose + Galactose",
                "Glicose + Glicose"
            },
            correctIndex = 0,
            questionNumber = 57,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "A sacarose combina glicose e frutose.\nÉ o açúcar que usamos no dia a dia.\nEssa combinação é clássica.\nPegadinha: glicose + galactose forma lactose.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "O amido é o principal polissacarídeo de reserva:",
            answers = new string[] {
                "Animal",
                "Fúngico",
                "Vegetal",
                "Bacteriano"
            },
            correctIndex = 2,
            questionNumber = 58,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "O amido é a reserva energética das plantas.\nEle armazena glicose para uso futuro.\nEncontrado em sementes e tubérculos.\nPegadinha: animais usam glicogênio, não amido.",
            bloomLevel = "lembrar"
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "O glicogênio é o polissacarídeo de reserva característico de:",
            answers = new string[] {
                "Plantas",
                "Fungos e animais",
                "Bactérias",
                "Protozoários"
            },
            correctIndex = 1,
            questionNumber = 59,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false,
            textHint = "O glicogênio é usado por animais e também por fungos.\nEle é altamente ramificado.\nPermite liberação rápida de energia.\nPegadinha: plantas usam amido.",
            bloomLevel = "lembrar"
        },
        new Question {
    questionDatabankName = "CarbohydratesQuestionDatabase",
    questionText = "A celulose tem como função principal nos vegetais:",
    answers = new string[] {
        "Armazenar energia",
        "Dar resistência à parede celular",
        "Servir como reserva osmótica",
        "Participar da respiração celular"
    },
    correctIndex = 1,
    questionNumber = 60,
    isImageAnswer = false,
    isImageQuestion = false,
    questionImagePath = "",
    questionLevel = 1,
    questionInDevelopment = false,
    textHint = "A celulose dá rigidez e resistência às plantas.\nEla compõe a parede celular.\nFunciona como uma estrutura de sustentação.\nPegadinha: não é usada como reserva de energia.",
    bloomLevel = "lembrar"
},
        // 60
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "A lactose, presente no leite, é formada por:",
            answers = new string[] {
                "Glicose + Glicose",
                "Glicose + Galactose",
                "Glicose + Frutose",
                "Galactose + Frutose"
            },
            correctIndex = 1,
            questionNumber = 61,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Entre as alternativas abaixo, qual é um polissacarídeo estrutural?",
            answers = new string[] {
                "Amido",
                "Glicogênio",
                "Celulose",
                "Maltose"
            },
            correctIndex = 2,
            questionNumber = 62,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "A fórmula geral mais comum dos carboidratos é:",
            answers = new string[] {
                "Cn(H₂O)n",
                "CnH₂nOn",
                "CnH₂n+₂On",
                "CnHnOn"
            },
            correctIndex = 0,
            questionNumber = 63,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "A glicose é classificada como:",
            answers = new string[] {
                "Dissacarídeo",
                "Monossacarídeo",
                "Polissacarídeo",
                "Oligossacarídeo"
            },
            correctIndex = 1,
            questionNumber = 64,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "A sacarose é formada pela união de:",
            answers = new string[] {
                "Glicose + Glicose",
                "Glicose + Galactose",
                "Glicose + Frutose",
                "Glicose + Manose"
            },
            correctIndex = 2,
            questionNumber = 65,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false
        },
         new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual destes carboidratos é considerado um polissacarídeo de armazenamento em animais?",
            answers = new string[] {
                "Amido",
                "Celulose",
                "Glicogênio",
                "Quitina"
            },
            correctIndex = 2,
            questionNumber = 66,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual destes carboidratos é um polissacarídeo estrutural presente na parede celular de vegetais?",
            answers = new string[] {
                "Amido",
                "Celulose",
                "Maltose",
                "Lactose"
            },
            correctIndex = 1,
            questionNumber = 67,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual destes carboidratos está presente no leite?",
            answers = new string[] {
                "Sacarose",
                "Lactose",
                "Maltose",
                "Glicogênio"
            },
            correctIndex = 1,
            questionNumber = 68,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Os carboidratos podem ser classificados de acordo com:",
            answers = new string[] {
                "O número de radicais R",
                "O número de átomos de carbono e grupos funcionais",
                "A presença de ácidos graxos",
                "O número de ligações peptídicas"
            },
            correctIndex = 1,
            questionNumber = 69,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false
        },
        new Question {
            questionDatabankName = "CarbohydratesQuestionDatabase",
            questionText = "Qual das opções representa uma função principal dos carboidratos nos organismos vivos?",
            answers = new string[] {
                "Atuar como catalisadores biológicos",
                "Armazenar informações genéticas",
                "Fornecer energia e servir como reserva energética",
                "Compor membranas celulares"
            },
            correctIndex = 2,
            questionNumber = 70,
            isImageAnswer = false,
            isImageQuestion = false,
            questionImagePath = "",
            questionLevel = 1,
            questionInDevelopment = false
        }
    };

    public List<Question> GetQuestions() => questions;
    public QuestionSet GetQuestionSetType() => QuestionSet.carbohydrates;
    public string GetDatabankName()  => "CarbohydratesQuestionDatabase";
    public string GetDisplayName()   => "Carboidratos";
    public bool IsDatabaseInDevelopment() => databaseInDevelopment;

}