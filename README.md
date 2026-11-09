# Material Property Modifier

Drive shader properties on uGUI `Graphic`s (Image, RawImage, Text…) per instance, without creating or duplicating material assets.

## Installation

Package Manager → **+** → **Add package from git URL…**

```
https://github.com/SamSarfaTDS/MaterialPropertyModifier.git#v0.1.1-preview
```

Or add it to `Packages/manifest.json`:

```json
"com.tripledot.materialpropertymodifier": "https://github.com/SamSarfaTDS/MaterialPropertyModifier.git#v0.1.1-preview"
```

## Usage

1. Give a `Graphic` a material that uses your shader.
2. Add **GraphicMaterialPropertyModifier** to the same GameObject.
3. Add bindings: use the **▾** picker to choose a property from the shader, or type the property name yourself.
4. Change or animate the values. Each Graphic gets its own material instance, so the shared material asset is never changed.

## Samples

Import **Examples** from the package's **Samples** tab in Package Manager. It contains a scene with a UI circle shader animated through the modifier.

## Requirements

- Unity 6000.0+
- uGUI (`com.unity.ugui`)
