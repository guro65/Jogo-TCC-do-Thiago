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

    [Header("Categoria avaliada")]
    public CategoriaSoftSkill categoria;

    [Header("Reação do NPC")]
    [TextArea(2, 4)]
    public string reacaoNPC;

    [Header("Pontuação da fase")]
    public int pontosAprovacao;

    [Header("Pontuação por soft skill")]
    public int deltaComunicacao;
    public int deltaTrabalhoEquipe;
    public int deltaResolucaoProblemas;
    public int deltaAdaptabilidade;
    public int deltaEmpatia;

    [Header("Emoções após a escolha")]
    public Emocao emocaoJogadorAposEscolha = Emocao.Neutro;
    public Emocao emocaoPersonagemAposEscolha = Emocao.Neutro;

    [Header("Fluxo")]
    public int proximoNo = -1;
}

[System.Serializable]
public class NoDialogoVN
{
    public int id;
    public TipoNoDialogo tipoNo;

    [Header("Quem fala")]
    public DadosPersonagem personagemFalando;

    [Header("Quem aparece na cena")]
    public DadosPersonagem personagemEsquerda;
    public DadosPersonagem personagemCentro;
    public DadosPersonagem personagemDireita;

    [Header("Emoções dos NPCs")]
    public Emocao emocaoEsquerda = Emocao.Neutro;
    public Emocao emocaoCentro = Emocao.Neutro;
    public Emocao emocaoDireita = Emocao.Neutro;

    [Header("Emoção do jogador durante essa fala")]
    public Emocao emocaoJogadorDuranteNo = Emocao.Neutro;

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