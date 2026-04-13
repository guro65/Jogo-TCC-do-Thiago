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
    public GameObject painelReflexao;
    public GameObject painelFinal;

    [Header("Tela Inicial")]
    public TMP_InputField campoNome;

    [Header("Diálogo")]
    public TMP_Text textoPersonagem;
    public TMP_Text textoFala;

    [Header("Escolhas")]
    public Button botaoEscolha1;
    public Button botaoEscolha2;
    public Button botaoEscolha3;
    public TMP_Text textoEscolha1;
    public TMP_Text textoEscolha2;
    public TMP_Text textoEscolha3;

    [Header("Reflexão")]
    public TMP_Text textoReflexao;
    public Button botaoContinuarReflexao;

    [Header("Final")]
    public TMP_Text textoFinal;
    public Button botaoReiniciar;

    private string nomeJogador = "Jogador";
    private int indiceCena = 0;

    private int empatia = 0;
    private int comunicacao = 0;
    private int controleEmocional = 0;
    private int lideranca = 0;

    private string ambienteEscolhido = "";
    private Dictionary<string, int> relacoes = new Dictionary<string, int>();

    private List<CenaDialogo> cenas = new List<CenaDialogo>();

    void Start()
    {
        painelInicio.SetActive(true);
        painelDialogo.SetActive(false);
        painelEscolhas.SetActive(false);
        painelReflexao.SetActive(false);
        painelFinal.SetActive(false);

        botaoContinuarReflexao.onClick.AddListener(ContinuarDepoisReflexao);
        botaoReiniciar.onClick.AddListener(ReiniciarJogo);
    }

    public void ComecarJogo()
    {
        nomeJogador = campoNome.text.Trim();

        if (string.IsNullOrEmpty(nomeJogador))
            nomeJogador = "Jogador";

        painelInicio.SetActive(false);
        painelDialogo.SetActive(true);
        painelEscolhas.SetActive(true);

        MontarCenasIntroducao();
        MostrarCenaAtual();
    }

    void MontarCenasIntroducao()
    {
        cenas.Clear();
        indiceCena = 0;

        cenas.Add(new CenaDialogo
        {
            personagem = "Sofia",
            fala = "Olá " + nomeJogador + ", me chamo Sofia. Estou aqui para guiar você no desenvolvimento das suas soft skills, algo muito importante para sua vida pessoal e profissional.",
            escolhas = new List<Escolha>()
            {
                new Escolha
                {
                    texto = "Continuar",
                    proximaCena = 1
                },
                new Escolha
                {
                    texto = "Quero entender melhor",
                    proximaCena = 1
                },
                new Escolha
                {
                    texto = "Vamos começar",
                    proximaCena = 1
                }
            }
        });

        cenas.Add(new CenaDialogo
        {
            personagem = "Sofia",
            fala = "Antes de começarmos, escolha o ambiente onde sua história irá começar.",
            escolhas = new List<Escolha>()
            {
                new Escolha
                {
                    texto = "Empresa",
                    proximaCena = 2,
                    ambiente = "Empresa"
                },
                new Escolha
                {
                    texto = "Faculdade",
                    proximaCena = 2,
                    ambiente = "Faculdade"
                },
                new Escolha
                {
                    texto = "Projeto em grupo",
                    proximaCena = 2,
                    ambiente = "Projeto em Grupo"
                }
            }
        });

        cenas.Add(new CenaDialogo
        {
            personagem = "Sofia",
            fala = "Ótimo. Agora vamos ver como você reage em situações do cotidiano.",
            escolhas = new List<Escolha>()
            {
                new Escolha
                {
                    texto = "Continuar",
                    proximaCena = 3
                },
                new Escolha
                {
                    texto = "Estou pronto",
                    proximaCena = 3
                },
                new Escolha
                {
                    texto = "Vamos lá",
                    proximaCena = 3
                }
            }
        });

        cenas.Add(new CenaDialogo
        {
            personagem = "Colega",
            fala = "Hoje não é um bom dia. Já estou cansado e ainda preciso lidar com isso tudo...",
            escolhas = new List<Escolha>()
            {
                new Escolha
                {
                    texto = "Ignorar as provocações",
                    proximaCena = 4,
                    deltaControle = 1,
                    deltaComunicacao = -1,
                    deltaRelacao = -1,
                    nomePersonagemRelacao = "Colega"
                },
                new Escolha
                {
                    texto = "Responder de forma agressiva",
                    proximaCena = 4,
                    deltaEmpatia = -2,
                    deltaControle = -2,
                    deltaRelacao = -2,
                    nomePersonagemRelacao = "Colega"
                },
                new Escolha
                {
                    texto = "Perguntar com calma o motivo",
                    proximaCena = 4,
                    deltaEmpatia = 2,
                    deltaComunicacao = 2,
                    deltaControle = 1,
                    deltaRelacao = 2,
                    nomePersonagemRelacao = "Colega"
                }
            }
        });

        cenas.Add(new CenaDialogo
        {
            personagem = "Sofia",
            fala = "Agora vamos refletir sobre sua escolha.",
            reflexao = "Por que você escolheu agir dessa forma? Você faria o mesmo na vida real?",
            mostrarReflexao = true,
            proximaCenaAposReflexao = 5
        });

        cenas.Add(new CenaDialogo
        {
            personagem = "Equipe",
            fala = "Um membro da equipe não entregou a parte dele e o prazo está acabando. O que você faz?",
            escolhas = new List<Escolha>()
            {
                new Escolha
                {
                    texto = "Culpar a pessoa na frente de todos",
                    proximaCena = 6,
                    deltaEmpatia = -2,
                    deltaLideranca = -1,
                    deltaControle = -1
                },
                new Escolha
                {
                    texto = "Conversar em particular e reorganizar a equipe",
                    proximaCena = 6,
                    deltaEmpatia = 2,
                    deltaLideranca = 2,
                    deltaComunicacao = 1
                },
                new Escolha
                {
                    texto = "Assumir tudo sozinho sem falar nada",
                    proximaCena = 6,
                    deltaControle = 1,
                    deltaComunicacao = -1,
                    deltaLideranca = -1
                }
            }
        });

        cenas.Add(new CenaDialogo
        {
            personagem = "Sofia",
            fala = "Chegamos ao momento decisivo.",
            escolhas = new List<Escolha>()
            {
                new Escolha
                {
                    texto = "Continuar",
                    proximaCena = 7
                },
                new Escolha
                {
                    texto = "Entendi",
                    proximaCena = 7
                },
                new Escolha
                {
                    texto = "Vamos ao final",
                    proximaCena = 7
                }
            }
        });

        cenas.Add(new CenaDialogo
        {
            personagem = "Gerente / Professor / Líder",
            fala = "O projeto apresentou um erro grave. Como você reage?",
            escolhas = new List<Escolha>()
            {
                new Escolha
                {
                    texto = "Culpar outra pessoa",
                    proximaCena = -1,
                    deltaEmpatia = -2,
                    deltaResponsabilidadeOculta = -2
                },
                new Escolha
                {
                    texto = "Assumir responsabilidade e buscar solução",
                    proximaCena = -1,
                    deltaEmpatia = 1,
                    deltaLideranca = 2,
                    deltaComunicacao = 1,
                    deltaResponsabilidadeOculta = 2
                },
                new Escolha
                {
                    texto = "Chamar todos para resolver juntos",
                    proximaCena = -1,
                    deltaEmpatia = 2,
                    deltaLideranca = 2,
                    deltaComunicacao = 2,
                    deltaResponsabilidadeOculta = 1
                }
            }
        });
    }

    void MostrarCenaAtual()
    {
        if (indiceCena < 0 || indiceCena >= cenas.Count)
        {
            MostrarFinal();
            return;
        }

        CenaDialogo cenaAtual = cenas[indiceCena];

        textoPersonagem.text = cenaAtual.personagem;

        string falaFormatada = cenaAtual.fala;
        if (!string.IsNullOrEmpty(ambienteEscolhido))
        {
            falaFormatada += "\n\nAmbiente atual: " + ambienteEscolhido;
        }

        textoFala.text = falaFormatada;

        if (cenaAtual.mostrarReflexao)
        {
            painelEscolhas.SetActive(false);
            painelReflexao.SetActive(true);
            textoReflexao.text = cenaAtual.reflexao;
            return;
        }

        painelReflexao.SetActive(false);
        painelEscolhas.SetActive(true);

        ConfigurarBotao(botaoEscolha1, textoEscolha1, cenaAtual.escolhas, 0);
        ConfigurarBotao(botaoEscolha2, textoEscolha2, cenaAtual.escolhas, 1);
        ConfigurarBotao(botaoEscolha3, textoEscolha3, cenaAtual.escolhas, 2);
    }

    void ConfigurarBotao(Button botao, TMP_Text textoBotao, List<Escolha> escolhas, int indice)
    {
        if (indice < escolhas.Count)
        {
            botao.gameObject.SetActive(true);
            textoBotao.text = escolhas[indice].texto;
            botao.onClick.RemoveAllListeners();
            Escolha escolhaAtual = escolhas[indice];
            botao.onClick.AddListener(() => ProcessarEscolha(escolhaAtual));
        }
        else
        {
            botao.gameObject.SetActive(false);
        }
    }

    void ProcessarEscolha(Escolha escolha)
    {
        empatia += escolha.deltaEmpatia;
        comunicacao += escolha.deltaComunicacao;
        controleEmocional += escolha.deltaControle;
        lideranca += escolha.deltaLideranca;

        if (!string.IsNullOrEmpty(escolha.ambiente))
        {
            ambienteEscolhido = escolha.ambiente;
        }

        if (!string.IsNullOrEmpty(escolha.nomePersonagemRelacao))
        {
            if (!relacoes.ContainsKey(escolha.nomePersonagemRelacao))
                relacoes[escolha.nomePersonagemRelacao] = 0;

            relacoes[escolha.nomePersonagemRelacao] += escolha.deltaRelacao;
        }

        if (escolha.proximaCena == -1)
        {
            MostrarFinal();
            return;
        }

        indiceCena = escolha.proximaCena;
        MostrarCenaAtual();
    }

    void ContinuarDepoisReflexao()
    {
        painelReflexao.SetActive(false);
        indiceCena = cenas[indiceCena].proximaCenaAposReflexao;
        MostrarCenaAtual();
    }

    void MostrarFinal()
    {
        painelDialogo.SetActive(false);
        painelEscolhas.SetActive(false);
        painelReflexao.SetActive(false);
        painelFinal.SetActive(true);

        string perfil = DefinirPerfilFinal();

        textoFinal.text =
            "Resultado Final\n\n" +
            "Jogador: " + nomeJogador + "\n" +
            "Ambiente escolhido: " + ambienteEscolhido + "\n\n" +
            "Empatia: " + empatia + "\n" +
            "Comunicação: " + comunicacao + "\n" +
            "Controle Emocional: " + controleEmocional + "\n" +
            "Liderança: " + lideranca + "\n\n" +
            "Perfil identificado:\n" + perfil;
    }

    string DefinirPerfilFinal()
    {
        if (empatia >= 4 && comunicacao >= 3)
        {
            return "Final colaborativo: você tende a ouvir, compreender os outros e buscar soluções em conjunto.";
        }

        if (lideranca >= 3 && controleEmocional >= 2)
        {
            return "Final racional: você demonstra boa tomada de decisão e postura diante de problemas.";
        }

        if (empatia < 0 || controleEmocional < 0)
        {
            return "Final conflituoso: suas escolhas mostraram dificuldade em lidar com emoções e conflitos.";
        }

        return "Final neutro: você evitou confrontos diretos, mas pode precisar se posicionar melhor em situações importantes.";
    }

    void ReiniciarJogo()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

[System.Serializable]
public class CenaDialogo
{
    public string personagem;
    [TextArea(3, 8)]
    public string fala;
    public List<Escolha> escolhas = new List<Escolha>();

    public bool mostrarReflexao = false;
    [TextArea(2, 5)]
    public string reflexao;
    public int proximaCenaAposReflexao = -1;
}

[System.Serializable]
public class Escolha
{
    public string texto;
    public int proximaCena;

    public int deltaEmpatia;
    public int deltaComunicacao;
    public int deltaControle;
    public int deltaLideranca;

    public int deltaRelacao;
    public string nomePersonagemRelacao;

    public string ambiente;

    public int deltaResponsabilidadeOculta;
}