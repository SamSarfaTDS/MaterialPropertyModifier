using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Tripledot.MaterialPropertyModifier
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Graphic))]
    public abstract class GraphicMaterialModifier : MonoBehaviour, IMaterialModifier
    {
        private Graphic graphic;
        private Material cachedBaseMaterial;
        private Material materialInstance;
        
        protected Graphic Graphic => graphic ??= GetComponent<Graphic>();

        /// <summary>
        /// Marks the material as dirty, forcing the <see cref="Graphic"/> to call <see cref="GetModifiedMaterial"/> and update the material instance with current property values.
        /// </summary>
        public void SetMaterialDirty()
        {
            Graphic.SetMaterialDirty();
        }

        /// <inheritdoc/>
        public virtual Material GetModifiedMaterial(Material baseMaterial)
        {
            if (cachedBaseMaterial != baseMaterial) {
                // Base material of the graphic has changed, either by another modifier or externally by the user.
                CleanupMaterialInstance();
                cachedBaseMaterial = baseMaterial;
            }

            if (materialInstance == null || materialInstance.shader != baseMaterial.shader) {
                materialInstance = new Material(baseMaterial) { hideFlags = HideFlags.DontSave };
#if UNITY_EDITOR
                // In Editor, we parent the material instance so base changes are reflected immediately in the material instance
                materialInstance.parent = baseMaterial;
#endif
            } else {
                materialInstance.CopyPropertiesFromMaterial(baseMaterial);
            }
            
            UpdateMaterialProperties(materialInstance);

            return materialInstance;
        }

        /// <summary>
        /// Applies per-instance property values to the runtime material.
        /// </summary>
        /// <param name="material">
        /// The runtime material instance to modify.
        /// </param>
        protected abstract void UpdateMaterialProperties([NotNull] Material material);

        protected virtual void OnEnable()
        {
            Canvas.willRenderCanvases += OnWillRenderCanvases;
            SetMaterialDirty();
        }

        protected virtual void OnDisable()
        {
            Canvas.willRenderCanvases -= OnWillRenderCanvases;
            CleanupMaterialInstance();
            SetMaterialDirty(); // Ensure the original material is used again
        }

        protected virtual void OnWillRenderCanvases()
        {
            if (materialInstance != null) {
                UpdateMaterialProperties(materialInstance);
            }
        }

        protected void CleanupMaterialInstance()
        {
            if (materialInstance != null) {
#if UNITY_EDITOR
                if (Application.isPlaying) {
                    Destroy(materialInstance);
                } else {
                    DestroyImmediate(materialInstance);
                }
#else
                Destroy(materialInstance);
#endif
                materialInstance = null;
            }
        }
    }
}
