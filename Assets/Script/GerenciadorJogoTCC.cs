using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GerenciadorJogoTCC : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject painelInicio;
    public GameObject painelDialogo;
    public GameObject painelEscolhas;
    public GameObject painelFinal;

    [Header("Início")]
    public TMP_InputField campoNome;
    public TMP_Dropdown dropdownGenero;
    public Button botaoComecar;
    public Button botaoFaculdade;
    public Button botaoTrabalho;
    public Button botaoGrupoAmigos;

    [Header("Diálogo")]
    public TMP_Text textoAmbiente;
    public TMP_Text textoNomePersonagem;
    public TMP_Text textoFala;
    public TMP_Text textoRespostaJogador;
    public Button botaoContinuar;

    [Header("Escolhas")]
    public Button botaoEscolha1;
    public Button botaoEscolha2;
    public Button botaoEscolha3;
    public TMP_Text textoEscolha1;
    public TMP_Text textoEscolha2;
    public TMP_Text textoEscolha3;

    [Header("Final")]
    public TMP_Text textoFinal;
    public Button botaoReiniciar;

    [Header("Visual Novel")]
    public ControladorCenaVN controladorCena;

    [Header("Aparências do Jogador")]
    public List<AparenciaJogador> aparenciasFemininas = new List<AparenciaJogador>();
    public List<AparenciaJogador> aparenciasMasculinas = new List<AparenciaJogador>();
    public List<AparenciaJogador> aparenciasNaoDefinidas = new List<AparenciaJogador>();

    [Header("Personagens por Ambiente")]
    public List<DadosPersonagem> personagensFaculdade = new List<DadosPersonagem>();
    public List<DadosPersonagem> personagensTrabalho = new List<DadosPersonagem>();
    public List<DadosPersonagem> personagensGrupoAmigos = new List<DadosPersonagem>();

    private string nomeJogador = "Jogador";
    private GeneroJogador generoJogador;
    private TipoAmbiente ambienteAtual = TipoAmbiente.Nenhum;

    private AparenciaJogador aparenciaAtualJogador;
    private Emocao emocaoAtualJogador = Emocao.Neutro;

    private int empatia;
    private int comunicacao;
    private int controleEmocional;
    private int lideranca;

    private List<NoDialogoVN> nos = new List<NoDialogoVN>();
    private int indiceNoAtual = 0;

    private List<DadosPersonagem> personagensAtivos = new List<DadosPersonagem>();

    void Start()
    {
        painelInicio.SetActive(true);
        painelDialogo.SetActive(false);
        painelEscolhas.SetActive(false);
        painelFinal.SetActive(false);

        botaoComecar.onClick.AddListener(PrepararInicio);
        botaoFaculdade.onClick.AddListener(() => SelecionarAmbiente(TipoAmbiente.Faculdade));
        botaoTrabalho.onClick.AddListener(() => SelecionarAmbiente(TipoAmbiente.Trabalho));
        botaoGrupoAmigos.onClick.AddListener(() => SelecionarAmbiente(TipoAmbiente.GrupoDeAmigos));

        botaoContinuar.onClick.AddListener(ContinuarDialogoSimples);
        botaoReiniciar.onClick.AddListener(ReiniciarJogo);
    }

    void PrepararInicio()
    {
        nomeJogador = campoNome.text.Trim();

        if (string.IsNullOrEmpty(nomeJogador))
            nomeJogador = "Jogador";

        generoJogador = (GeneroJogador)dropdownGenero.value;
        aparenciaAtualJogador = SortearAparenciaJogador(generoJogador);
        emocaoAtualJogador = Emocao.Neutro;
    }

    AparenciaJogador SortearAparenciaJogador(GeneroJogador genero)
    {
        List<AparenciaJogador> lista = null;

        switch (genero)
        {
            case GeneroJogador.Feminino:
                lista = aparenciasFemininas;
                break;
            case GeneroJogador.Masculino:
                lista = aparenciasMasculinas;
                break;
            case GeneroJogador.NaoDefinido:
                lista = aparenciasNaoDefinidas;
                break;
        }

        if (lista == null || lista.Count == 0)
            return null;

        return lista[Random.Range(0, lista.Count)];
    }

    void SelecionarAmbiente(TipoAmbiente ambiente)
    {
        ambienteAtual = ambiente;
        personagensAtivos = ObterListaDoAmbiente(ambiente);

        if (personagensAtivos.Count < 6)
        {
            Debug.LogError("Cada ambiente precisa ter exatamente 6 personagens configurados.");
            return;
        }

        painelInicio.SetActive(false);
        painelDialogo.SetActive(true);

        textoAmbiente.text = "Ambiente: " + ambienteAtual.ToString();

        MontarRoteiroBase();
        MostrarNoAtual();
    }

    List<DadosPersonagem> ObterListaDoAmbiente(TipoAmbiente ambiente)
    {
        switch (ambiente)
        {
            case TipoAmbiente.Faculdade:
                return personagensFaculdade;
            case TipoAmbiente.Trabalho:
                return personagensTrabalho;
            case TipoAmbiente.GrupoDeAmigos:
                return personagensGrupoAmigos;
        }

        return new List<DadosPersonagem>();
    }

    void MontarRoteiroBase()
    {
        nos.Clear();
        indiceNoAtual = 0;

        DadosPersonagem p1 = personagensAtivos[0];
        DadosPersonagem p2 = personagensAtivos[1];
        DadosPersonagem p3 = personagensAtivos[2];
        DadosPersonagem p4 = personagensAtivos[3];
        DadosPersonagem p5 = personagensAtivos[4];
        DadosPersonagem p6 = personagensAtivos[5];

        nos.Add(new NoDialogoVN
        {
            id = 0,
            tipoNo = TipoNoDialogo.DialogoSimples,
            personagemFalando = p1,
            mostrarEsquerda = true,
            mostrarCentro = true,
            mostrarDireita = true,
            personagemEsquerda = p1,
            personagemCentro = p2,
            personagemDireita = p3,
            emocaoEsquerda = Emocao.Neutro,
            emocaoCentro = Emocao.Neutro,
            emocaoDireita = Emocao.Feliz,
            falasVariaveis = GerarFalasPorPersonalidade(p1, "apresentacao"),
            respostasJogadorVariaveis = new List<string>
            {
                "Oi, prazer em conhecer vocês.",
                "Olá, espero me dar bem com todo mundo.",
                "Tudo bem, estou tentando me adaptar."
            },
            proximoNoSimples = 1
        });

        nos.Add(new NoDialogoVN
        {
            id = 1,
            tipoNo = TipoNoDialogo.DialogoSimples,
            personagemFalando = p4,
            mostrarEsquerda = true,
            mostrarCentro = true,
            mostrarDireita = true,
            personagemEsquerda = p4,
            personagemCentro = p5,
            personagemDireita = p6,
            emocaoEsquerda = Emocao.Feliz,
            emocaoCentro = Emocao.Neutro,
            emocaoDireita = Emocao.Neutro,
            falasVariaveis = GerarFalasPorPersonalidade(p4, "convivio"),
            respostasJogadorVariaveis = new List<string>
            {
                "Entendi, vou observar melhor como tudo funciona.",
                "Parece que cada pessoa aqui tem um jeito diferente.",
                "Quero aprender a lidar bem com todos."
            },
            proximoNoSimples = 2
        });

        nos.Add(new NoDialogoVN
        {
            id = 2,
            tipoNo = TipoNoDialogo.Escolha,
            personagemFalando = p2,
            mostrarEsquerda = true,
            mostrarCentro = true,
            mostrarDireita = true,
            personagemEsquerda = p1,
            personagemCentro = p2,
            personagemDireita = p3,
            emocaoEsquerda = Emocao.Neutro,
            emocaoCentro = Emocao.Raiva,
            emocaoDireita = Emocao.Neutro,
            falasVariaveis = GerarFalasPorPersonalidade(p2, "conflito"),
            opcoes = new List<OpcaoEscolha>
            {
                new OpcaoEscolha
                {
                    textoOpcao = "Responder com calma e tentar entender",
                    respostaJogador = "Calma, eu quero entender o que aconteceu.",
                    deltaEmpatia = 2,
                    deltaComunicacao = 2,
                    deltaControleEmocional = 1,
                    emocaoJogadorAposEscolha = Emocao.Neutro,
                    emocaoPersonagemAposEscolha = Emocao.Neutro,
                    proximoNo = 3
                },
                new OpcaoEscolha
                {
                    textoOpcao = "Responder de forma neutra e curta",
                    respostaJogador = "Tudo bem, vamos seguir.",
                    deltaComunicacao = 1,
                    deltaControleEmocional = 1,
                    emocaoJogadorAposEscolha = Emocao.Neutro,
                    emocaoPersonagemAposEscolha = Emocao.Neutro,
                    proximoNo = 3
                },
                new OpcaoEscolha
                {
                    textoOpcao = "Responder de forma agressiva",
                    respostaJogador = "Você não precisa falar assim comigo.",
                    deltaEmpatia = -2,
                    deltaControleEmocional = -2,
                    emocaoJogadorAposEscolha = Emocao.Raiva,
                    emocaoPersonagemAposEscolha = Emocao.Raiva,
                    proximoNo = 3
                }
            }
        });

        nos.Add(new NoDialogoVN
        {
            id = 3,
            tipoNo = TipoNoDialogo.DialogoSimples,
            personagemFalando = p3,
            mostrarEsquerda = true,
            mostrarCentro = true,
            mostrarDireita = true,
            personagemEsquerda = p1,
            personagemCentro = p2,
            personagemDireita = p3,
            emocaoEsquerda = Emocao.Neutro,
            emocaoCentro = Emocao.Neutro,
            emocaoDireita = Emocao.Feliz,
            falasVariaveis = GerarFalasPorPersonalidade(p3, "reacao"),
            respostasJogadorVariaveis = new List<string>
            {
                "Certo, acho que agora entendi melhor.",
                "Vou pensar melhor antes de agir.",
                "Foi uma situação mais complicada do que parece."
            },
            proximoNoSimples = 4
        });

        nos.Add(new NoDialogoVN
        {
            id = 4,
            tipoNo = TipoNoDialogo.Escolha,
            personagemFalando = p5,
            mostrarEsquerda = true,
            mostrarCentro = true,
            mostrarDireita = true,
            personagemEsquerda = p4,
            personagemCentro = p5,
            personagemDireita = p6,
            emocaoEsquerda = Emocao.Neutro,
            emocaoCentro = Emocao.Raiva,
            emocaoDireita = Emocao.Neutro,
            falasVariaveis = GerarFalasPorPersonalidade(p5, "pressao"),
            opcoes = new List<OpcaoEscolha>
            {
                new OpcaoEscolha
                {
                    textoOpcao = "Tentar organizar todos",
                    respostaJogador = "Vamos nos organizar e dividir melhor as tarefas.",
                    deltaLideranca = 2,
                    deltaComunicacao = 2,
                    deltaEmpatia = 1,
                    emocaoJogadorAposEscolha = Emocao.Neutro,
                    emocaoPersonagemAposEscolha = Emocao.Neutro,
                    proximoNo = 5
                },
                new OpcaoEscolha
                {
                    textoOpcao = "Fazer sua parte em silêncio",
                    respostaJogador = "Vou focar no que eu consigo fazer agora.",
                    deltaControleEmocional = 1,
                    proximoNo = 5,
                    emocaoJogadorAposEscolha = Emocao.Neutro,
                    emocaoPersonagemAposEscolha = Emocao.Neutro
                },
                new OpcaoEscolha
                {
                    textoOpcao = "Culpar alguém",
                    respostaJogador = "Isso aconteceu porque alguém não fez a parte certa.",
                    deltaEmpatia = -2,
                    deltaLideranca = -1,
                    emocaoJogadorAposEscolha = Emocao.Raiva,
                    emocaoPersonagemAposEscolha = Emocao.Raiva,
                    proximoNo = 5
                }
            }
        });

        nos.Add(new NoDialogoVN
        {
            id = 5,
            tipoNo = TipoNoDialogo.Escolha,
            personagemFalando = p6,
            mostrarEsquerda = true,
            mostrarCentro = true,
            mostrarDireita = true,
            personagemEsquerda = p4,
            personagemCentro = p5,
            personagemDireita = p6,
            emocaoEsquerda = Emocao.Neutro,
            emocaoCentro = Emocao.Neutro,
            emocaoDireita = Emocao.Neutro,
            falasVariaveis = GerarFalasPorPersonalidade(p6, "decisaoFinal"),
            opcoes = new List<OpcaoEscolha>
            {
                new OpcaoEscolha
                {
                    textoOpcao = "Assumir responsabilidade",
                    respostaJogador = "Eu assumo minha parte e vou ajudar a resolver.",
                    deltaEmpatia = 1,
                    deltaComunicacao = 2,
                    deltaLideranca = 2,
                    emocaoJogadorAposEscolha = Emocao.Feliz,
                    emocaoPersonagemAposEscolha = Emocao.Feliz,
                    proximoNo = -1
                },
                new OpcaoEscolha
                {
                    textoOpcao = "Resolver sem se comprometer muito",
                    respostaJogador = "Vamos tentar corrigir isso com calma.",
                    deltaControleEmocional = 1,
                    deltaComunicacao = 1,
                    emocaoJogadorAposEscolha = Emocao.Neutro,
                    emocaoPersonagemAposEscolha = Emocao.Neutro,
                    proximoNo = -1
                },
                new OpcaoEscolha
                {
                    textoOpcao = "Jogar a culpa em outro",
                    respostaJogador = "Isso não foi erro meu.",
                    deltaEmpatia = -2,
                    deltaLideranca = -2,
                    emocaoJogadorAposEscolha = Emocao.Raiva,
                    emocaoPersonagemAposEscolha = Emocao.Raiva,
                    proximoNo = -1
                }
            }
        });
    }

    List<string> GerarFalasPorPersonalidade(DadosPersonagem personagem, string momento)
    {
        List<string> falas = new List<string>();

        switch (personagem.personalidade)
        {
            case PersonalidadePersonagem.Gentil:
                if (momento == "apresentacao")
                {
                    falas.Add("Oi, seja bem-vindo. Espero que você se sinta confortável aqui.");
                    falas.Add("Que bom te conhecer. Pode contar comigo no que precisar.");
                }
                else if (momento == "convivio")
                {
                    falas.Add("Cada pessoa aqui tem um jeito diferente, mas com respeito tudo funciona.");
                    falas.Add("Conviver bem com os outros faz muita diferença no dia a dia.");
                }
                else if (momento == "conflito")
                {
                    falas.Add("Desculpa, eu estou um pouco sobrecarregado hoje.");
                    falas.Add("Não queria descontar isso em você, mas estou cansado.");
                }
                else if (momento == "reacao")
                {
                    falas.Add("Sua reação mostrou bastante maturidade.");
                    falas.Add("Foi bom ver que você tentou lidar bem com a situação.");
                }
                else if (momento == "pressao")
                {
                    falas.Add("O prazo está apertado, então precisamos agir juntos.");
                    falas.Add("Se nos organizarmos bem, ainda dá tempo de resolver.");
                }
                else if (momento == "decisaoFinal")
                {
                    falas.Add("Agora precisamos decidir como vamos enfrentar esse problema.");
                    falas.Add("Esse momento mostra muito sobre nossa responsabilidade.");
                }
                break;

            case PersonalidadePersonagem.Irritado:
                if (momento == "apresentacao")
                {
                    falas.Add("Tá, então você é a pessoa nova.");
                    falas.Add("Certo. Só tenta não atrapalhar.");
                }
                else if (momento == "convivio")
                {
                    falas.Add("Nem todo mundo aqui sabe trabalhar bem em grupo.");
                    falas.Add("Você vai perceber rápido que algumas pessoas complicam tudo.");
                }
                else if (momento == "conflito")
                {
                    falas.Add("Hoje já deu tudo errado e eu não estou com paciência.");
                    falas.Add("Já estou cheio de problema para ainda lidar com isso.");
                }
                else if (momento == "reacao")
                {
                    falas.Add("Bom... pelo menos você não piorou tudo.");
                    falas.Add("Você reagiu melhor do que eu esperava.");
                }
                else if (momento == "pressao")
                {
                    falas.Add("O prazo está acabando e ninguém resolve nada.");
                    falas.Add("Se continuar assim, isso vai dar errado.");
                }
                else if (momento == "decisaoFinal")
                {
                    falas.Add("Agora alguém vai ter que responder por isso.");
                    falas.Add("Não dá mais para fingir que nada aconteceu.");
                }
                break;

            default:
                if (momento == "apresentacao")
                    falas.Add("Olá, prazer em conhecer você.");
                else if (momento == "convivio")
                    falas.Add("Ainda estamos nos adaptando uns aos outros.");
                else if (momento == "conflito")
                    falas.Add("Estou tendo um dia complicado.");
                else if (momento == "reacao")
                    falas.Add("Foi uma situação interessante.");
                else if (momento == "pressao")
                    falas.Add("Precisamos lidar com essa situação.");
                else if (momento == "decisaoFinal")
                    falas.Add("Chegou a hora de decidir.");
                break;
        }

        return falas;
    }

    void MostrarNoAtual()
    {
        if (indiceNoAtual < 0 || indiceNoAtual >= nos.Count)
        {
            MostrarFinal();
            return;
        }

        NoDialogoVN noAtual = nos[indiceNoAtual];

        textoNomePersonagem.text = noAtual.personagemFalando != null ? noAtual.personagemFalando.nomePersonagem : "";
        textoFala.text = EscolherTextoAleatorio(noAtual.falasVariaveis);
        textoRespostaJogador.text = EscolherTextoAleatorio(noAtual.respostasJogadorVariaveis);

        controladorCena.AtualizarPersonagem(controladorCena.imagemEsquerda, noAtual.personagemEsquerda, noAtual.emocaoEsquerda, noAtual.mostrarEsquerda);
        controladorCena.AtualizarPersonagem(controladorCena.imagemCentro, noAtual.personagemCentro, noAtual.emocaoCentro, noAtual.mostrarCentro);
        controladorCena.AtualizarPersonagem(controladorCena.imagemDireita, noAtual.personagemDireita, noAtual.emocaoDireita, noAtual.mostrarDireita);
        controladorCena.AtualizarJogador(aparenciaAtualJogador, emocaoAtualJogador);

        if (noAtual.tipoNo == TipoNoDialogo.DialogoSimples)
        {
            painelEscolhas.SetActive(false);
            botaoContinuar.gameObject.SetActive(true);
        }
        else
        {
            painelEscolhas.SetActive(true);
            botaoContinuar.gameObject.SetActive(false);

            ConfigurarBotaoEscolha(botaoEscolha1, textoEscolha1, noAtual.opcoes, 0);
            ConfigurarBotaoEscolha(botaoEscolha2, textoEscolha2, noAtual.opcoes, 1);
            ConfigurarBotaoEscolha(botaoEscolha3, textoEscolha3, noAtual.opcoes, 2);
        }
    }

    void ConfigurarBotaoEscolha(Button botao, TMP_Text texto, List<OpcaoEscolha> opcoes, int indice)
    {
        if (indice >= opcoes.Count)
        {
            botao.gameObject.SetActive(false);
            return;
        }

        botao.gameObject.SetActive(true);
        texto.text = opcoes[indice].textoOpcao;
        botao.onClick.RemoveAllListeners();
        botao.onClick.AddListener(() => EscolherOpcao(opcoes[indice]));
    }

    void ContinuarDialogoSimples()
    {
        NoDialogoVN noAtual = nos[indiceNoAtual];
        indiceNoAtual = noAtual.proximoNoSimples;
        MostrarNoAtual();
    }

    void EscolherOpcao(OpcaoEscolha opcao)
    {
        empatia += opcao.deltaEmpatia;
        comunicacao += opcao.deltaComunicacao;
        controleEmocional += opcao.deltaControleEmocional;
        lideranca += opcao.deltaLideranca;

        emocaoAtualJogador = opcao.emocaoJogadorAposEscolha;
        textoRespostaJogador.text = opcao.respostaJogador;

        if (opcao.proximoNo == -1)
        {
            MostrarFinal();
            return;
        }

        indiceNoAtual = opcao.proximoNo;
        MostrarNoAtual();
    }

    void MostrarFinal()
    {
        painelDialogo.SetActive(false);
        painelEscolhas.SetActive(false);
        painelFinal.SetActive(true);

        textoFinal.text =
            "Resultado Final\n\n" +
            "Jogador: " + nomeJogador + "\n" +
            "Gênero: " + generoJogador + "\n" +
            "Ambiente: " + ambienteAtual + "\n\n" +
            "Empatia: " + empatia + "\n" +
            "Comunicação: " + comunicacao + "\n" +
            "Controle Emocional: " + controleEmocional + "\n" +
            "Liderança: " + lideranca + "\n\n" +
            "Perfil: " + GerarPerfil() + "\n\n" +
            "Áreas indicadas:\n" + GerarAreas();
    }

    string GerarPerfil()
    {
        if (empatia >= 4 && comunicacao >= 4)
            return "Perfil colaborativo e comunicativo.";

        if (lideranca >= 4 && controleEmocional >= 3)
            return "Perfil de liderança e tomada de decisão.";

        if (empatia < 0 || controleEmocional < 0)
            return "Perfil com dificuldade maior em lidar com conflitos emocionais.";

        return "Perfil equilibrado com pontos a desenvolver.";
    }

    string GerarAreas()
    {
        List<string> areas = new List<string>();

        if (empatia >= 4)
            areas.Add("- Psicologia, Recursos Humanos, Assistência Social");

        if (comunicacao >= 4)
            areas.Add("- Comunicação, Ensino, Atendimento, Marketing");

        if (lideranca >= 4)
            areas.Add("- Administração, Gestão, Coordenação");

        if (controleEmocional >= 4)
            areas.Add("- Mediação, Gestão de Crises, Liderança sob pressão");

        if (areas.Count == 0)
            areas.Add("- Desenvolvimento geral de competências socioemocionais");

        return string.Join("\n", areas);
    }

    string EscolherTextoAleatorio(List<string> lista)
    {
        if (lista == null || lista.Count == 0)
            return "";

        return lista[Random.Range(0, lista.Count)];
    }

    void ReiniciarJogo()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}