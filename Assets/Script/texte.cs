using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DialogoPainel : MonoBehaviour
{
    public GameObject painel;
    public TextMeshProUGUI falaNpc;

    public float velocidade = 0.05f;

    private int index = 0;
    private bool escrevendo = false;

    void Start()
    {
        painel.SetActive(false);
    }

    public void BotaoDialogo()
    {
        if (!painel.activeSelf)
        {
            IniciarDialogo();
        }
        else
        {
            ProximaFala();
        }
    }

    void IniciarDialogo()
    {
        painel.SetActive(true);
        index = 0;
        StartCoroutine(Escrever());
    }

    IEnumerator Escrever()
    {
        escrevendo = true;
        falaNpc.text = "";

        string falaAtual = PegarFala();

        foreach (char letra in falaAtual)
        {
            falaNpc.text += letra;
            yield return new WaitForSeconds(velocidade);
        }

        escrevendo = false;
    }

    void ProximaFala()
    {
        if (escrevendo) return;

        if (index < 2)
        {
            index++;
            StartCoroutine(Escrever());
        }
        else
        {
            painel.SetActive(false);
        }
    }

    string PegarFala()
    {
        switch (index)
        {
            case 0:
                return "Olá jogador!";
            case 1:
                return "Bem-vindo ao meu jogo.";
            case 2:
                return "Aperte no botão para continuar.";
            default:
                return "";
        }
    }
}