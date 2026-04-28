using UnityEngine;

[System.Serializable]
public class AparenciaJogador
{
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