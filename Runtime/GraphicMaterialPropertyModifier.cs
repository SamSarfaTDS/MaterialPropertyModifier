using System;
using UnityEngine;

namespace Tripledot.MaterialPropertyModifier
{
    public enum MaterialPropertyValueType
    {
        Float,
        Int,
        Bool,
        Color,
        Vector
    }

    public enum MaterialPropertyControlMode
    {
        Number,
        Slider,
        Toggle,
        ColorWheel,
        Vector2,
        Vector3,
        Vector4
    }

    [Serializable]
    public struct MaterialPropertyBinding
    {
        public string propertyName;
        public MaterialPropertyValueType valueType;
        public MaterialPropertyControlMode controlMode;
        public float floatValue;
        public int intValue;
        public float minValue;
        public float maxValue;
        public bool boolValue;
        public Color colorValue;
        public Vector4 vectorValue;
    }

    public class GraphicMaterialPropertyModifier : GraphicMaterialModifier
    {
        private const int SlotCount = 8;

        [SerializeField]
        private MaterialPropertyBinding property0;
        [SerializeField]
        private MaterialPropertyBinding property1;
        [SerializeField]
        private MaterialPropertyBinding property2;
        [SerializeField]
        private MaterialPropertyBinding property3;
        [SerializeField]
        private MaterialPropertyBinding property4;
        [SerializeField]
        private MaterialPropertyBinding property5;
        [SerializeField]
        private MaterialPropertyBinding property6;
        [SerializeField]
        private MaterialPropertyBinding property7;
        
        private readonly ResolvedProperty[] resolved = new ResolvedProperty[SlotCount];

        protected override void UpdateMaterialProperties(Material material)
        {
            ApplyBinding(material, property0, ref resolved[0]);
            ApplyBinding(material, property1, ref resolved[1]);
            ApplyBinding(material, property2, ref resolved[2]);
            ApplyBinding(material, property3, ref resolved[3]);
            ApplyBinding(material, property4, ref resolved[4]);
            ApplyBinding(material, property5, ref resolved[5]);
            ApplyBinding(material, property6, ref resolved[6]);
            ApplyBinding(material, property7, ref resolved[7]);
        }

        private static void ApplyBinding(Material material, in MaterialPropertyBinding binding, ref ResolvedProperty cache)
        {
            if (string.IsNullOrEmpty(binding.propertyName)) {
                return;
            }

            // UpdateMaterialProperties runs every frame (see GraphicMaterialModifier.OnWillRenderCanvases),
            // so this cache skips the native Material.Set* call when the bound value hasn't actually
            // changed since the last frame, which is the common case outside of an active animation.
            if (cache.name != binding.propertyName || cache.appliedType != binding.valueType) {
                cache.name = binding.propertyName;
                cache.id = Shader.PropertyToID(binding.propertyName);
                cache.appliedType = binding.valueType;
                cache.hasApplied = false;
            }

            switch (binding.valueType) {
                case MaterialPropertyValueType.Int:
                    if (!cache.hasApplied || cache.appliedInt != binding.intValue) {
                        material.SetInt(cache.id, binding.intValue);
                        cache.appliedInt = binding.intValue;
                    }
                    break;
                case MaterialPropertyValueType.Bool:
                    if (!cache.hasApplied || cache.appliedBool != binding.boolValue) {
                        material.SetInt(cache.id, binding.boolValue ? 1 : 0);
                        cache.appliedBool = binding.boolValue;
                    }
                    break;
                case MaterialPropertyValueType.Color:
                    if (!cache.hasApplied || cache.appliedColor != binding.colorValue) {
                        material.SetColor(cache.id, binding.colorValue);
                        cache.appliedColor = binding.colorValue;
                    }
                    break;
                case MaterialPropertyValueType.Vector:
                    if (!cache.hasApplied || cache.appliedVector != binding.vectorValue) {
                        material.SetVector(cache.id, binding.vectorValue);
                        cache.appliedVector = binding.vectorValue;
                    }
                    break;
                default:
                    if (!cache.hasApplied || cache.appliedFloat != binding.floatValue) {
                        material.SetFloat(cache.id, binding.floatValue);
                        cache.appliedFloat = binding.floatValue;
                    }
                    break;
            }

            cache.hasApplied = true;
        }

        private struct ResolvedProperty
        {
            public string name;
            public int id;
            public bool hasApplied;
            public MaterialPropertyValueType appliedType;
            public float appliedFloat;
            public int appliedInt;
            public bool appliedBool;
            public Color appliedColor;
            public Vector4 appliedVector;
        }
    }
}
