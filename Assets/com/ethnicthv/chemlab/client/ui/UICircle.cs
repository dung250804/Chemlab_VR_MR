using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace com.ethnicthv.chemlab.client.ui.element
{
    [RequireComponent(typeof(CanvasRenderer))]
    [ExecuteInEditMode]
    public class UICircle : Graphic
    { 
        [Range(0,100)]
        public int fillPercent;
        public bool fill = true;
        public int thickness = 5;

        void Update(){
            thickness = (int)Mathf.Clamp(thickness, 0, rectTransform.rect.width/2);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            float outer = -rectTransform.pivot.x * rectTransform.rect.width;
            float inner = -rectTransform.pivot.x * rectTransform.rect.width + thickness;

            float f = fillPercent / 100f;
            int fa = (int)(362 * f);

            UIVertex vert = UIVertex.simpleVert;
            Vector2 prevX = Vector2.zero;
            Vector2 prevY = Vector2.zero;

            for (int i = 0; i < fa; i++)
            {
                float rad = Mathf.Deg2Rad * i;
                float c = Mathf.Cos(rad);
                float s = Mathf.Sin(rad);
                float x = outer * c;
                float y = inner * c;
                vert.color = color;

                vert.position = prevX;
                vh.AddVert(vert);
                prevX = new Vector2(outer * c, outer * s);
                vert.position = prevX;
                vh.AddVert(vert);

                if (fill)
                {
                    vert.position = Vector2.zero;
                    vh.AddVert(vert);
                    vh.AddVert(vert);
                }
                else
                {
                    vert.position = new Vector2(inner * c, inner * s);
                    vh.AddVert(vert);
                    vert.position = prevY;
                    vh.AddVert(vert);
                    prevY = new Vector2(inner * c, inner * s);
                }
            }

            for (int i = 0; i < fa - 1; i++)
            {
                int index = i * 4;
                vh.AddTriangle(index, index + 1, index + 2);
                vh.AddTriangle(index + 2, index + 3, index);
            }
        }
    }
}