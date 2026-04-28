using UnityEngine;
using UnityEngine.UI;

public class ControladorCenaVN : MonoBehaviour
{
    public Image imagemEsquerda;
    public Image imagemCentro;
    public Image imagemDireita;
    public Image imagemJogador;

    public Color corFalando = Color.white;
    public Color corNaoFalando = new Color(0.65f, 0.65f, 0.65f, 1f);

    public void AtualizarPersonagem(Image imagemDestino, DadosPersonagem personagem, Emocao emocao, bool mostrar)
    {
        if (imagemDestino == null)
            return;

        if (!mostrar || personagem == null)
        {
            imagemDestino.gameObject.SetActive(false);
            return;
        }

        imagemDestino.gameObject.SetActive(true);
        imagemDestino.sprite = personagem.ObterSpritePorEmocao(emocao);
        imagemDestino.color = corNaoFalando;
    }

    public void AtualizarJogador(AparenciaJogador aparencia, Emocao emocao)
    {
        if (imagemJogador == null || aparencia == null)
            return;

        imagemJogador.gameObject.SetActive(true);
        imagemJogador.sprite = aparencia.ObterSpritePorEmocao(emocao);
    }

    public void DestacarFalante(DadosPersonagem falante, DadosPersonagem esquerda, DadosPersonagem centro, DadosPersonagem direita)
    {
        if (imagemEsquerda != null)
            imagemEsquerda.color = (falante == esquerda) ? corFalando : corNaoFalando;

        if (imagemCentro != null)
            imagemCentro.color = (falante == centro) ? corFalando : corNaoFalando;

        if (imagemDireita != null)
            imagemDireita.color = (falante == direita) ? corFalando : corNaoFalando;
    }

    public void EsconderTodos()
    {
        if (imagemEsquerda != null) imagemEsquerda.gameObject.SetActive(false);
        if (imagemCentro != null) imagemCentro.gameObject.SetActive(false);
        if (imagemDireita != null) imagemDireita.gameObject.SetActive(false);
        if (imagemJogador != null) imagemJogador.gameObject.SetActive(false);
    }
}