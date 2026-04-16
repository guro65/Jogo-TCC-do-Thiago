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

    [Header("Áudio")]
    public AudioSource fonteAudio;
    public AudioClip musicaInicio;
    public AudioClip musicaFaculdade;
    public AudioClip musicaTrabalho;
    public AudioClip musicaGrupoAmigos;

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

    private string ultimaRespostaJogador = "";
    private string ultimaReacaoNPC = "";
    private bool mostrarRespostaEscolhida = false;

    void Start()
    {
        AtivarSomentePainel(painelInicio);

        if (painelDadosIniciais != null) painelDadosIniciais.SetActive(true);
        if (painelEscolhaAmbiente != null) painelEscolhaAmbiente.SetActive(false);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (painelDialogo != null) painelDialogo.SetActive(false);
        if (painelFinal != null) painelFinal.SetActive(false);
        if (fundo != null) fundo.SetActive(false);

        if (botaoComecar != null) botaoComecar.onClick.AddListener(PrepararJogador);
        if (botaoFaculdade != null) botaoFaculdade.onClick.AddListener(() => SelecionarAmbiente(TipoAmbiente.Faculdade));
        if (botaoTrabalho != null) botaoTrabalho.onClick.AddListener(() => SelecionarAmbiente(TipoAmbiente.Trabalho));
        if (botaoGrupoAmigos != null) botaoGrupoAmigos.onClick.AddListener(() => SelecionarAmbiente(TipoAmbiente.GrupoDeAmigos));
        if (botaoContinuar != null) botaoContinuar.onClick.AddListener(ContinuarDialogoSimples);
        if (botaoReiniciar != null) botaoReiniciar.onClick.AddListener(ReiniciarJogo);

        TocarMusicaInicio();
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
        TocarMusicaAmbiente();

        if (fundo != null) fundo.SetActive(true);

        if (painelDadosIniciais != null) painelDadosIniciais.SetActive(false);
        if (painelEscolhaAmbiente != null) painelEscolhaAmbiente.SetActive(false);
        if (painelInicio != null) painelInicio.SetActive(false);

        if (painelFinal != null) painelFinal.SetActive(false);
        if (painelDialogo != null) painelDialogo.SetActive(true);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);

        if (textoAmbiente != null)
            textoAmbiente.text = "Ambiente: " + NomeAmbiente(ambienteAtual);

        MontarRoteiroBaseDoAmbiente();
        indiceNoAtual = 0;
        mostrarRespostaEscolhida = false;
        ultimaRespostaJogador = "";
        ultimaReacaoNPC = "";

        MostrarNoAtual();
    }

    void TocarMusica(AudioClip musica)
    {
        if (fonteAudio == null || musica == null)
            return;

        if (fonteAudio.clip == musica && fonteAudio.isPlaying)
            return;

        fonteAudio.Stop();
        fonteAudio.clip = musica;
        fonteAudio.Play();
    }

    void TocarMusicaInicio()
    {
        TocarMusica(musicaInicio);
    }

    void TocarMusicaAmbiente()
    {
        switch (ambienteAtual)
        {
            case TipoAmbiente.Faculdade:
                TocarMusica(musicaFaculdade);
                break;
            case TipoAmbiente.Trabalho:
                TocarMusica(musicaTrabalho);
                break;
            case TipoAmbiente.GrupoDeAmigos:
                TocarMusica(musicaGrupoAmigos);
                break;
        }
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
            default:
                return new List<DadosPersonagem>();
        }
    }

    string NomeAmbiente(TipoAmbiente ambiente)
    {
        switch (ambiente)
        {
            case TipoAmbiente.Faculdade:
                return "Faculdade";
            case TipoAmbiente.Trabalho:
                return "Trabalho";
            case TipoAmbiente.GrupoDeAmigos:
                return "Grupo de Amigos";
            default:
                return "Nenhum";
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
            falasVariaveis = new List<string>
            {
                "Oi, você deve ser a pessoa nova daqui, né? Prazer em te conhecer.",
                "Então você é quem acabou de chegar. Espero que consiga se sentir à vontade por aqui.",
                "Prazer, eu sou " + p1.nomePersonagem + ". Se precisar de ajuda no começo, pode falar comigo."
            },
            respostasJogadorVariaveis = new List<string>
            {
                "Oi, prazer em conhecer vocês. Ainda estou me acostumando com tudo.",
                "Olá, estou começando agora, então talvez eu demore um pouco para pegar o ritmo.",
                "Prazer. Espero conseguir me adaptar bem aqui."
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
            falasVariaveis = new List<string>
            {
                "Olha... já vou avisando que hoje não está sendo um dia muito bom pra mim.",
                "Se eu parecer meio irritado, não leva pro lado pessoal. O dia já começou complicado.",
                "Não estou no meu melhor humor hoje, então talvez eu acabe sendo mais seco do que o normal."
            },
            opcoes = new List<OpcaoEscolha>
            {
                new OpcaoEscolha
                {
                    textoOpcao = "Perguntar com calma o motivo",
                    respostaJogador = "Tudo bem... aconteceu alguma coisa? Se quiser falar, eu posso ouvir.",
                    tomResposta = TomResposta.Boa,
                    reacaoNPC = "Foi mal. Eu acabei descontando em você sem querer. É que realmente já aconteceram algumas coisas chatas hoje.",
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
                    respostaJogador = "Tudo bem. Vou deixar isso passar por enquanto.",
                    tomResposta = TomResposta.Neutra,
                    reacaoNPC = "Certo... obrigado por não transformar isso em um problema maior.",
                    deltaControleEmocional = 1,
                    deltaComunicacao = -1,
                    emocaoJogadorAposEscolha = Emocao.Neutro,
                    emocaoPersonagemAposEscolha = Emocao.Neutro,
                    proximoNo = 2
                },
                new OpcaoEscolha
                {
                    textoOpcao = "Responder de forma agressiva",
                    respostaJogador = "Se você está irritado, isso não te dá direito de falar assim comigo.",
                    tomResposta = TomResposta.Rude,
                    reacaoNPC = "Eu sei que fui grosso, mas você também não precisava responder desse jeito.",
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
            falasVariaveis = new List<string>
            {
                "Bom, pelo menos vocês conseguiram conversar sem deixar a situação pior do que já estava.",
                "Esses primeiros contatos dizem muito sobre como a convivência vai ser daqui pra frente.",
                "Dá pra perceber bastante coisa pela forma como alguém reage num momento tenso."
            },
            respostasJogadorVariaveis = new List<string>
            {
                "É... eu ainda estou entendendo como lidar com cada pessoa aqui.",
                "Acho que já deu para perceber que cada um reage de um jeito diferente.",
                "Essa conversa me fez prestar mais atenção na forma como eu respondo."
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
            falasVariaveis = new List<string>
            {
                "Temos um problema: uma parte importante do que precisava ser feito ainda não ficou pronta e o tempo está acabando.",
                "A situação apertou. O prazo está muito perto e ainda falta uma parte essencial para finalizar tudo.",
                "Se a gente não se organizar agora, isso pode virar um problema bem maior em pouco tempo."
            },
            opcoes = new List<OpcaoEscolha>
            {
                new OpcaoEscolha
                {
                    textoOpcao = "Organizar todos para resolver",
                    respostaJogador = "Em vez de ficar apontando culpa agora, acho melhor a gente dividir o que falta e tentar resolver juntos.",
                    tomResposta = TomResposta.Boa,
                    reacaoNPC = "Essa foi uma boa ideia. Se cada um assumir uma parte, ainda dá pra consertar a situação.",
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
                    respostaJogador = "Eu vou garantir pelo menos a minha parte primeiro. Depois vejo no que mais consigo ajudar.",
                    tomResposta = TomResposta.Neutra,
                    reacaoNPC = "Faz sentido querer garantir o que está com você, mas talvez a gente precise pensar mais como grupo agora.",
                    deltaControleEmocional = 1,
                    emocaoJogadorAposEscolha = Emocao.Neutro,
                    emocaoPersonagemAposEscolha = Emocao.Neutro,
                    proximoNo = 4
                },
                new OpcaoEscolha
                {
                    textoOpcao = "Culpar alguém pela falha",
                    respostaJogador = "Se estamos nessa situação, é porque alguém não fez a própria parte direito.",
                    tomResposta = TomResposta.Rude,
                    reacaoNPC = "Ficar culpando alguém agora só vai aumentar a tensão. Isso não resolve o que ainda precisa ser feito.",
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
            falasVariaveis = new List<string>
            {
                "No fim das contas, precisamos decidir qual postura vamos tomar diante do erro que aconteceu.",
                "Agora não tem mais como adiar: alguém vai precisar se posicionar sobre o que deu errado.",
                "A forma como isso for resolvido agora vai dizer muito sobre responsabilidade e trabalho em equipe."
            },
            opcoes = new List<OpcaoEscolha>
            {
                new OpcaoEscolha
                {
                    textoOpcao = "Assumir responsabilidade",
                    respostaJogador = "Eu reconheço minha parte nisso e quero ajudar a corrigir o que for necessário.",
                    tomResposta = TomResposta.Boa,
                    reacaoNPC = "Assumir a responsabilidade desse jeito mostra maturidade. Isso ajuda muito mais do que tentar escapar do problema.",
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
                    respostaJogador = "Prefiro focar em resolver o problema agora e depois a gente vê com calma o que aconteceu.",
                    tomResposta = TomResposta.Neutra,
                    reacaoNPC = "Não é a resposta mais direta, mas pelo menos você ainda está tentando contribuir para a solução.",
                    deltaControleEmocional = 1,
                    deltaComunicacao = 1,
                    emocaoJogadorAposEscolha = Emocao.Neutro,
                    emocaoPersonagemAposEscolha = Emocao.Neutro,
                    proximoNo = -1
                },
                new OpcaoEscolha
                {
                    textoOpcao = "Jogar a culpa em outra pessoa",
                    respostaJogador = "Eu não tenho culpa disso. O problema começou por causa do erro de outra pessoa.",
                    tomResposta = TomResposta.Rude,
                    reacaoNPC = "Tentar jogar a culpa em alguém agora só piora a confiança entre todo mundo. Isso dificilmente ajuda a resolver de verdade.",
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
        List<string> falas = new List<string>();

        switch (ambienteAtual)
        {
            case TipoAmbiente.Faculdade:
                falas = FalasFaculdade(personagem, momento);
                break;

            case TipoAmbiente.Trabalho:
                falas = FalasTrabalho(personagem, momento);
                break;

            case TipoAmbiente.GrupoDeAmigos:
                falas = FalasGrupoAmigos(personagem, momento);
                break;
        }

        if (falas == null || falas.Count == 0)
            falas.Add("...");

        return falas;
    }

    List<string> FalasFaculdade(DadosPersonagem personagem, string momento)
    {
        List<string> falas = new List<string>();

        switch (personagem.personalidade)
        {
            case PersonalidadePersonagem.Gentil:
                if (momento == "introducao")
                {
                    falas.Add("Oi, você é novo por aqui, né? Prazer. Se quiser, depois eu posso te mostrar onde ficam algumas coisas.");
                    falas.Add("Você acabou de chegar? Relaxa, no começo é normal se sentir meio perdido.");
                }
                else if (momento == "conflitoLeve")
                {
                    falas.Add("Foi mal... eu estou meio cansado hoje. Tive aula demais e ainda estou tentando resolver umas coisas.");
                    falas.Add("Desculpa se eu pareci grosseiro. O dia na faculdade já começou meio complicado pra mim.");
                }
                else if (momento == "reacao")
                {
                    falas.Add("Você lidou bem com isso. Nem todo mundo consegue manter a calma nessas horas.");
                    falas.Add("Dá pra perceber que você pensa antes de responder, isso ajuda bastante.");
                }
                else if (momento == "pressao")
                {
                    falas.Add("A apresentação está perto e ainda tem parte do trabalho sem terminar. A gente precisa se organizar.");
                    falas.Add("Se ninguém alinhar isso agora, esse trabalho em grupo vai virar uma bagunça.");
                }
                else if (momento == "decisaoFinal")
                {
                    falas.Add("Agora não adianta fugir, a gente precisa decidir como vai resolver isso com o professor.");
                    falas.Add("O que a gente fizer agora pode mudar totalmente a forma como esse grupo vai ser visto.");
                }
                break;

            case PersonalidadePersonagem.Irritado:
                if (momento == "introducao")
                {
                    falas.Add("Você é a pessoa nova da turma? Tá... depois você vai entender como as coisas funcionam aqui.");
                    falas.Add("Certo, você acabou de chegar. Só tenta não deixar tudo mais confuso do que já é.");
                }
                else if (momento == "conflitoLeve")
                {
                    falas.Add("Hoje já foi puxado demais e eu ainda tenho que lidar com trabalho, aula e prazo ao mesmo tempo.");
                    falas.Add("Não estou com muita paciência agora. A semana já está pesada demais.");
                }
                else if (momento == "reacao")
                {
                    falas.Add("Bom... pelo menos você não respondeu pior do que eu esperava.");
                    falas.Add("Você lidou melhor com isso do que muita gente da turma lidaria.");
                }
                else if (momento == "pressao")
                {
                    falas.Add("O prazo está em cima e ainda tem gente que não fez a parte do trabalho.");
                    falas.Add("Se continuar desse jeito, a apresentação vai dar errado e vai sobrar pra todo mundo.");
                }
                else if (momento == "decisaoFinal")
                {
                    falas.Add("Agora alguém vai ter que assumir o que aconteceu nesse trabalho.");
                    falas.Add("Não dá mais pra fingir que esse problema vai se resolver sozinho.");
                }
                break;

            case PersonalidadePersonagem.Calmo:
                if (momento == "introducao")
                {
                    falas.Add("Oi. Vai com calma, no começo sempre parece muita informação ao mesmo tempo.");
                    falas.Add("Você está começando agora? Relaxa, com o tempo você pega o jeito da turma.");
                }
                else if (momento == "conflitoLeve")
                {
                    falas.Add("Foi mal, eu estou um pouco sobrecarregado com as coisas da faculdade hoje.");
                    falas.Add("Talvez eu tenha falado de um jeito pior do que devia. O dia já estava meio pesado.");
                }
                else if (momento == "reacao")
                {
                    falas.Add("Você conseguiu responder de um jeito equilibrado, isso ajuda bastante.");
                    falas.Add("Foi uma boa forma de lidar com a situação sem deixar o clima pior.");
                }
                else if (momento == "pressao")
                {
                    falas.Add("Ainda dá tempo de organizar o trabalho, mas todo mundo precisa colaborar agora.");
                    falas.Add("Se a gente alinhar as tarefas direito, ainda consegue apresentar bem.");
                }
                else if (momento == "decisaoFinal")
                {
                    falas.Add("Agora vale pensar com cuidado antes de decidir o que fazer.");
                    falas.Add("Esse é o tipo de momento que mostra maturidade no grupo.");
                }
                break;

            default:
                if (momento == "introducao")
                    falas.Add("Oi, prazer em te conhecer. Espero que você consiga se adaptar à turma.");
                else if (momento == "conflitoLeve")
                    falas.Add("Hoje a rotina da faculdade já me deixou bem cansado.");
                else if (momento == "reacao")
                    falas.Add("Essa conversa já mostrou bastante sobre como você reage.");
                else if (momento == "pressao")
                    falas.Add("O trabalho da faculdade precisa ser resolvido logo.");
                else if (momento == "decisaoFinal")
                    falas.Add("A decisão de agora vai afetar o grupo todo.");
                break;
        }

        return falas;
    }

    List<string> FalasTrabalho(DadosPersonagem personagem, string momento)
    {
        List<string> falas = new List<string>();

        switch (personagem.personalidade)
        {
            case PersonalidadePersonagem.Gentil:
                if (momento == "introducao")
                {
                    falas.Add("Oi, seja bem-vindo à equipe. No começo sempre aparece muita coisa nova, mas você vai pegando o ritmo.");
                    falas.Add("Prazer, qualquer dúvida pode me chamar. É melhor perguntar do que ficar perdido no começo.");
                }
                else if (momento == "conflitoLeve")
                {
                    falas.Add("Desculpa, eu estou num dia meio ruim. O volume de trabalho hoje ficou bem acima do normal.");
                    falas.Add("Foi mal se eu pareci seco. Estou tentando resolver várias pendências ao mesmo tempo.");
                }
                else if (momento == "reacao")
                {
                    falas.Add("Você respondeu bem. No trabalho, saber manter a postura faz bastante diferença.");
                    falas.Add("Dá pra ver que você tenta lidar com as pessoas sem piorar a situação.");
                }
                else if (momento == "pressao")
                {
                    falas.Add("Tem uma entrega importante chegando e ainda falta alinhar bastante coisa da equipe.");
                    falas.Add("Se a comunicação continuar falhando, esse projeto pode atrasar de vez.");
                }
                else if (momento == "decisaoFinal")
                {
                    falas.Add("Agora a gente precisa decidir como vai lidar com esse erro no projeto.");
                    falas.Add("Essa situação vai exigir responsabilidade de verdade de todo mundo.");
                }
                break;

            case PersonalidadePersonagem.Irritado:
                if (momento == "introducao")
                {
                    falas.Add("Então você é quem entrou agora? Certo. Espero que consiga acompanhar o ritmo daqui.");
                    falas.Add("Você é novo na equipe? Beleza. Só presta atenção porque aqui as coisas andam rápido.");
                }
                else if (momento == "conflitoLeve")
                {
                    falas.Add("Hoje o trabalho já começou cheio de problema e eu realmente estou sem paciência.");
                    falas.Add("Eu já estou lidando com pressão demais hoje, então não estou no melhor humor.");
                }
                else if (momento == "reacao")
                {
                    falas.Add("Bom... pelo menos você não levou a conversa para um lado ainda pior.");
                    falas.Add("Você respondeu melhor do que eu achei que responderia.");
                }
                else if (momento == "pressao")
                {
                    falas.Add("O prazo está muito em cima e ninguém parece alinhado sobre o que ainda falta.");
                    falas.Add("Se continuar assim, a entrega vai sair errada ou nem vai sair.");
                }
                else if (momento == "decisaoFinal")
                {
                    falas.Add("Agora alguém vai ter que responder por esse erro.");
                    falas.Add("Não dá mais para empurrar isso para depois.");
                }
                break;

            case PersonalidadePersonagem.Calmo:
                if (momento == "introducao")
                {
                    falas.Add("Seja bem-vindo. No início é melhor observar bastante e ir entendendo o fluxo da equipe.");
                    falas.Add("Prazer. Vai com calma que, com o tempo, você entende melhor o ritmo do trabalho.");
                }
                else if (momento == "conflitoLeve")
                {
                    falas.Add("Talvez eu tenha falado de forma mais seca agora há pouco. O dia está bem puxado.");
                    falas.Add("Desculpa, estou tentando resolver muita coisa ao mesmo tempo e acabei ficando mais tenso.");
                }
                else if (momento == "reacao")
                {
                    falas.Add("Foi uma resposta equilibrada. Isso conta bastante num ambiente profissional.");
                    falas.Add("Você conseguiu manter a postura, e isso ajuda muito em situações tensas.");
                }
                else if (momento == "pressao")
                {
                    falas.Add("Se a equipe se organizar agora, ainda dá para evitar um problema maior.");
                    falas.Add("O prazo está apertado, mas ainda existe espaço para corrigir a rota.");
                }
                else if (momento == "decisaoFinal")
                {
                    falas.Add("Esse é o momento de decidir com responsabilidade.");
                    falas.Add("A forma de agir agora vai dizer muito sobre a equipe.");
                }
                break;

            default:
                if (momento == "introducao")
                    falas.Add("Prazer em conhecer você. Seja bem-vindo ao ambiente de trabalho.");
                else if (momento == "conflitoLeve")
                    falas.Add("O dia no trabalho está mais complicado do que o normal.");
                else if (momento == "reacao")
                    falas.Add("Sua postura já mostra muito sobre como você lida com pressão.");
                else if (momento == "pressao")
                    falas.Add("O projeto precisa de organização urgente.");
                else if (momento == "decisaoFinal")
                    falas.Add("A forma de resolver isso importa bastante.");
                break;
        }

        return falas;
    }

    List<string> FalasGrupoAmigos(DadosPersonagem personagem, string momento)
    {
        List<string> falas = new List<string>();

        switch (personagem.personalidade)
        {
            case PersonalidadePersonagem.Gentil:
                if (momento == "introducao")
                {
                    falas.Add("Oi, prazer. Se você vai ficar com a gente, pode relaxar mais.");
                    falas.Add("Que bom te conhecer. Aos poucos você vai se enturmando com o pessoal.");
                }
                else if (momento == "conflitoLeve")
                {
                    falas.Add("Foi mal... eu não estou num dia muito bom e acabei falando de um jeito ruim.");
                    falas.Add("Desculpa se eu pareci estranho. Hoje eu já estava meio estressado antes mesmo de chegar aqui.");
                }
                else if (momento == "reacao")
                {
                    falas.Add("Você soube lidar bem com isso. Nem todo mundo teria essa paciência.");
                    falas.Add("Dá para ver que você tenta manter a amizade sem transformar tudo em briga.");
                }
                else if (momento == "pressao")
                {
                    falas.Add("Se a gente não se entender agora, isso vai virar discussão entre todo mundo.");
                    falas.Add("Todo mundo está falando ao mesmo tempo e ninguém está realmente ouvindo.");
                }
                else if (momento == "decisaoFinal")
                {
                    falas.Add("Agora a gente precisa decidir se vai resolver isso juntos ou deixar a situação piorar.");
                    falas.Add("Dependendo do que acontecer agora, o clima entre o grupo pode mudar bastante.");
                }
                break;

            case PersonalidadePersonagem.Irritado:
                if (momento == "introducao")
                {
                    falas.Add("Então você vai andar com a gente? Tá, vamos ver quanto tempo leva para você se acostumar.");
                    falas.Add("Certo, você chegou agora. Depois você vai entender o jeito do pessoal.");
                }
                else if (momento == "conflitoLeve")
                {
                    falas.Add("Hoje eu já estava sem paciência antes mesmo de encontrar todo mundo.");
                    falas.Add("Não estou no clima para conversa leve agora, então talvez eu fale de um jeito pior.");
                }
                else if (momento == "reacao")
                {
                    falas.Add("Bom... pelo menos você não explodiu também.");
                    falas.Add("Você segurou melhor a situação do que eu imaginei.");
                }
                else if (momento == "pressao")
                {
                    falas.Add("Se ninguém parar para ouvir o outro, isso aqui vai virar briga de verdade.");
                    falas.Add("Do jeito que está, qualquer coisa pode acabar virando discussão.");
                }
                else if (momento == "decisaoFinal")
                {
                    falas.Add("Agora ou a gente resolve isso logo ou cada um vai acabar saindo irritado daqui.");
                    falas.Add("Se continuar desse jeito, o grupo vai rachar por besteira.");
                }
                break;

            case PersonalidadePersonagem.Calmo:
                if (momento == "introducao")
                {
                    falas.Add("Prazer. Vai com calma que você vai se entrosando aos poucos com o grupo.");
                    falas.Add("Relaxa, no começo é normal ainda estar entendendo o jeito de cada um.");
                }
                else if (momento == "conflitoLeve")
                {
                    falas.Add("Talvez eu tenha falado de um jeito ruim. Hoje eu já estava meio tenso antes de vir.");
                    falas.Add("Foi mal, eu não estava muito bem e acabei deixando isso aparecer na conversa.");
                }
                else if (momento == "reacao")
                {
                    falas.Add("Você conseguiu segurar bem a situação sem transformar tudo em discussão.");
                    falas.Add("Foi uma boa forma de responder sem deixar o clima pior.");
                }
                else if (momento == "pressao")
                {
                    falas.Add("Se todo mundo diminuir o tom agora, ainda dá para conversar direito.");
                    falas.Add("O grupo ainda consegue se entender, mas alguém precisa puxar a conversa do jeito certo.");
                }
                else if (momento == "decisaoFinal")
                {
                    falas.Add("Agora é o momento de decidir se vocês vão resolver isso ou continuar alimentando a briga.");
                    falas.Add("Essa escolha pode mudar bastante o clima entre todo mundo.");
                }
                break;

            default:
                if (momento == "introducao")
                    falas.Add("Oi, prazer em te conhecer. Logo você pega intimidade com o grupo.");
                else if (momento == "conflitoLeve")
                    falas.Add("O clima hoje não está dos melhores.");
                else if (momento == "reacao")
                    falas.Add("Essa conversa já mostrou bastante sobre como você reage.");
                else if (momento == "pressao")
                    falas.Add("O grupo precisa se entender antes que isso vire briga.");
                else if (momento == "decisaoFinal")
                    falas.Add("Essa decisão vai mudar bastante o clima entre vocês.");
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

        string falaNPC;
        string falaJogador;

        if (mostrarRespostaEscolhida)
        {
            falaJogador = ultimaRespostaJogador;
            falaNPC = ultimaReacaoNPC;
            mostrarRespostaEscolhida = false;
        }
        else
        {
            falaNPC = EscolherTextoAleatorio(noAtual.falasVariaveis);
            falaJogador = EscolherTextoAleatorio(noAtual.respostasJogadorVariaveis);
        }

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

        ultimaRespostaJogador = opcao.respostaJogador;
        ultimaReacaoNPC = opcao.reacaoNPC;
        mostrarRespostaEscolhida = true;

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

        if (empatia >= 2)
            areas.Add("- Psicologia, Recursos Humanos, Assistência Social");

        if (comunicacao >= 2)
            areas.Add("- Comunicação, Ensino, Atendimento, Marketing");

        if (lideranca >= 2)
            areas.Add("- Administração, Gestão, Coordenação");

        if (controleEmocional >= 2)
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