using UnityEngine;

namespace com.ethnicthv.chemlab.client.game.util
{
    [ExecuteInEditMode]
    public class PopulateAtlasInfo : MonoBehaviour
    {
        private static readonly int SpriteData = Shader.PropertyToID("_SpriteData");

        private void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            Act();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Application.isPlaying) return;
            var rend = GetComponent<SpriteRenderer>();
            var sprite1 = rend.sprite;
            var sprite = sprite1.textureRect;

            var spriteData = new Vector4(
                sprite.x / sprite1.texture.width,
                sprite.y / sprite1.texture.height,
                sprite.width / sprite1.texture.width,
                sprite.height / sprite1.texture.height
            );

            rend.sharedMaterial.SetVector(SpriteData, spriteData);
        }
#endif

        private void Act()
        {
            var rend = GetComponent<SpriteRenderer>();
            var sprite1 = rend.sprite;
            var sprite = sprite1.textureRect;

            var spriteData = new Vector4(
                sprite.x / sprite1.texture.width,
                sprite.y / sprite1.texture.height,
                sprite.width / sprite1.texture.width,
                sprite.height / sprite1.texture.height
            );

            rend.material.SetVector(SpriteData, spriteData);
        }
    }
}