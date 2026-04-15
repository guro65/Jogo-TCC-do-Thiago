using UnityEngine;

public class FecharJogo : MonoBehaviour
{
    public void SairDoJogo()
    {
        Debug.Log("Saindo do jogo...");

        Application.Quit();
    }
}