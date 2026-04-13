using System;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("componetes de fala")]
    public GameObject painel;
    public TextMeshProUGUI falaNpc;
    public GameObject imagem;
    public Button button;
    public float velocidadeDeEscrita = 0.06f;
    [Header("iniciar Fala")]
    private int index = 0;
    public bool podeFalar = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button.gameObject.SetActive(true);
        painel.SetActive(false);
        
        button = GetComponent<Button>();
    }
    // Update is called once per frame
    void Update()
    {
      
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
        podeFalar = true;
        falaNpc.text = "";

        string FalaAtual = PegarFala();

        foreach(char letra in FalaAtual)
        {
        falaNpc.text += letra;
        yield return new WaitForSeconds(velocidadeDeEscrita);
        }

        podeFalar = false;
    }

    void ProximaFala()
    {
        if (podeFalar) return;
        if(index < 2)
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
                return "Aperte E para continuar.";
            default:
                return "";
        }
    }

}
