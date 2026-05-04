using System.Collections.Generic;
using QuestionSystem;

public class CarbohydratesQuestionDatabase : IQuestionDatabase
{
    private bool databaseInDevelopment = false;
    
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
            globalId = "carbohydrates_001",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Os monossacarídeos seguem geralmente a fórmula (CH2O)n, que mostra uma proporção típica entre carbono, hidrogênio e oxigênio.
É como uma “receita básica” que se repete, variando só o tamanho da molécula.
Cuidado: algumas alternativas parecem parecidas, mas alteram essa proporção clássica.
Nem toda fórmula com C, H e O está correta — a proporção importa!" }
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
            globalId = "carbohydrates_002",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A diferença está no grupo funcional: aldoses têm aldeído, cetoses têm cetona.
Pense nisso como mudar a “posição da função” na molécula.
Isso altera propriedades químicas importantes, mesmo com mesma fórmula.
Pegadinha: não tem relação com tamanho nem solubilidade." }
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
            globalId = "carbohydrates_003",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "aplicar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Cetoses possuem grupo cetona, geralmente no carbono interno.
Na nomenclatura, “ceto-” já indica isso.
Observe a posição do grupo carbonila na estrutura.
Pegadinha: “aldo-” indica aldeído — não confundir!" }
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
            globalId = "carbohydrates_004",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "aplicar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "“Aldo” indica presença de aldeído e “tetrose” indica 4 carbonos.
Então você procura uma cadeia com 4 C e grupo aldeído.
É como ler o nome químico como uma descrição.
Pegadinha: triose (3C) e pentose (5C) estão ali pra confundir." }
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
            globalId = "carbohydrates_005",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A forma L ou D depende da orientação do carbono quiral mais distante do grupo carbonila.
É como um “espelho” da molécula.
Compare com o padrão da glicose conhecida.
Pegadinha: D ≠ direita na tela — é uma convenção estrutural." }
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
            globalId = "carbohydrates_006",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Epímeros diferem em apenas UM carbono quiral.
É como duas moléculas quase iguais, mudando só um detalhe.
Isso pode mudar propriedades importantes.
Pegadinha: não são espelhos completos (isso seria enantiômero)." }
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
            globalId = "carbohydrates_007",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A tagatose é uma cetose, então procure o grupo cetona.
Além disso, observe a configuração D.
Compare com glicose/frutose para eliminar alternativas.
Pegadinha: L/D e tipo (aldo/ceto) aparecem misturados." }
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
            globalId = "carbohydrates_008",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Anômeros diferem apenas no carbono anomérico após ciclagem.
Isso gera formas alfa e beta.
É como virar só uma “pecinha” da molécula.
Pegadinha: não confundir com epímeros (outros carbonos quirais)." }
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
            globalId = "carbohydrates_009",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Procure a forma cíclica da glicose (piranose).
Depois veja se o OH do carbono anomérico está em posição alfa.
Alfa geralmente aponta para baixo na projeção.
Pegadinha: galactose é parecida, mas muda em um carbono." }
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
            globalId = "carbohydrates_010",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A sacarose é o açúcar comum da cana e do açúcar de mesa.
Ela é formada por glicose + frutose.
É o que usamos no dia a dia no café.
Pegadinha: lactose é do leite e maltose vem do amido." }
        },
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
            globalId = "carbohydrates_011",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A lactose é o açúcar característico do leite.
Ela é formada por glicose + galactose.
Por isso, intolerância à lactose envolve dificuldade em quebrar essa molécula.
Pegadinha: sacarose é açúcar de mesa, não do leite." }
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
            globalId = "carbohydrates_012",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Monossacarídeos se unem por ligações glicosídicas.
É como um “elo” entre açúcares formando cadeias maiores.
Essa ligação envolve o carbono anomérico.
Pegadinha: peptídica é de proteínas, fosfodiéster é de DNA/RNA." }
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
            globalId = "carbohydrates_013",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "aplicar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Dissacarídeos são formados por dois monossacarídeos ligados.
Visualmente, você deve identificar duas unidades conectadas.
É como juntar dois blocos de Lego.
Pegadinha: polissacarídeos têm muitas unidades, não só duas." }
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
            globalId = "carbohydrates_014",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Observe os carbonos envolvidos na ligação e a orientação do OH.
“1→4” indica quais carbonos estão ligados.
“beta” depende da posição espacial do grupo.
Pegadinha: alfa vs beta muda totalmente a estrutura final!" }
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
            globalId = "carbohydrates_015",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Plantas armazenam energia principalmente na forma de amido.
É como o “estoque de energia” vegetal.
Encontrado em alimentos como batata e arroz.
Pegadinha: celulose é estrutural, não reserva." }
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
            globalId = "carbohydrates_016",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Animais armazenam glicose na forma de glicogênio.
Ele fica principalmente no fígado e músculos.
É como uma “bateria de energia rápida”.
Pegadinha: amido é reserva vegetal, não animal." }
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
            globalId = "carbohydrates_017",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A amilose é linear, enquanto a amilopectina é ramificada.
Isso afeta como a molécula ocupa espaço.
Imagine uma corda vs uma árvore com galhos.
Pegadinha: ambas têm ligações alfa, então não é esse o diferencial principal." }
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
            globalId = "carbohydrates_018",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A celulose possui ligações beta(1→4).
Isso gera uma estrutura rígida e resistente.
É como fibras bem alinhadas formando parede celular.
Pegadinha: alfa(1→4) é do amido, não da celulose." }
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
            globalId = "carbohydrates_019",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O amido possui principalmente ligações alfa(1→4).
Isso permite formar estruturas mais flexíveis.
Facilita a digestão por enzimas humanas.
Pegadinha: beta(1→4) é resistente (celulose)." }
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
            globalId = "carbohydrates_020",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A amilase quebra o amido em açúcares menores.
Ela está presente na saliva e no pâncreas.
Por isso a digestão de carboidratos começa na boca.
Pegadinha: lactase atua na lactose, não no amido." }
        },
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
            globalId = "carbohydrates_021",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A celulose é quebrada pela enzima celulase.
Humanos não produzem essa enzima, por isso não digerimos celulose.
Alguns microrganismos fazem isso (ex: em ruminantes).
Pegadinha: amilase não quebra celulose." }
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
            globalId = "carbohydrates_022",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Glicoproteínas são proteínas com cadeias de carboidratos ligadas.
Elas aparecem muito na membrana celular.
Funcionam como reconhecimento e sinalização.
Pegadinha: não confundir com glicolipídeos (lipídios + carboidratos)." }
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
            globalId = "carbohydrates_023",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Glicosaminoglicanos são heteropolissacarídeos da matriz extracelular.
Eles ajudam a dar suporte e retenção de água aos tecidos.
São como uma “esponja estrutural”.
Pegadinha: não são simples dissacarídeos nem ficam no citoplasma." }
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
            globalId = "carbohydrates_024",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Proteoglicanos são proteínas ligadas a glicosaminoglicanos.
Eles formam estruturas grandes na matriz extracelular.
Ajudam na hidratação e resistência dos tecidos.
Pegadinha: glicoproteínas têm menos carboidrato que proteoglicanos." }
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
            globalId = "carbohydrates_025",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Eles são encontrados principalmente na matriz extracelular.
Essa região dá suporte aos tecidos fora das células.
Funcionam como preenchimento e estrutura.
Pegadinha: não ficam dentro da célula como principal localização." }
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
            globalId = "carbohydrates_026",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "aplicar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Glicosaminoglicanos contêm açúcares modificados com grupos amina.
A galactosamina é um exemplo clássico.
Procure por estruturas com nitrogênio ligado.
Pegadinha: glicose simples não tem esse grupo amina." }
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
            globalId = "carbohydrates_027",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O ácido hialurônico é formado por unidades repetidas específicas.
Ele não possui sulfatação, diferente de outros GAGs.
É muito abundante em tecidos conjuntivos.
Pegadinha: dermatan e condroitin são parecidos, mas diferentes na composição." }
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
            globalId = "carbohydrates_028",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O ácido hialurônico atua em lubrificação, suporte e viscosidade.
Ele retém água e forma um gel nos tecidos.
Muito importante em articulações e pele.
Pegadinha: não tem só uma função — são várias ao mesmo tempo." }
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
            globalId = "carbohydrates_029",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A heparina é formada por unidades altamente sulfatadas.
Isso explica sua carga negativa elevada.
Essa característica é essencial para sua função biológica.
Pegadinha: não confundir com outros GAGs menos carregados." }
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
            globalId = "carbohydrates_030",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A heparina é formada por unidades altamente sulfatadas.
Isso explica sua carga negativa elevada.
Essa característica é essencial para sua função biológica.
Pegadinha: não confundir com outros GAGs menos carregados." }
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
            correctIndex = 2,
            questionNumber = 31,
            isImageAnswer = false,
            isImageQuestion = true,
            questionImagePath = "QuestionImages/CarbohydrateDB/carbohydrateDB_ImageQuestionContainer31",
            questionLevel = 2,
            questionInDevelopment = false,
            globalId = "carbohydrates_031",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Observe quais carbonos estão ligados e a orientação do grupo OH.
O padrão “1→3” indica os carbonos envolvidos.
O termo beta depende da posição espacial da ligação.
Pegadinha: alfa vs beta muda completamente a geometria da molécula." }
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
            globalId = "carbohydrates_032",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O condroitin ajuda a dar resistência e elasticidade às cartilagens.
Ele faz parte da matriz extracelular.
Funciona como um “amortecedor” biológico.
Pegadinha: não atua como hormônio nem neurotransmissor." }
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
            globalId = "carbohydrates_033",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Identifique os carbonos que fazem a ligação e a orientação beta.
O padrão 1→4 é muito comum em estruturas como celulose.
Observe a geometria da ligação.
Pegadinha: alfa(1→4) aparece muito no amido — não confundir." }
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
            globalId = "carbohydrates_034",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A frutose é uma cetose, então procure o grupo cetona.
Ela geralmente forma estruturas cíclicas diferentes da glicose.
Compare a posição do grupo carbonila.
Pegadinha: glicose e galactose são aldoses, não cetoses." }
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
            globalId = "carbohydrates_035",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A glicose é uma aldose com 6 carbonos (hexose).
Procure o grupo aldeído na forma aberta ou padrão típico na cíclica.
Compare com frutose (cetose).
Pegadinha: tagatose e frutose confundem por serem cetoses." }
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
            globalId = "carbohydrates_036",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Procure a glicose em forma beta (OH do carbono anomérico para cima).
A diferença alfa/beta está só nesse carbono.
Compare a orientação do grupo OH.
Pegadinha: galactose muda em outro carbono, não no anomérico." }
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
            globalId = "carbohydrates_037",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "aplicar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Beta-D-glicopiranose é a forma cíclica da glicose com OH anomérico para cima.
“D” indica a configuração da molécula.
“Piranose” indica um anel de 6 membros.
Pegadinha: alfa muda só a orientação do OH." }
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
            globalId = "carbohydrates_038",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Identifique a glicose e depois observe se está na forma alfa.
Alfa significa OH do carbono anomérico voltado para baixo.
Compare com a forma beta.
Pegadinha: galactose parece muito com glicose." }
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
            globalId = "carbohydrates_039",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Homopolissacarídeos têm um único tipo de monossacarídeo repetido.
Heteropolissacarídeos têm diferentes tipos combinados.
É como repetir uma mesma peça vs misturar várias.
Pegadinha: não depende de forma (linear/ramificada)." }
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
            globalId = "carbohydrates_040",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Amido, glicogênio e celulose são feitos só de glicose.
Por isso são homopolissacarídeos.
Eles variam na forma e função, mas não no tipo de monômero.
Pegadinha: dissacarídeos e monossacarídeos não entram nessa classificação." }
        },
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
            globalId = "carbohydrates_041",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Heteropolissacarídeos são formados por diferentes monossacarídeos.
Exemplos incluem glicosaminoglicanos e peptidoglicanos.
Eles têm funções estruturais importantes.
Pegadinha: amido e glicogênio são homogêneos (só glicose)." }
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
            globalId = "carbohydrates_042",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A diferença chave está na ligação: alfa na amilose e beta na celulose.
Isso muda completamente a forma da molécula.
Amilose é mais flexível; celulose é rígida.
Pegadinha: ambas são feitas de glicose." }
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
            globalId = "carbohydrates_043",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Aqui você deve reconhecer o grupo funcional característico do aminoácido.
A asparagina possui grupo amida na cadeia lateral.
Observe presença de nitrogênio adicional.
Pegadinha: leucina e prolina são apolares e bem diferentes estruturalmente." }
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
            globalId = "carbohydrates_044",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "analisar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A serina possui um grupo hidroxila (-OH) na cadeia lateral.
Isso a torna polar.
Procure uma estrutura com OH extra.
Pegadinha: triptofano é muito maior e aromático." }
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
            globalId = "carbohydrates_045",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Carboidratos são formados principalmente por carbono, hidrogênio e oxigênio.
Esses três elementos aparecem na fórmula geral.
É uma das definições básicas da classe.
Pegadinha: nitrogênio aparece em proteínas, não é padrão aqui." }
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
            globalId = "carbohydrates_046",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A forma mais comum é CnH2nOn.
Isso reflete a proporção típica entre os elementos.
Equivale à ideia de (CH2O)n.
Pegadinha: pequenas mudanças na fórmula tornam a alternativa incorreta." }
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
            globalId = "carbohydrates_047",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A ligação glicosídica conecta açúcares formando cadeias.
Ela envolve o carbono anomérico.
É essencial para formar di e polissacarídeos.
Pegadinha: peptídica é ligação de proteínas." }
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
            globalId = "carbohydrates_048",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O glicogênio é a forma de armazenamento de glicose em animais.
Ele é altamente ramificado.
Permite liberação rápida de energia.
Pegadinha: amido é reserva vegetal." }
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
            globalId = "carbohydrates_049",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A quitina forma estruturas rígidas em fungos e artrópodes.
Ela dá proteção e sustentação.
É semelhante à celulose, mas com nitrogênio.
Pegadinha: não está em plantas (lá é celulose)." }
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
            globalId = "carbohydrates_050",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A sacarose é formada por glicose + frutose.
É o açúcar de mesa comum.
Essa combinação é clássica em bioquímica.
Pegadinha: glicose + galactose forma lactose." }
        },
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
            globalId = "carbohydrates_051",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Glicose, frutose e galactose são unidades básicas de carboidratos.
Por isso são monossacarídeos.
Eles não precisam ser quebrados para gerar energia.
Pegadinha: dissacarídeos são formados por dois monossacarídeos." }
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
            globalId = "carbohydrates_052",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A diferença está no tipo de ligação: beta na celulose, alfa no amido.
Isso muda completamente a estrutura.
Celulose forma fibras rígidas, amido é mais acessível.
Pegadinha: ambos são feitos de glicose." }
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
            globalId = "carbohydrates_053",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Carboidratos atuam como energia, reserva e estrutura.
Mas não catalisam reações diretamente.
Essa função é típica de enzimas (proteínas).
Pegadinha: alguns carboidratos participam de reconhecimento, mas não catalisam." }
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
            globalId = "carbohydrates_054",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Carboidratos também são chamados de glicídios.
Esse é um termo equivalente em bioquímica.
Pode aparecer em provas e textos.
Pegadinha: lipídios e aminoácidos são outras classes." }
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
            globalId = "carbohydrates_055",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A fórmula mais usada é CnH2nOn.
Ela representa a proporção típica entre os elementos.
Equivale à ideia de (CH2O)n.
Pegadinha: outras fórmulas parecem corretas, mas não seguem essa proporção." }
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
            globalId = "carbohydrates_056",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A glicose é um monossacarídeo simples.
Ela é a principal fonte de energia celular.
Não precisa ser quebrada para ser usada.
Pegadinha: sacarose e maltose são dissacarídeos." }
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
            globalId = "carbohydrates_057",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A sacarose combina glicose e frutose.
É o açúcar que usamos no dia a dia.
Essa combinação é clássica.
Pegadinha: glicose + galactose forma lactose." }
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
            globalId = "carbohydrates_058",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O amido é a reserva energética das plantas.
Ele armazena glicose para uso futuro.
Encontrado em sementes e tubérculos.
Pegadinha: animais usam glicogênio, não amido." }
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
            globalId = "carbohydrates_059",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O glicogênio é usado por animais e também por fungos.
Ele é altamente ramificado.
Permite liberação rápida de energia.
Pegadinha: plantas usam amido." }
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
            globalId = "carbohydrates_060",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A celulose dá rigidez e resistência às plantas.
Ela compõe a parede celular.
Funciona como uma estrutura de sustentação.
Pegadinha: não é usada como reserva de energia." }
        },
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
            questionInDevelopment = false,
            globalId = "carbohydrates_061",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A lactose é o principal açúcar do leite, sendo um dissacarídeo formado por glicose e galactose." }
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
            questionInDevelopment = false,
            globalId = "carbohydrates_062",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A celulose forma as fibras da parede celular das plantas, sendo o principal polissacarídeo estrutural vegetal." }
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
            questionInDevelopment = false,
            globalId = "carbohydrates_063",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "O nome 'carboidrato' ou 'hidrato de carbono' deriva da proporção típica de um átomo de carbono para cada molécula de água: Cn(H2O)n." }
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
            questionInDevelopment = false,
            globalId = "carbohydrates_064",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A glicose é um açúcar simples que não pode ser hidrolisado em compostos menores, logo, é um monossacarídeo." }
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
            questionInDevelopment = false,
            globalId = "carbohydrates_065",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A sacarose é o açúcar de mesa comum, composto pelos monossacarídeos glicose e frutose." }
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
            questionInDevelopment = false,
            globalId = "carbohydrates_066",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "Os animais armazenam sua energia sob a forma de glicogênio, principalmente no fígado e nos músculos." }
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
            questionInDevelopment = false,
            globalId = "carbohydrates_067",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A celulose confere rigidez à parede celular dos vegetais." }
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
            questionInDevelopment = false,
            globalId = "carbohydrates_068",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "lembrar",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A lactose é o açúcar característico encontrado no leite dos mamíferos." }
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
            questionInDevelopment = false,
            globalId = "carbohydrates_069",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A classificação básica envolve o tamanho da cadeia de carbonos (triose, pentose, hexose) e o grupo funcional (aldose ou cetose)." }
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
            questionInDevelopment = false,
            globalId = "carbohydrates_070",
            topic = "carbohydrates",
            subtopic = null,
            displayName = "Carboidratos",
            bloomLevel = "compreender",
            conceptTags = null,
            prerequisites = null,
            questionHint = new QuestionHint { text = "A principal função dos carboidratos no metabolismo é atuar como fonte primária e reserva de energia." }
        }
    };

    public List<Question> GetQuestions() => questions;
    public QuestionSet GetQuestionSetType() => QuestionSet.carbohydrates;
    public string GetDatabankName()  => "CarbohydratesQuestionDatabase";
    public string GetDisplayName()   => "Carboidratos";
    public bool IsDatabaseInDevelopment() => databaseInDevelopment;

}