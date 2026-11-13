# Material Property Modifier

Drive shader properties on uGUI `Graphic`s (Image, RawImage, Text…) per instance, without creating or duplicating material assets.

## Installation

Package Manager → **+** → **Add package from git URL…**

```
https://github.com/SamSarfaTDS/MaterialPropertyModifier.git#vx.x.x
```

Or add it to `Packages/manifest.json`:

```json
"com.tripledot.materialpropertymodifier": "https://github.com/SamSarfaTDS/MaterialPropertyModifier.git#v0.1.1"
```

## Usage

1. Give a `Graphic` a material that uses your shader (e.g., an Image).
2. Add **GraphicMaterialPropertyModifier** to the same GameObject.
3. Add bindings: use the **▾** picker to choose a property from the shader, or type the property name yourself.
4. Animate the shader via the bindings. No more need to manage material instances.

### Warning

The property modifier creates short lived instance of the material. This should be used exclusively for animations. Saving a scene or prefab discards the instanced material, reverting the values on screen to the ones in the original material.

## Example

Add the component to your game object

<img width="547" height="111" alt="image" src="https://github.com/user-attachments/assets/b05da843-1b9b-460f-9da0-b39f9121a7df" />

Click the Add Property button and select a property to expose from the list. The properties are read automatically from the material in the Image component

<img width="546" height="375" alt="image" src="https://github.com/user-attachments/assets/29392e36-4fe4-4e47-8029-79acecdc8749" />

Select the type of value you want to use to control the property and the control you want (e.g. free floating number, slider, Vector, etc)

<img width="529" height="148" alt="image" src="https://github.com/user-attachments/assets/41481170-afc9-4763-95fa-7a828e2a73fc" />


https://github.com/user-attachments/assets/b8e72896-0187-4af3-9279-6c5785ca73b6


## Samples

Import **Examples** from the package's **Samples** tab in Package Manager. It contains a scene with a UI circle shader animated through the modifier.

<img width="865" height="343" alt="image" src="https://github.com/user-attachments/assets/213080f2-badb-4c36-8044-063210fc26c4" />


## Requirements

- Unity 6000.0+
- uGUI (`com.unity.ugui`)
