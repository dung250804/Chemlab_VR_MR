using System.Collections.Generic;
using com.ethnicthv.chemlab.engine.api.element;
using UnityEngine;

namespace com.ethnicthv.chemlab.client.core.renderer
{
    public class AtomColorAssigner
    {
        private static readonly Color[] StandardColors = {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.cyan,
            Color.magenta,
            Color.white,
            Color.black,
            Color.gray,
            Color.grey
        };
        
        private readonly Dictionary<Element, Color> _elementColors = new();
        
        private int _colorPivot = 0;

        public Color GetColorForElement(Element element)
        {
            if (_elementColors.TryGetValue(element, out var forElement))
            {
                return forElement;
            }
            
            if (_colorPivot < StandardColors.Length)
            {
                _elementColors[element] = StandardColors[_colorPivot];
                _colorPivot++;
                return StandardColors[_colorPivot - 1];
            }

            var color = GetRandomColor();
            while (CheckColorExist(color))
            {
                color = GetRandomColor();
            }
            _elementColors[element] = color;
            Debug.Log($"Color for {element} is {color}");
            return color;
        }
        
        public void Clear()
        {
            _colorPivot = 0;
            _elementColors.Clear();
        }

        private Color GetRandomColor()
        {
            return Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }

        private bool CheckColorExist(Color color)
        {
            foreach (var (_, elementColor) in _elementColors)
            {
                if (elementColor == color)
                {
                    return true;
                }
            }

            return false;
        }
        
        public IReadOnlyDictionary<Element, Color> GetElementColors()
        {
            return _elementColors;
        }
    }
}