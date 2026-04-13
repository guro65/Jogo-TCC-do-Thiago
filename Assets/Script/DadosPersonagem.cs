using UnityEngine;

public class DadosPersonagem : MonoBehaviour
{
    [Header("Informações")]
    public string nomePersonagem;
    public int idade;
    [TextArea(2, 5)]
    public string descricaoPersonalidade;

    [Header("Configuração")]
    public PersonalidadePersonagem personalidade;
    public TipoAmbiente ambiente;

    [Header("Sprites por emoção")]
    public Sprite spriteFeliz;
    public Sprite spriteNeutro;
    public Sprite spriteRaiva;

    public Sprite ObterSpritePorEmocao(Emocao emocao)
    {
        switch (emocao)
        {
            case Emocao.Feliz:
                return spriteFeliz;
            case Emocao.Raiva:
                return spriteRaiva;
            default:
                return spriteNeutro;
        }
    }
}