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
    public GameObject inicioMenu;
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
        inicioMenu.SetActive(true);
        
        button = GetComponent<Button>();
        IniciarDialogo();
    }
    // Update is called once per frame
    void Update()
    {
      
    }
     public void BotaoDialogo()
    {
        if (!inicioMenu.activeSelf)
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
        inicioMenu.SetActive(true);
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
        if(index < 1)
        {
            index++;
            StartCoroutine(Escrever());
        }
        else
        {
            inicioMenu.SetActive(false);
        }
    }
     string PegarFala()
    {
        switch (index)
        {
            case 0:
                return "Saudações usuario, no SkillForge nos treinamos a capacidade das suas soft skills, com varios testes que irão testar suas capacidades de comunicação, trabalho em equipe, resolução de conflitos , adaptabilidade e empatia";
            case 1:
                return "Meu nome é Sophia. Vou acompanhar você nessa jornada. Nos campos, peço que coloque seu nome e suas especificações de gênero. Aproveite o Skill Forge!";
            default:
                return "";
        }
    }

}
