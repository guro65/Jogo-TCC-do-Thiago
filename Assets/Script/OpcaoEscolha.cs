using System.Collections.Generic;
using UnityEngine;

public enum TomResposta
{
    Boa,
    Neutra,
    Rude
}

[System.Serializable]
public class OpcaoEscolha
{
    [Header("Texto do botão")]
    public string textoOpcao;

    [Header("Fala real do jogador")]
    [TextArea(2, 4)]
    public string respostaJogador;

    [Header("Tom da resposta")]
    public TomResposta tomResposta = TomResposta.Neutra;

    [Header("Reação do NPC depois da resposta")]
    [TextArea(2, 4)]
    public string reacaoNPC;

    public int deltaEmpatia;
    public int deltaComunicacao;
    public int deltaControleEmocional;
    public int deltaLideranca;

    public Emocao emocaoJogadorAposEscolha = Emocao.Neutro;
    public Emocao emocaoPersonagemAposEscolha = Emocao.Neutro;

    public int proximoNo = -1;
}

[System.Serializable]
public class NoDialogoVN
{
    public int id;
    public TipoNoDialogo tipoNo;

    [Header("Quem fala")]
    public DadosPersonagem personagemFalando;
    public Emocao emocaoPersonagemFalando = Emocao.Neutro;

    [Header("Quem aparece na cena")]
    public DadosPersonagem personagemEsquerda;
    public DadosPersonagem personagemCentro;
    public DadosPersonagem personagemDireita;

    public Emocao emocaoEsquerda = Emocao.Neutro;
    public Emocao emocaoCentro = Emocao.Neutro;
    public Emocao emocaoDireita = Emocao.Neutro;

    public bool mostrarEsquerda = true;
    public bool mostrarCentro = true;
    public bool mostrarDireita = true;

    [Header("Texto do NPC")]
    [TextArea(2, 5)]
    public List<string> falasVariaveis = new List<string>();

    [Header("Fala do jogador")]
    public List<string> respostasJogadorVariaveis = new List<string>();

    [Header("Escolhas")]
    public List<OpcaoEscolha> opcoes = new List<OpcaoEscolha>();

    [Header("Fluxo")]
    public int proximoNoSimples = -1;
}