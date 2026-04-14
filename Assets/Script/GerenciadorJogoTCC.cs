using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GerenciadorJogoTCC : MonoBehaviour
{
    [Header("Etapas iniciais")]
    public GameObject painelInicio;
    public GameObject painelDadosIniciais;
    public GameObject painelEscolhaAmbiente;

    [Header("Painéis principais")]
    public GameObject painelDialogo;
    public GameObject painelEscolhas;
    public GameObject painelFinal;

    [Header("Fundo")]
    public GameObject fundo;

    [Header("Tela inicial")]
    public TMP_InputField campoNome;
    public TMP_Dropdown dropdownGenero;
    public Button botaoComecar;
    public Button botaoFaculdade;
    public Button botaoTrabalho;
    public Button botaoGrupoAmigos;

    [Header("Diálogo")]
    public TMP_Text textoAmbiente;

    public GameObject caixaNomeNPC;
    public TMP_Text textoNomeNPC;
    public TMP_Text textoFalaNPC;

    public GameObject caixaNomeJogador;
    public TMP_Text textoNomeJogador;
    public TMP_Text textoFalaJogador;

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

    [Header("Visual da cena")]
    public ControladorCenaVN controladorCena;
    public Image imagemFundo;
    public Sprite fundoFaculdade;
    public Sprite fundoTrabalho;
    public Sprite fundoGrupoAmigos;

    [Header("Aparências do jogador")]
    public List<AparenciaJogador> aparenciasMasculinas = new();
    public List<AparenciaJogador> aparenciasFemininas = new();
    public List<AparenciaJogador> aparenciasNaoDefinidas = new();

    [Header("Personagens por ambiente")]
    public List<DadosPersonagem> personagensFaculdade = new();
    public List<DadosPersonagem> personagensTrabalho = new();
    public List<DadosPersonagem> personagensGrupoAmigos = new();

    private string nomeJogador = "Jogador";
    private GeneroJogador generoJogador = GeneroJogador.Nada;
    private TipoAmbiente ambienteAtual = TipoAmbiente.Nenhum;
    private AparenciaJogador aparenciaAtualJogador;
    private Emocao emocaoAtualJogador = Emocao.Neutro;

    private int empatia;
    private int comunicacao;
    private int controleEmocional;
    private int lideranca;

    private List<DadosPersonagem> personagensAtivos = new();
    private List<NoDialogoVN> nos = new();
    private int indiceNoAtual;

    void Start()
    {
        AtivarSomentePainel(painelInicio);

        if (painelDadosIniciais != null) painelDadosIniciais.SetActive(true);
        if (painelEscolhaAmbiente != null) painelEscolhaAmbiente.SetActive(false);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);

        if (botaoComecar != null) botaoComecar.onClick.AddListener(PrepararJogador);
        if (botaoFaculdade != null) botaoFaculdade.onClick.AddListener(() => SelecionarAmbiente(TipoAmbiente.Faculdade));
        if (botaoTrabalho != null) botaoTrabalho.onClick.AddListener(() => SelecionarAmbiente(TipoAmbiente.Trabalho));
        if (botaoGrupoAmigos != null) botaoGrupoAmigos.onClick.AddListener(() => SelecionarAmbiente(TipoAmbiente.GrupoDeAmigos));
        if (botaoContinuar != null) botaoContinuar.onClick.AddListener(ContinuarDialogoSimples);
        if (botaoReiniciar != null) botaoReiniciar.onClick.AddListener(ReiniciarJogo);
    }

    void AtivarSomentePainel(GameObject painelPrincipal)
    {
        if (painelInicio != null) painelInicio.SetActive(painelPrincipal == painelInicio);
        if (painelDialogo != null) painelDialogo.SetActive(painelPrincipal == painelDialogo);
        if (painelFinal != null) painelFinal.SetActive(painelPrincipal == painelFinal);

        if (painelEscolhas != null && painelPrincipal != painelDialogo)
            painelEscolhas.SetActive(false);
    }

    void PrepararJogador()
    {
        nomeJogador = campoNome != null ? campoNome.text.Trim() : "";

        if (string.IsNullOrWhiteSpace(nomeJogador))
        {
            Debug.LogWarning("Digite um nome antes de continuar.");
            return;
        }

        if (dropdownGenero == null)
        {
            Debug.LogError("Dropdown de gênero não foi ligado no Inspector.");
            return;
        }

        generoJogador = (GeneroJogador)dropdownGenero.value;

        if (generoJogador == GeneroJogador.Nada)
        {
            Debug.LogWarning("Escolha um gênero ou a opção 'Não definido' para continuar.");
            return;
        }

        aparenciaAtualJogador = SortearAparenciaJogador(generoJogador);
        emocaoAtualJogador = Emocao.Neutro;

        if (painelDadosIniciais != null) painelDadosIniciais.SetActive(false);
        if (painelEscolhaAmbiente != null) painelEscolhaAmbiente.SetActive(true);
    }

    AparenciaJogador SortearAparenciaJogador(GeneroJogador genero)
    {
        List<AparenciaJogador> lista = null;

        switch (genero)
        {
            case GeneroJogador.Masculino:
                lista = aparenciasMasculinas;
                break;
            case GeneroJogador.Feminino:
                lista = aparenciasFemininas;
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
        personagensAtivos = ObterListaDoAmbiente(ambienteAtual);

        if (personagensAtivos == null || personagensAtivos.Count < 6)
        {
            Debug.LogError("O ambiente escolhido precisa ter 6 personagens configurados.");
            return;
        }

        AtualizarFundoPorAmbiente();

        if (fundo != null) fundo.SetActive(true);
        if (painelDadosIniciais != null) painelDadosIniciais.SetActive(false);
        if (painelEscolhaAmbiente != null) painelEscolhaAmbiente.SetActive(false);
        if (painelInicio != null) painelInicio.SetActive(false);
        if (painelDialogo != null) painelDialogo.SetActive(true);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (painelFinal != null) painelFinal.SetActive(false);

        if (textoAmbiente != null)
            textoAmbiente.text = "Ambiente: " + NomeAmbiente(ambienteAtual);

        MontarRoteiroBaseDoAmbiente();
        indiceNoAtual = 0;

        MostrarNoAtual();
    }

    List<DadosPersonagem> ObterListaDoAmbiente(TipoAmbiente ambiente)
    {
        switch (ambiente)
        {
            case TipoAmbiente.Faculdade: return personagensFaculdade;
            case TipoAmbiente.Trabalho: return personagensTrabalho;
            case TipoAmbiente.GrupoDeAmigos: return personagensGrupoAmigos;
            default: return new List<DadosPersonagem>();
        }
    }

    string NomeAmbiente(TipoAmbiente ambiente)
    {
        switch (ambiente)
        {
            case TipoAmbiente.Faculdade: return "Faculdade";
            case TipoAmbiente.Trabalho: return "Trabalho";
            case TipoAmbiente.GrupoDeAmigos: return "Grupo de Amigos";
            default: return "Nenhum";
        }
    }

    void AtualizarFundoPorAmbiente()
    {
        if (imagemFundo == null) return;

        switch (ambienteAtual)
        {
            case TipoAmbiente.Faculdade:
                imagemFundo.sprite = fundoFaculdade;
                break;
            case TipoAmbiente.Trabalho:
                imagemFundo.sprite = fundoTrabalho;
                break;
            case TipoAmbiente.GrupoDeAmigos:
                imagemFundo.sprite = fundoGrupoAmigos;
                break;
        }
    }

    void MontarRoteiroBaseDoAmbiente()
    {
        nos.Clear();

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
            personagemEsquerda = p1,
            personagemCentro = p2,
            personagemDireita = p3,
            emocaoEsquerda = Emocao.Neutro,
            emocaoCentro = Emocao.Feliz,
            emocaoDireita = Emocao.Neutro,
            falasVariaveis = FalasPorPersonalidade(p1, "introducao"),
            respostasJogadorVariaveis = new List<string>
            {
                "Oi, prazer em conhecer vocês.",
                "Olá, estou começando agora.",
                "Espero me adaptar bem."
            },
            proximoNoSimples = 1
        });

        nos.Add(new NoDialogoVN
        {
            id = 1,
            tipoNo = TipoNoDialogo.Escolha,
            personagemFalando = p2,
            personagemEsquerda = p1,
            personagemCentro = p2,
            personagemDireita = p3,
            emocaoEsquerda = Emocao.Neutro,
            emocaoCentro = Emocao.Raiva,
            emocaoDireita = Emocao.Neutro,
            falasVariaveis = FalasPorPersonalidade(p2, "conflitoLeve"),
            opcoes = new List<OpcaoEscolha>
            {
                new OpcaoEscolha
                {
                    textoOpcao = "Perguntar com calma o motivo",
                    respostaJogador = "Calma, aconteceu alguma coisa?",
                    deltaEmpatia = 2,
                    deltaComunicacao = 2,
                    deltaControleEmocional = 1,
                    emocaoJogadorAposEscolha = Emocao.Neutro,
                    emocaoPersonagemAposEscolha = Emocao.Neutro,
                    proximoNo = 2
                },
                new OpcaoEscolha
                {
                    textoOpcao = "Ignorar a provocação",
                    respostaJogador = "Tudo bem, vou deixar isso passar.",
                    deltaControleEmocional = 1,
                    deltaComunicacao = -1,
                    emocaoJogadorAposEscolha = Emocao.Neutro,
                    emocaoPersonagemAposEscolha = Emocao.Neutro,
                    proximoNo = 2
                },
                new OpcaoEscolha
                {
                    textoOpcao = "Responder de forma agressiva",
                    respostaJogador = "Você não precisa falar assim comigo.",
                    deltaEmpatia = -2,
                    deltaControleEmocional = -2,
                    emocaoJogadorAposEscolha = Emocao.Raiva,
                    emocaoPersonagemAposEscolha = Emocao.Raiva,
                    proximoNo = 2
                }
            }
        });

        nos.Add(new NoDialogoVN
        {
            id = 2,
            tipoNo = TipoNoDialogo.DialogoSimples,
            personagemFalando = p3,
            personagemEsquerda = p1,
            personagemCentro = p2,
            personagemDireita = p3,
            emocaoEsquerda = Emocao.Neutro,
            emocaoCentro = Emocao.Neutro,
            emocaoDireita = Emocao.Feliz,
            falasVariaveis = FalasPorPersonalidade(p3, "reacao"),
            respostasJogadorVariaveis = new List<string>
            {
                "Entendi melhor a situação agora.",
                "Vou pensar nisso.",
                "Essa conversa foi mais importante do que parecia."
            },
            proximoNoSimples = 3
        });

        nos.Add(new NoDialogoVN
        {
            id = 3,
            tipoNo = TipoNoDialogo.Escolha,
            personagemFalando = p4,
            personagemEsquerda = p4,
            personagemCentro = p5,
            personagemDireita = p6,
            emocaoEsquerda = Emocao.Neutro,
            emocaoCentro = Emocao.Raiva,
            emocaoDireita = Emocao.Neutro,
            falasVariaveis = FalasPorPersonalidade(p4, "pressao"),
            opcoes = new List<OpcaoEscolha>
            {
                new OpcaoEscolha
                {
                    textoOpcao = "Organizar todos para resolver",
                    respostaJogador = "Vamos nos organizar e dividir as responsabilidades.",
                    deltaLideranca = 2,
                    deltaComunicacao = 2,
                    deltaEmpatia = 1,
                    emocaoJogadorAposEscolha = Emocao.Neutro,
                    emocaoPersonagemAposEscolha = Emocao.Neutro,
                    proximoNo = 4
                },
                new OpcaoEscolha
                {
                    textoOpcao = "Focar só na sua parte",
                    respostaJogador = "Vou resolver o que está comigo primeiro.",
                    deltaControleEmocional = 1,
                    emocaoJogadorAposEscolha = Emocao.Neutro,
                    emocaoPersonagemAposEscolha = Emocao.Neutro,
                    proximoNo = 4
                },
                new OpcaoEscolha
                {
                    textoOpcao = "Culpar alguém pela falha",
                    respostaJogador = "Isso aconteceu porque alguém errou.",
                    deltaEmpatia = -2,
                    deltaLideranca = -1,
                    emocaoJogadorAposEscolha = Emocao.Raiva,
                    emocaoPersonagemAposEscolha = Emocao.Raiva,
                    proximoNo = 4
                }
            }
        });

        nos.Add(new NoDialogoVN
        {
            id = 4,
            tipoNo = TipoNoDialogo.Escolha,
            personagemFalando = p6,
            personagemEsquerda = p4,
            personagemCentro = p5,
            personagemDireita = p6,
            emocaoEsquerda = Emocao.Neutro,
            emocaoCentro = Emocao.Neutro,
            emocaoDireita = Emocao.Neutro,
            falasVariaveis = FalasPorPersonalidade(p6, "decisaoFinal"),
            opcoes = new List<OpcaoEscolha>
            {
                new OpcaoEscolha
                {
                    textoOpcao = "Assumir responsabilidade",
                    respostaJogador = "Eu assumo minha parte e quero ajudar a resolver.",
                    deltaEmpatia = 1,
                    deltaComunicacao = 2,
                    deltaLideranca = 2,
                    emocaoJogadorAposEscolha = Emocao.Feliz,
                    emocaoPersonagemAposEscolha = Emocao.Feliz,
                    proximoNo = -1
                },
                new OpcaoEscolha
                {
                    textoOpcao = "Tentar resolver sem se expor muito",
                    respostaJogador = "Vamos corrigir isso com calma.",
                    deltaControleEmocional = 1,
                    deltaComunicacao = 1,
                    emocaoJogadorAposEscolha = Emocao.Neutro,
                    emocaoPersonagemAposEscolha = Emocao.Neutro,
                    proximoNo = -1
                },
                new OpcaoEscolha
                {
                    textoOpcao = "Jogar a culpa em outra pessoa",
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

    List<string> FalasPorPersonalidade(DadosPersonagem personagem, string momento)
    {
        List<string> falas = new();

        switch (personagem.personalidade)
        {
            case PersonalidadePersonagem.Gentil:
                if (momento == "introducao")
                {
                    falas.Add("Oi, seja bem-vindo. Espero que você se sinta confortável aqui.");
                    falas.Add("Que bom te conhecer. Pode contar comigo.");
                }
                else if (momento == "conflitoLeve")
                {
                    falas.Add("Desculpa, estou um pouco sobrecarregado hoje.");
                    falas.Add("Não queria falar desse jeito com você.");
                }
                else if (momento == "reacao")
                {
                    falas.Add("Sua resposta mostrou maturidade.");
                    falas.Add("Foi bom ver que você tentou agir bem.");
                }
                else if (momento == "pressao")
                {
                    falas.Add("O prazo está apertado, então precisamos agir juntos.");
                    falas.Add("Se nos organizarmos, ainda dá tempo.");
                }
                else if (momento == "decisaoFinal")
                {
                    falas.Add("Agora precisamos decidir como enfrentar esse problema.");
                    falas.Add("Esse momento mostra nossa responsabilidade.");
                }
                break;

            case PersonalidadePersonagem.Irritado:
                if (momento == "introducao")
                {
                    falas.Add("Tá, então você é a pessoa nova.");
                    falas.Add("Certo. Só tenta não atrapalhar.");
                }
                else if (momento == "conflitoLeve")
                {
                    falas.Add("Hoje já deu tudo errado e eu não estou com paciência.");
                    falas.Add("Já estou cheio de problema.");
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
                if (momento == "introducao") falas.Add("Olá, prazer em conhecer você.");
                else if (momento == "conflitoLeve") falas.Add("Estou tendo um dia complicado.");
                else if (momento == "reacao") falas.Add("Foi uma situação importante.");
                else if (momento == "pressao") falas.Add("Precisamos lidar com essa situação.");
                else if (momento == "decisaoFinal") falas.Add("Chegou a hora de decidir.");
                break;
        }

        if (falas.Count == 0)
            falas.Add("...");

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

        string falaNPC = EscolherTextoAleatorio(noAtual.falasVariaveis);
        string falaJogador = EscolherTextoAleatorio(noAtual.respostasJogadorVariaveis);

        bool npcTemFala = !string.IsNullOrWhiteSpace(falaNPC);
        bool jogadorTemFala = !string.IsNullOrWhiteSpace(falaJogador);

        if (caixaNomeNPC != null) caixaNomeNPC.SetActive(npcTemFala);
        if (textoFalaNPC != null) textoFalaNPC.gameObject.SetActive(npcTemFala);

        if (caixaNomeJogador != null) caixaNomeJogador.SetActive(jogadorTemFala);
        if (textoFalaJogador != null) textoFalaJogador.gameObject.SetActive(jogadorTemFala);

        if (npcTemFala)
        {
            if (textoNomeNPC != null)
                textoNomeNPC.text = noAtual.personagemFalando != null ? noAtual.personagemFalando.nomePersonagem : "NPC";

            if (textoFalaNPC != null)
                textoFalaNPC.text = falaNPC;
        }

        if (jogadorTemFala)
        {
            if (textoNomeJogador != null)
                textoNomeJogador.text = nomeJogador;

            if (textoFalaJogador != null)
                textoFalaJogador.text = falaJogador;
        }

        if (controladorCena != null)
        {
            controladorCena.AtualizarPersonagem(controladorCena.imagemEsquerda, noAtual.personagemEsquerda, noAtual.emocaoEsquerda, noAtual.mostrarEsquerda);
            controladorCena.AtualizarPersonagem(controladorCena.imagemCentro, noAtual.personagemCentro, noAtual.emocaoCentro, noAtual.mostrarCentro);
            controladorCena.AtualizarPersonagem(controladorCena.imagemDireita, noAtual.personagemDireita, noAtual.emocaoDireita, noAtual.mostrarDireita);
            controladorCena.AtualizarJogador(aparenciaAtualJogador, emocaoAtualJogador);
            controladorCena.DestacarFalante(noAtual.personagemFalando, noAtual.personagemEsquerda, noAtual.personagemCentro, noAtual.personagemDireita);
        }

        if (noAtual.tipoNo == TipoNoDialogo.DialogoSimples)
        {
            if (painelEscolhas != null) painelEscolhas.SetActive(false);
            if (botaoContinuar != null) botaoContinuar.gameObject.SetActive(true);
        }
        else
        {
            if (painelEscolhas != null) painelEscolhas.SetActive(true);
            if (botaoContinuar != null) botaoContinuar.gameObject.SetActive(false);

            ConfigurarBotaoEscolha(botaoEscolha1, textoEscolha1, noAtual.opcoes, 0);
            ConfigurarBotaoEscolha(botaoEscolha2, textoEscolha2, noAtual.opcoes, 1);
            ConfigurarBotaoEscolha(botaoEscolha3, textoEscolha3, noAtual.opcoes, 2);
        }
    }

    void ConfigurarBotaoEscolha(Button botao, TMP_Text texto, List<OpcaoEscolha> opcoes, int indice)
    {
        if (botao == null || texto == null)
            return;

        if (opcoes == null || indice >= opcoes.Count)
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
        if (indiceNoAtual < 0 || indiceNoAtual >= nos.Count)
            return;

        indiceNoAtual = nos[indiceNoAtual].proximoNoSimples;
        MostrarNoAtual();
    }

    void EscolherOpcao(OpcaoEscolha opcao)
    {
        empatia += opcao.deltaEmpatia;
        comunicacao += opcao.deltaComunicacao;
        controleEmocional += opcao.deltaControleEmocional;
        lideranca += opcao.deltaLideranca;

        emocaoAtualJogador = opcao.emocaoJogadorAposEscolha;

        if (caixaNomeJogador != null) caixaNomeJogador.SetActive(true);
        if (textoNomeJogador != null) textoNomeJogador.text = nomeJogador;
        if (textoFalaJogador != null) textoFalaJogador.text = opcao.respostaJogador;
        if (textoFalaJogador != null) textoFalaJogador.gameObject.SetActive(true);

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
        AtivarSomentePainel(painelFinal);

        if (textoFinal == null)
            return;

        textoFinal.text =
            "Resultado Final\n\n" +
            "Jogador: " + nomeJogador + "\n" +
            "Gênero: " + generoJogador + "\n" +
            "Ambiente: " + NomeAmbiente(ambienteAtual) + "\n\n" +
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
            return "Perfil de liderança e boa tomada de decisão.";

        if (empatia < 0 || controleEmocional < 0)
            return "Perfil com maior dificuldade para lidar com conflitos emocionais.";

        return "Perfil equilibrado, com pontos importantes para desenvolver.";
    }

    string GerarAreas()
    {
        List<string> areas = new();

        if (empatia >= 4)
            areas.Add("- Psicologia, Recursos Humanos, Assistência Social");

        if (comunicacao >= 4)
            areas.Add("- Comunicação, Ensino, Atendimento, Marketing");

        if (lideranca >= 4)
            areas.Add("- Administração, Gestão, Coordenação");

        if (controleEmocional >= 4)
            areas.Add("- Mediação, Gestão de Crises, Liderança sob pressão");

        if (areas.Count == 0)
            areas.Add("- Desenvolvimento geral das competências socioemocionais");

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
        if (fundo != null) fundo.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}