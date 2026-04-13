using UnityEngine;
using UnityEngine.UI;

public class ControladorCenaVN : MonoBehaviour
{
    public Image imagemEsquerda;
    public Image imagemCentro;
    public Image imagemDireita;
    public Image imagemJogador;

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
    }

    public void AtualizarJogador(AparenciaJogador aparencia, Emocao emocao)
    {
        if (imagemJogador == null || aparencia == null)
            return;

        imagemJogador.sprite = aparencia.ObterSpritePorEmocao(emocao);
    }
}