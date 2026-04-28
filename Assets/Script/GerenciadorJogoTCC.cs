using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GerenciadorJogoTCC : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject painelInicio;
    public GameObject painelDadosIniciais;
    public GameObject painelTopo;
    public GameObject painelDialogo;
    public GameObject painelEscolhas;
    public GameObject painelResultadoFase;
    public GameObject painelFinal;

    [Header("Áudio")]
    public AudioSource fonteAudio;
    public AudioClip musicaInicio;
    public AudioClip musicaFaseFacil;
    public AudioClip musicaFaseMedia;
    public AudioClip musicaFaseDificil;

    [Header("Fundo")]
    public GameObject fundo;
    public Image imagemFundo;
    public Sprite fundoTrabalhoTI;

    [Header("Tela inicial")]
    public TMP_InputField campoNome;
    public TMP_Dropdown dropdownGenero;
    public Button botaoComecar;

    [Header("Topo")]
    public TMP_Text textoFase;

    [Header("Medidor de aprovação")]
    public Slider medidorAprovacao;
    public TMP_Text textoMedidorAprovacao;

    [Header("Diálogo")]
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

    [Header("Resultado da fase")]
    public TMP_Text textoResultadoFase;
    public Button botaoContinuarFase;

    [Header("Final")]
    public TMP_Text textoFinal;
    public Button botaoReiniciar;

    [Header("Visual da cena")]
    public ControladorCenaVN controladorCena;

    [Header("Aparências do jogador")]
    public List<AparenciaJogador> aparenciasMasculinas = new List<AparenciaJogador>();
    public List<AparenciaJogador> aparenciasFemininas = new List<AparenciaJogador>();
    public List<AparenciaJogador> aparenciasNaoDefinidas = new List<AparenciaJogador>();

    [Header("3 NPCs da Fase Fácil / Júnior")]
    public List<DadosPersonagem> personagensJunior = new List<DadosPersonagem>();

    [Header("3 NPCs da Fase Média / Pleno")]
    public List<DadosPersonagem> personagensPleno = new List<DadosPersonagem>();

    [Header("3 NPCs da Fase Difícil / Sênior")]
    public List<DadosPersonagem> personagensSenior = new List<DadosPersonagem>();

    [Header("Efeito de digitação")]
    public float velocidadeDigitacao = 0.025f;

    private string nomeJogador = "Jogador";
    private GeneroJogador generoJogador = GeneroJogador.Nada;
    private AparenciaJogador aparenciaAtualJogador;
    private Emocao emocaoAtualJogador = Emocao.Neutro;
    private Emocao ultimaEmocaoPersonagem = Emocao.Neutro;

    private FaseProfissional faseAtual = FaseProfissional.FacilJunior;
    private FaseProfissional proximaFaseDepoisResultado;

    private int comunicacao;
    private int trabalhoEquipe;
    private int resolucaoProblemas;
    private int adaptabilidade;
    private int empatia;

    private int pontosFaseAtual;
    private int pontosMaximosFase;
    private float porcentagemFase;

    private List<NoDialogoVN> nos = new List<NoDialogoVN>();
    private int indiceNoAtual;

    private string ultimaRespostaJogador = "";
    private string ultimaReacaoNPC = "";
    private bool exibindoReacaoEscolha;
    private bool aguardandoResultadoFase;
    private int proximoNoAposReacao;
    private bool finalizarDepoisResultado;

    private bool textoDigitando;
    private Coroutine rotinaDigitacao;
    private string textoCompletoNPC = "";
    private string textoCompletoJogador = "";

    private const int TOTAL_PERGUNTAS_POR_FASE = 24;

    private class QuestaoTI
    {
        public CategoriaSoftSkill categoria;

        public DadosPersonagem npc;
        public DadosPersonagem esquerda;
        public DadosPersonagem centro;
        public DadosPersonagem direita;

        public Emocao emocaoNPC;
        public Emocao emocaoJogadorAoOuvir;

        public string falaNPC;

        public string botaoBom;
        public string respostaBoa;
        public string reacaoBoa;

        public string botaoMedio;
        public string respostaMedia;
        public string reacaoMedia;

        public string botaoRuim;
        public string respostaRuim;
        public string reacaoRuim;
    }

    void Start()
    {
        AtivarEstadoInicial();

        if (botaoComecar != null) botaoComecar.onClick.AddListener(PrepararJogador);
        if (botaoContinuar != null) botaoContinuar.onClick.AddListener(ContinuarDialogo);
        if (botaoContinuarFase != null) botaoContinuarFase.onClick.AddListener(ContinuarDepoisResultadoFase);
        if (botaoReiniciar != null) botaoReiniciar.onClick.AddListener(ReiniciarJogo);

        TocarMusica(musicaInicio);
    }

    void AtivarEstadoInicial()
    {
        if (painelInicio != null) painelInicio.SetActive(true);
        if (painelDadosIniciais != null) painelDadosIniciais.SetActive(true);
        if (painelTopo != null) painelTopo.SetActive(false);
        if (painelDialogo != null) painelDialogo.SetActive(false);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (painelResultadoFase != null) painelResultadoFase.SetActive(false);
        if (painelFinal != null) painelFinal.SetActive(false);
        if (fundo != null) fundo.SetActive(false);

        if (controladorCena != null)
            controladorCena.EsconderTodos();

        AtualizarMedidor();
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
            Debug.LogWarning("Escolha um gênero ou selecione Não definido.");
            return;
        }

        aparenciaAtualJogador = SortearAparenciaJogador(generoJogador);
        emocaoAtualJogador = Emocao.Neutro;

        if (!ListasDeNPCsValidas())
            return;

        if (painelInicio != null) painelInicio.SetActive(false);
        if (painelDadosIniciais != null) painelDadosIniciais.SetActive(false);

        faseAtual = FaseProfissional.FacilJunior;
        IniciarFase(faseAtual);
    }

    bool ListasDeNPCsValidas()
    {
        if (personagensJunior == null || personagensJunior.Count < 3)
        {
            Debug.LogError("A fase Júnior precisa ter 3 NPCs.");
            return false;
        }

        if (personagensPleno == null || personagensPleno.Count < 3)
        {
            Debug.LogError("A fase Pleno precisa ter 3 NPCs.");
            return false;
        }

        if (personagensSenior == null || personagensSenior.Count < 3)
        {
            Debug.LogError("A fase Sênior precisa ter 3 NPCs.");
            return false;
        }

        return true;
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

    void IniciarFase(FaseProfissional fase)
    {
        faseAtual = fase;

        pontosFaseAtual = 0;
        pontosMaximosFase = TOTAL_PERGUNTAS_POR_FASE * 2;
        porcentagemFase = 0;

        ultimaRespostaJogador = "";
        ultimaReacaoNPC = "";
        exibindoReacaoEscolha = false;
        aguardandoResultadoFase = false;
        proximoNoAposReacao = -1;
        finalizarDepoisResultado = false;
        ultimaEmocaoPersonagem = Emocao.Neutro;
        emocaoAtualJogador = Emocao.Neutro;

        if (fundo != null) fundo.SetActive(true);
        if (imagemFundo != null && fundoTrabalhoTI != null) imagemFundo.sprite = fundoTrabalhoTI;

        if (painelTopo != null) painelTopo.SetActive(true);
        if (painelDialogo != null) painelDialogo.SetActive(true);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (painelResultadoFase != null) painelResultadoFase.SetActive(false);
        if (painelFinal != null) painelFinal.SetActive(false);

        AtualizarTextoFase();
        AtualizarMedidor();
        TocarMusicaDaFase();

        MontarRoteiroDaFase();

        indiceNoAtual = 0;
        MostrarNoAtual();
    }

    void AtualizarTextoFase()
    {
        if (textoFase == null)
            return;

        textoFase.text = NomeFase(faseAtual);
    }

    string NomeFase(FaseProfissional fase)
    {
        switch (fase)
        {
            case FaseProfissional.FacilJunior:
                return "1ª Fase - Fácil (Júnior de TI)";

            case FaseProfissional.MedioPleno:
                return "2ª Fase - Média (Pleno de TI)";

            case FaseProfissional.DificilSenior:
                return "3ª Fase - Difícil (Sênior de TI)";

            default:
                return "";
        }
    }

    float PorcentagemNecessaria(FaseProfissional fase)
    {
        switch (fase)
        {
            case FaseProfissional.FacilJunior:
                return 50f;

            case FaseProfissional.MedioPleno:
                return 60f;

            case FaseProfissional.DificilSenior:
                return 70f;

            default:
                return 0f;
        }
    }

    void AtualizarMedidor()
    {
        float valor = 0f;

        if (pontosMaximosFase > 0)
            valor = (float)pontosFaseAtual / pontosMaximosFase;

        if (medidorAprovacao != null)
            medidorAprovacao.value = valor;

        if (textoMedidorAprovacao != null)
            textoMedidorAprovacao.text = "Aprovação: " + Mathf.RoundToInt(valor * 100f) + "% / Necessário: " + PorcentagemNecessaria(faseAtual).ToString("F0") + "%";
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

    void TocarMusicaDaFase()
    {
        switch (faseAtual)
        {
            case FaseProfissional.FacilJunior:
                TocarMusica(musicaFaseFacil);
                break;

            case FaseProfissional.MedioPleno:
                TocarMusica(musicaFaseMedia);
                break;

            case FaseProfissional.DificilSenior:
                TocarMusica(musicaFaseDificil);
                break;
        }
    }

    void MontarRoteiroDaFase()
    {
        nos.Clear();

        List<QuestaoTI> questoes = new List<QuestaoTI>();

        switch (faseAtual)
        {
            case FaseProfissional.FacilJunior:
                questoes = CriarQuestoesJunior();
                break;

            case FaseProfissional.MedioPleno:
                questoes = CriarQuestoesPleno();
                break;

            case FaseProfissional.DificilSenior:
                questoes = CriarQuestoesSenior();
                break;
        }

        for (int i = 0; i < questoes.Count; i++)
        {
            QuestaoTI q = questoes[i];

            nos.Add(new NoDialogoVN
            {
                id = i,
                tipoNo = TipoNoDialogo.Escolha,

                personagemFalando = q.npc,

                personagemEsquerda = q.esquerda,
                personagemCentro = q.centro,
                personagemDireita = q.direita,

                emocaoEsquerda = q.npc == q.esquerda ? q.emocaoNPC : Emocao.Neutro,
                emocaoCentro = q.npc == q.centro ? q.emocaoNPC : Emocao.Neutro,
                emocaoDireita = q.npc == q.direita ? q.emocaoNPC : Emocao.Neutro,

                emocaoJogadorDuranteNo = q.emocaoJogadorAoOuvir,

                falasVariaveis = new List<string> { q.falaNPC },

                opcoes = new List<OpcaoEscolha>
                {
                    CriarOpcaoBoa(q.categoria, q.botaoBom, q.respostaBoa, q.reacaoBoa, i + 1),
                    CriarOpcaoNeutra(q.categoria, q.botaoMedio, q.respostaMedia, q.reacaoMedia, i + 1),
                    CriarOpcaoRuim(q.categoria, q.botaoRuim, q.respostaRuim, q.reacaoRuim, i + 1)
                }
            });

            if (i == questoes.Count - 1)
            {
                foreach (OpcaoEscolha opcao in nos[i].opcoes)
                    opcao.proximoNo = -1;
            }
        }
    }

    QuestaoTI Q(
        CategoriaSoftSkill categoria,
        DadosPersonagem npc,
        DadosPersonagem esquerda,
        DadosPersonagem centro,
        DadosPersonagem direita,
        Emocao emocaoNPC,
        Emocao emocaoJogadorAoOuvir,
        string falaNPC,
        string botaoBom,
        string respostaBoa,
        string reacaoBoa,
        string botaoMedio,
        string respostaMedia,
        string reacaoMedia,
        string botaoRuim,
        string respostaRuim,
        string reacaoRuim)
    {
        return new QuestaoTI
        {
            categoria = categoria,
            npc = npc,
            esquerda = esquerda,
            centro = centro,
            direita = direita,
            emocaoNPC = emocaoNPC,
            emocaoJogadorAoOuvir = emocaoJogadorAoOuvir,
            falaNPC = falaNPC,
            botaoBom = botaoBom,
            respostaBoa = respostaBoa,
            reacaoBoa = reacaoBoa,
            botaoMedio = botaoMedio,
            respostaMedia = respostaMedia,
            reacaoMedia = reacaoMedia,
            botaoRuim = botaoRuim,
            respostaRuim = respostaRuim,
            reacaoRuim = reacaoRuim
        };
    }

    List<QuestaoTI> CriarQuestoesJunior()
    {
        return CriarQuestoesPorModelo(
            FaseProfissional.FacilJunior,
            personagensJunior,
            "uma task no Jira com descrição incompleta",
            "um pull request com comentários de revisão",
            "um bug simples encontrado pelo QA",
            "uma mudança pequena de requisito no meio da sprint"
        );
    }

    List<QuestaoTI> CriarQuestoesPleno()
    {
        return CriarQuestoesPorModelo(
            FaseProfissional.MedioPleno,
            personagensPleno,
            "uma sprint atrasada por falta de alinhamento",
            "um conflito entre desenvolvedor e QA",
            "uma prioridade técnica competindo com demanda urgente do produto",
            "uma refatoração necessária em código legado"
        );
    }

    List<QuestaoTI> CriarQuestoesSenior()
    {
        return CriarQuestoesPorModelo(
            FaseProfissional.DificilSenior,
            personagensSenior,
            "um incidente crítico em produção",
            "uma war room com cliente impactado",
            "uma decisão de arquitetura com risco técnico",
            "um conflito entre pessoas experientes da equipe"
        );
    }

    List<QuestaoTI> CriarQuestoesPorModelo(FaseProfissional fase, List<DadosPersonagem> personagens, string tema1, string tema2, string tema3, string tema4)
    {
        List<QuestaoTI> questoes = new List<QuestaoTI>();

        CategoriaSoftSkill[] categorias =
        {
            CategoriaSoftSkill.Comunicacao,
            CategoriaSoftSkill.TrabalhoEquipe,
            CategoriaSoftSkill.ResolucaoProblemas,
            CategoriaSoftSkill.Adaptabilidade,
            CategoriaSoftSkill.Empatia
        };

        for (int i = 0; i < TOTAL_PERGUNTAS_POR_FASE; i++)
        {
            CategoriaSoftSkill categoria = categorias[i % categorias.Length];

            DadosPersonagem npc = personagens[i % 3];
            DadosPersonagem esquerda = personagens[0];
            DadosPersonagem centro = personagens[1];
            DadosPersonagem direita = personagens[2];

            string tema = ObterTema(i, tema1, tema2, tema3, tema4);

            questoes.Add(Q(
                categoria,
                npc,
                esquerda,
                centro,
                direita,
                EscolherEmocaoNPCDaPergunta(fase, categoria),
                EscolherEmocaoJogadorAoOuvir(fase, categoria),
                CriarFalaNPC(fase, categoria, npc, tema, i),
                CriarTextoBotaoBom(categoria),
                CriarRespostaBoa(fase, categoria, tema),
                CriarReacaoBoa(fase, categoria),
                CriarTextoBotaoMedio(categoria),
                CriarRespostaMedia(fase, categoria, tema),
                CriarReacaoMedia(fase, categoria),
                CriarTextoBotaoRuim(categoria),
                CriarRespostaRuim(fase, categoria, tema),
                CriarReacaoRuim(fase, categoria)
            ));
        }

        return questoes;
    }

    string ObterTema(int indice, string tema1, string tema2, string tema3, string tema4)
    {
        switch (indice % 4)
        {
            case 0:
                return tema1;
            case 1:
                return tema2;
            case 2:
                return tema3;
            default:
                return tema4;
        }
    }

    Emocao EscolherEmocaoNPCDaPergunta(FaseProfissional fase, CategoriaSoftSkill categoria)
    {
        if (fase == FaseProfissional.DificilSenior)
        {
            if (categoria == CategoriaSoftSkill.TrabalhoEquipe || categoria == CategoriaSoftSkill.Empatia)
                return Emocao.Raiva;

            return Emocao.Neutro;
        }

        if (fase == FaseProfissional.MedioPleno)
        {
            if (categoria == CategoriaSoftSkill.Empatia || categoria == CategoriaSoftSkill.TrabalhoEquipe)
                return Emocao.Raiva;

            return Emocao.Neutro;
        }

        if (categoria == CategoriaSoftSkill.Empatia)
            return Emocao.Raiva;

        return Emocao.Neutro;
    }

    Emocao EscolherEmocaoJogadorAoOuvir(FaseProfissional fase, CategoriaSoftSkill categoria)
    {
        if (fase == FaseProfissional.DificilSenior)
            return Emocao.Neutro;

        if (categoria == CategoriaSoftSkill.Empatia)
            return Emocao.Neutro;

        return Emocao.Neutro;
    }

    string CriarFalaNPC(FaseProfissional fase, CategoriaSoftSkill categoria, DadosPersonagem npc, string tema, int indice)
    {
        string cargo = string.IsNullOrWhiteSpace(npc.cargoOuFuncao) ? "profissional de TI" : npc.cargoOuFuncao;

        if (fase == FaseProfissional.FacilJunior)
        {
            switch (categoria)
            {
                case CategoriaSoftSkill.Comunicacao:
                    return npc.nomePersonagem + " (" + cargo + "): Você está lidando com " + tema + ". Como júnior, o mais importante é comunicar dúvida antes que ela vire retrabalho.";

                case CategoriaSoftSkill.TrabalhoEquipe:
                    return npc.nomePersonagem + " (" + cargo + "): Essa situação de " + tema + " afeta outras pessoas do time. Como você pretende colaborar sem depender totalmente dos outros?";

                case CategoriaSoftSkill.ResolucaoProblemas:
                    return npc.nomePersonagem + " (" + cargo + "): Encontramos um problema ligado a " + tema + ". Antes de pedir solução pronta, quero ver como você investiga.";

                case CategoriaSoftSkill.Adaptabilidade:
                    return npc.nomePersonagem + " (" + cargo + "): A prioridade mudou por causa de " + tema + ". Quero ver como você se adapta sem travar.";

                case CategoriaSoftSkill.Empatia:
                    return npc.nomePersonagem + " (" + cargo + "): Alguém do time está pressionado por causa de " + tema + ". Sua resposta pode melhorar ou piorar o clima.";
            }
        }

        if (fase == FaseProfissional.MedioPleno)
        {
            switch (categoria)
            {
                case CategoriaSoftSkill.Comunicacao:
                    return npc.nomePersonagem + " (" + cargo + "): Como pleno, você precisa alinhar " + tema + " com devs, QA e produto sem deixar ruído virar atraso.";

                case CategoriaSoftSkill.TrabalhoEquipe:
                    return npc.nomePersonagem + " (" + cargo + "): O time está dividido por causa de " + tema + ". Esperamos que você ajude a destravar a colaboração.";

                case CategoriaSoftSkill.ResolucaoProblemas:
                    return npc.nomePersonagem + " (" + cargo + "): Existe um problema técnico envolvendo " + tema + ". A solução precisa ser prática, mas não pode virar gambiarra.";

                case CategoriaSoftSkill.Adaptabilidade:
                    return npc.nomePersonagem + " (" + cargo + "): O planejamento mudou por causa de " + tema + ". Um pleno precisa se reorganizar sem perder qualidade.";

                case CategoriaSoftSkill.Empatia:
                    return npc.nomePersonagem + " (" + cargo + "): Uma pessoa do time está sobrecarregada por causa de " + tema + " e começou a cometer erros.";
            }
        }

        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                return npc.nomePersonagem + " (" + cargo + "): Em uma situação sênior envolvendo " + tema + ", você precisa comunicar riscos para equipe, liderança e cliente.";

            case CategoriaSoftSkill.TrabalhoEquipe:
                return npc.nomePersonagem + " (" + cargo + "): A equipe está sob pressão por causa de " + tema + ". As pessoas estão se atacando em vez de resolver.";

            case CategoriaSoftSkill.ResolucaoProblemas:
                return npc.nomePersonagem + " (" + cargo + "): Precisamos decidir uma solução técnica para " + tema + " sem comprometer estabilidade e manutenção.";

            case CategoriaSoftSkill.Adaptabilidade:
                return npc.nomePersonagem + " (" + cargo + "): A direção do projeto mudou por causa de " + tema + ". O time espera uma decisão rápida e madura.";

            case CategoriaSoftSkill.Empatia:
                return npc.nomePersonagem + " (" + cargo + "): Depois de " + tema + ", parte da equipe está insegura e com medo de assumir erros.";
        }

        return npc.nomePersonagem + ": Temos uma situação importante para resolver.";
    }

    string CriarTextoBotaoBom(CategoriaSoftSkill categoria)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                return "Comunicar com clareza";
            case CategoriaSoftSkill.TrabalhoEquipe:
                return "Colaborar com o time";
            case CategoriaSoftSkill.ResolucaoProblemas:
                return "Investigar com método";
            case CategoriaSoftSkill.Adaptabilidade:
                return "Adaptar o plano";
            case CategoriaSoftSkill.Empatia:
                return "Responder com empatia";
            default:
                return "Boa resposta";
        }
    }

    string CriarTextoBotaoMedio(CategoriaSoftSkill categoria)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                return "Responder parcialmente";
            case CategoriaSoftSkill.TrabalhoEquipe:
                return "Ajudar só o necessário";
            case CategoriaSoftSkill.ResolucaoProblemas:
                return "Resolver no improviso";
            case CategoriaSoftSkill.Adaptabilidade:
                return "Aceitar com resistência";
            case CategoriaSoftSkill.Empatia:
                return "Manter distância";
            default:
                return "Resposta mediana";
        }
    }

    string CriarTextoBotaoRuim(CategoriaSoftSkill categoria)
    {
        switch (categoria)
        {
            case CategoriaSoftSkill.Comunicacao:
                return "Esconder informação";
            case CategoriaSoftSkill.TrabalhoEquipe:
                return "Culpar o time";
            case CategoriaSoftSkill.ResolucaoProblemas:
                return "Chutar solução";
            case CategoriaSoftSkill.Adaptabilidade:
                return "Reclamar da mudança";
            case CategoriaSoftSkill.Empatia:
                return "Responder sem consideração";
            default:
                return "Resposta ruim";
        }
    }

    string CriarRespostaBoa(FaseProfissional fase, CategoriaSoftSkill categoria, string tema)
    {
        if (fase == FaseProfissional.DificilSenior)
            return "Vou organizar a comunicação, assumir a responsabilidade técnica e conduzir o time para uma solução segura sobre " + tema + ".";

        if (fase == FaseProfissional.MedioPleno)
            return "Vou alinhar com o time, entender o impacto de " + tema + " e propor um caminho claro sem expor ninguém.";

        return "Vou perguntar com clareza, entender melhor " + tema + " e agir sem esconder minhas dúvidas.";
    }

    string CriarRespostaMedia(FaseProfissional fase, CategoriaSoftSkill categoria, string tema)
    {
        if (fase == FaseProfissional.DificilSenior)
            return "Vou resolver tecnicamente primeiro e depois comunico o que for necessário sobre " + tema + ".";

        if (fase == FaseProfissional.MedioPleno)
            return "Vou tentar resolver a parte urgente de " + tema + " e depois vejo como alinhar o restante com o time.";

        return "Vou tentar resolver " + tema + " sozinho primeiro. Se não der, eu peço ajuda.";
    }

    string CriarRespostaRuim(FaseProfissional fase, CategoriaSoftSkill categoria, string tema)
    {
        if (fase == FaseProfissional.DificilSenior)
            return "Para evitar desgaste, é melhor corrigir " + tema + " em silêncio e não chamar atenção para o problema.";

        if (fase == FaseProfissional.MedioPleno)
            return "Isso aconteceu porque alguém não fez a própria parte direito. Primeiro precisamos apontar quem errou em " + tema + ".";

        return "Acho que " + tema + " não é responsabilidade minha. Vou seguir do jeito que der.";
    }

    string CriarReacaoBoa(FaseProfissional fase, CategoriaSoftSkill categoria)
    {
        if (fase == FaseProfissional.DificilSenior)
            return "Essa é uma postura sênior: técnica, ética, comunicação e liderança ao mesmo tempo.";

        if (fase == FaseProfissional.MedioPleno)
            return "Boa postura de pleno. Você pensou em entrega, pessoas e clareza.";

        return "Boa postura de júnior. Você demonstrou vontade de aprender e evitar retrabalho.";
    }

    string CriarReacaoMedia(FaseProfissional fase, CategoriaSoftSkill categoria)
    {
        if (fase == FaseProfissional.DificilSenior)
            return "Pode funcionar no curto prazo, mas um sênior precisa pensar também em confiança, processo e impacto.";

        if (fase == FaseProfissional.MedioPleno)
            return "Resolve parte do problema, mas ainda falta visão de equipe e prevenção.";

        return "Você tentou resolver, mas ainda precisa melhorar comunicação e organização.";
    }

    string CriarReacaoRuim(FaseProfissional fase, CategoriaSoftSkill categoria)
    {
        if (fase == FaseProfissional.DificilSenior)
            return "Essa postura coloca a confiança em risco. Em nível sênior, esconder ou culpar pode causar danos maiores que o erro técnico.";

        if (fase == FaseProfissional.MedioPleno)
            return "Essa resposta aumenta atrito e reduz colaboração. Como pleno, você precisa ajudar a unir o time.";

        return "Essa resposta pode gerar retrabalho, afastar o time e dificultar seu crescimento profissional.";
    }

    OpcaoEscolha CriarOpcaoBoa(CategoriaSoftSkill categoria, string textoBotao, string falaJogador, string reacaoNPC, int proximoNo)
    {
        return new OpcaoEscolha
        {
            textoOpcao = textoBotao,
            respostaJogador = falaJogador,
            tomResposta = TomResposta.Boa,
            categoria = categoria,
            reacaoNPC = reacaoNPC,
            pontosAprovacao = 2,

            deltaComunicacao = categoria == CategoriaSoftSkill.Comunicacao ? 2 : 1,
            deltaTrabalhoEquipe = categoria == CategoriaSoftSkill.TrabalhoEquipe ? 2 : 1,
            deltaResolucaoProblemas = categoria == CategoriaSoftSkill.ResolucaoProblemas ? 2 : 1,
            deltaAdaptabilidade = categoria == CategoriaSoftSkill.Adaptabilidade ? 2 : 1,
            deltaEmpatia = categoria == CategoriaSoftSkill.Empatia ? 2 : 1,

            emocaoJogadorAposEscolha = Emocao.Feliz,
            emocaoPersonagemAposEscolha = Emocao.Feliz,

            proximoNo = proximoNo
        };
    }

    OpcaoEscolha CriarOpcaoNeutra(CategoriaSoftSkill categoria, string textoBotao, string falaJogador, string reacaoNPC, int proximoNo)
    {
        return new OpcaoEscolha
        {
            textoOpcao = textoBotao,
            respostaJogador = falaJogador,
            tomResposta = TomResposta.Neutra,
            categoria = categoria,
            reacaoNPC = reacaoNPC,
            pontosAprovacao = 1,

            deltaComunicacao = categoria == CategoriaSoftSkill.Comunicacao ? 1 : 0,
            deltaTrabalhoEquipe = categoria == CategoriaSoftSkill.TrabalhoEquipe ? 1 : 0,
            deltaResolucaoProblemas = categoria == CategoriaSoftSkill.ResolucaoProblemas ? 1 : 0,
            deltaAdaptabilidade = categoria == CategoriaSoftSkill.Adaptabilidade ? 1 : 0,
            deltaEmpatia = categoria == CategoriaSoftSkill.Empatia ? 1 : 0,

            emocaoJogadorAposEscolha = Emocao.Neutro,
            emocaoPersonagemAposEscolha = Emocao.Neutro,

            proximoNo = proximoNo
        };
    }

    OpcaoEscolha CriarOpcaoRuim(CategoriaSoftSkill categoria, string textoBotao, string falaJogador, string reacaoNPC, int proximoNo)
    {
        return new OpcaoEscolha
        {
            textoOpcao = textoBotao,
            respostaJogador = falaJogador,
            tomResposta = TomResposta.Rude,
            categoria = categoria,
            reacaoNPC = reacaoNPC,
            pontosAprovacao = 0,

            deltaComunicacao = categoria == CategoriaSoftSkill.Comunicacao ? -1 : 0,
            deltaTrabalhoEquipe = categoria == CategoriaSoftSkill.TrabalhoEquipe ? -1 : 0,
            deltaResolucaoProblemas = categoria == CategoriaSoftSkill.ResolucaoProblemas ? -1 : 0,
            deltaAdaptabilidade = categoria == CategoriaSoftSkill.Adaptabilidade ? -1 : 0,
            deltaEmpatia = categoria == CategoriaSoftSkill.Empatia ? -1 : 0,

            emocaoJogadorAposEscolha = Emocao.Raiva,
            emocaoPersonagemAposEscolha = Emocao.Raiva,

            proximoNo = proximoNo
        };
    }

    void MostrarNoAtual()
    {
        if (indiceNoAtual < 0 || indiceNoAtual >= nos.Count)
        {
            MostrarResultadoFase();
            return;
        }

        NoDialogoVN noAtual = nos[indiceNoAtual];

        string falaNPC;
        string falaJogador;

        if (exibindoReacaoEscolha)
        {
            falaJogador = ultimaRespostaJogador;
            falaNPC = ultimaReacaoNPC;
        }
        else
        {
            falaNPC = EscolherTextoAleatorio(noAtual.falasVariaveis);
            falaJogador = EscolherTextoAleatorio(noAtual.respostasJogadorVariaveis);
        }

        textoCompletoNPC = falaNPC;
        textoCompletoJogador = falaJogador;

        bool npcTemFala = !string.IsNullOrWhiteSpace(falaNPC);
        bool jogadorTemFala = !string.IsNullOrWhiteSpace(falaJogador);

        if (caixaNomeNPC != null) caixaNomeNPC.SetActive(npcTemFala);
        if (textoFalaNPC != null) textoFalaNPC.gameObject.SetActive(npcTemFala);

        if (caixaNomeJogador != null) caixaNomeJogador.SetActive(jogadorTemFala);
        if (textoFalaJogador != null) textoFalaJogador.gameObject.SetActive(jogadorTemFala);

        if (textoNomeNPC != null && npcTemFala)
            textoNomeNPC.text = noAtual.personagemFalando != null ? noAtual.personagemFalando.nomePersonagem : "NPC";

        if (textoNomeJogador != null && jogadorTemFala)
            textoNomeJogador.text = nomeJogador;

        Emocao emocaoEsquerdaFinal = noAtual.emocaoEsquerda;
        Emocao emocaoCentroFinal = noAtual.emocaoCentro;
        Emocao emocaoDireitaFinal = noAtual.emocaoDireita;

        if (exibindoReacaoEscolha && noAtual.personagemFalando != null)
        {
            if (noAtual.personagemFalando == noAtual.personagemEsquerda)
                emocaoEsquerdaFinal = ultimaEmocaoPersonagem;

            if (noAtual.personagemFalando == noAtual.personagemCentro)
                emocaoCentroFinal = ultimaEmocaoPersonagem;

            if (noAtual.personagemFalando == noAtual.personagemDireita)
                emocaoDireitaFinal = ultimaEmocaoPersonagem;
        }
        else
        {
            emocaoAtualJogador = noAtual.emocaoJogadorDuranteNo;
        }

        if (controladorCena != null)
        {
            controladorCena.AtualizarPersonagem(controladorCena.imagemEsquerda, noAtual.personagemEsquerda, emocaoEsquerdaFinal, noAtual.mostrarEsquerda);
            controladorCena.AtualizarPersonagem(controladorCena.imagemCentro, noAtual.personagemCentro, emocaoCentroFinal, noAtual.mostrarCentro);
            controladorCena.AtualizarPersonagem(controladorCena.imagemDireita, noAtual.personagemDireita, emocaoDireitaFinal, noAtual.mostrarDireita);
            controladorCena.AtualizarJogador(aparenciaAtualJogador, emocaoAtualJogador);

            controladorCena.DestacarFalante(
                noAtual.personagemFalando,
                noAtual.personagemEsquerda,
                noAtual.personagemCentro,
                noAtual.personagemDireita
            );
        }

        if (painelEscolhas != null) painelEscolhas.SetActive(false);

        if (botaoContinuar != null)
            botaoContinuar.gameObject.SetActive(true);

        IniciarDigitacao(falaJogador, falaNPC, jogadorTemFala, npcTemFala);
    }

    void IniciarDigitacao(string falaJogador, string falaNPC, bool jogadorTemFala, bool npcTemFala)
    {
        if (rotinaDigitacao != null)
            StopCoroutine(rotinaDigitacao);

        rotinaDigitacao = StartCoroutine(DigitarTextos(falaJogador, falaNPC, jogadorTemFala, npcTemFala));
    }

    IEnumerator DigitarTextos(string falaJogador, string falaNPC, bool jogadorTemFala, bool npcTemFala)
    {
        textoDigitando = true;

        if (textoFalaJogador != null)
            textoFalaJogador.text = "";

        if (textoFalaNPC != null)
            textoFalaNPC.text = "";

        if (jogadorTemFala)
        {
            for (int i = 0; i <= falaJogador.Length; i++)
            {
                if (textoFalaJogador != null)
                    textoFalaJogador.text = falaJogador.Substring(0, i);

                yield return new WaitForSeconds(velocidadeDigitacao);
            }
        }

        if (npcTemFala)
        {
            for (int i = 0; i <= falaNPC.Length; i++)
            {
                if (textoFalaNPC != null)
                    textoFalaNPC.text = falaNPC.Substring(0, i);

                yield return new WaitForSeconds(velocidadeDigitacao);
            }
        }

        textoDigitando = false;
        FinalizarExibicaoDoNo();
    }

    void FinalizarDigitacaoImediata()
    {
        if (rotinaDigitacao != null)
            StopCoroutine(rotinaDigitacao);

        if (textoFalaJogador != null)
            textoFalaJogador.text = textoCompletoJogador;

        if (textoFalaNPC != null)
            textoFalaNPC.text = textoCompletoNPC;

        textoDigitando = false;
        FinalizarExibicaoDoNo();
    }

    void FinalizarExibicaoDoNo()
    {
        if (indiceNoAtual < 0 || indiceNoAtual >= nos.Count)
            return;

        NoDialogoVN noAtual = nos[indiceNoAtual];

        if (exibindoReacaoEscolha)
        {
            if (painelEscolhas != null) painelEscolhas.SetActive(false);
            if (botaoContinuar != null) botaoContinuar.gameObject.SetActive(true);
            return;
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

    void ContinuarDialogo()
    {
        if (textoDigitando)
        {
            FinalizarDigitacaoImediata();
            return;
        }

        if (exibindoReacaoEscolha)
        {
            exibindoReacaoEscolha = false;

            if (aguardandoResultadoFase)
            {
                aguardandoResultadoFase = false;
                MostrarResultadoFase();
                return;
            }

            indiceNoAtual = proximoNoAposReacao;
            MostrarNoAtual();
            return;
        }

        if (indiceNoAtual < 0 || indiceNoAtual >= nos.Count)
            return;

        indiceNoAtual = nos[indiceNoAtual].proximoNoSimples;
        MostrarNoAtual();
    }

    void EscolherOpcao(OpcaoEscolha opcao)
    {
        pontosFaseAtual += opcao.pontosAprovacao;
        AtualizarMedidor();

        comunicacao += opcao.deltaComunicacao;
        trabalhoEquipe += opcao.deltaTrabalhoEquipe;
        resolucaoProblemas += opcao.deltaResolucaoProblemas;
        adaptabilidade += opcao.deltaAdaptabilidade;
        empatia += opcao.deltaEmpatia;

        emocaoAtualJogador = opcao.emocaoJogadorAposEscolha;
        ultimaEmocaoPersonagem = opcao.emocaoPersonagemAposEscolha;

        ultimaRespostaJogador = opcao.respostaJogador;
        ultimaReacaoNPC = opcao.reacaoNPC;

        exibindoReacaoEscolha = true;

        if (opcao.proximoNo == -1 || opcao.proximoNo >= nos.Count)
        {
            aguardandoResultadoFase = true;
            proximoNoAposReacao = -1;
        }
        else
        {
            aguardandoResultadoFase = false;
            proximoNoAposReacao = opcao.proximoNo;
        }

        MostrarNoAtual();
    }

    void MostrarResultadoFase()
    {
        if (painelDialogo != null) painelDialogo.SetActive(false);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (painelResultadoFase != null) painelResultadoFase.SetActive(true);

        porcentagemFase = ((float)pontosFaseAtual / pontosMaximosFase) * 100f;

        bool aprovado = porcentagemFase >= PorcentagemNecessaria(faseAtual);
        string textoAprovacao;

        if (faseAtual == FaseProfissional.FacilJunior)
        {
            proximaFaseDepoisResultado = aprovado ? FaseProfissional.MedioPleno : FaseProfissional.FacilJunior;
            textoAprovacao = aprovado ? "Aprovado para a 2ª Fase - Média (Pleno)." : "Você precisa repetir a 1ª Fase - Fácil.";
            finalizarDepoisResultado = false;
        }
        else if (faseAtual == FaseProfissional.MedioPleno)
        {
            proximaFaseDepoisResultado = aprovado ? FaseProfissional.DificilSenior : FaseProfissional.MedioPleno;
            textoAprovacao = aprovado ? "Aprovado para a 3ª Fase - Difícil (Sênior)." : "Você precisa repetir a 2ª Fase - Média.";
            finalizarDepoisResultado = false;
        }
        else
        {
            textoAprovacao = aprovado ? "Você concluiu a fase Sênior com bom desempenho." : "Você concluiu a fase Sênior, mas precisa melhorar suas soft skills.";
            finalizarDepoisResultado = true;
        }

        if (textoResultadoFase != null)
        {
            textoResultadoFase.text =
                "Resultado da " + NomeFase(faseAtual) + "\n\n" +
                "Aprovação necessária: " + PorcentagemNecessaria(faseAtual).ToString("F0") + "%\n" +
                "Sua aprovação: " + porcentagemFase.ToString("F0") + "%\n\n" +
                textoAprovacao + "\n\n" +
                "Comunicação: " + comunicacao + "\n" +
                "Trabalho em Equipe: " + trabalhoEquipe + "\n" +
                "Resolução de Problemas: " + resolucaoProblemas + "\n" +
                "Adaptabilidade: " + adaptabilidade + "\n" +
                "Empatia: " + empatia;
        }
    }

    void ContinuarDepoisResultadoFase()
    {
        if (painelResultadoFase != null)
            painelResultadoFase.SetActive(false);

        if (finalizarDepoisResultado)
        {
            MostrarFinal();
            return;
        }

        IniciarFase(proximaFaseDepoisResultado);
    }

    void MostrarFinal()
    {
        if (painelDialogo != null) painelDialogo.SetActive(false);
        if (painelEscolhas != null) painelEscolhas.SetActive(false);
        if (painelResultadoFase != null) painelResultadoFase.SetActive(false);
        if (painelFinal != null) painelFinal.SetActive(true);

        if (textoFinal == null)
            return;

        textoFinal.text =
            "Resultado Final\n\n" +
            "Jogador: " + nomeJogador + "\n" +
            "Simulação concluída: Carreira em TI\n\n" +
            "Comunicação: " + comunicacao + "\n" +
            "Trabalho em Equipe: " + trabalhoEquipe + "\n" +
            "Resolução de Problemas: " + resolucaoProblemas + "\n" +
            "Adaptabilidade: " + adaptabilidade + "\n" +
            "Empatia: " + empatia + "\n\n" +
            "Perfil: " + GerarPerfil() + "\n\n" +
            "Áreas indicadas:\n" + GerarAreas();
    }

    string GerarPerfil()
    {
        if (comunicacao >= 25 && trabalhoEquipe >= 25 && empatia >= 20)
            return "Perfil colaborativo, comunicativo e preparado para atuar bem em equipes de TI.";

        if (resolucaoProblemas >= 25 && adaptabilidade >= 20)
            return "Perfil técnico forte, com boa capacidade de resolver problemas e se adaptar a mudanças.";

        if (trabalhoEquipe >= 25 && comunicacao >= 20)
            return "Perfil com potencial para liderança técnica, coordenação de equipe e mediação de conflitos.";

        if (empatia < 5 || comunicacao < 5)
            return "Perfil que precisa desenvolver melhor escuta, comunicação e inteligência emocional no ambiente profissional.";

        return "Perfil equilibrado, com competências socioemocionais em desenvolvimento.";
    }

    string GerarAreas()
    {
        List<string> areas = new List<string>();

        if (resolucaoProblemas >= 20)
            areas.Add("- Desenvolvimento de Software, Backend, Frontend, Full Stack");

        if (comunicacao >= 20)
            areas.Add("- Product Owner, Scrum Master, Suporte Técnico, Customer Success");

        if (trabalhoEquipe >= 20)
            areas.Add("- Squad de Desenvolvimento, Gestão Ágil, Coordenação de Projetos");

        if (adaptabilidade >= 20)
            areas.Add("- DevOps, SRE, Cloud, Sustentação de Sistemas");

        if (empatia >= 20)
            areas.Add("- Liderança Técnica, Mentoria, People Management, RH Tech");

        if (areas.Count == 0)
            areas.Add("- Áreas iniciais de TI com foco em desenvolvimento gradual de soft skills");

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