////////////////////////////////////////////////////////////////////////////////////////////
                          Stylized Hit & Slash (by. VFX_Klaus)
////////////////////////////////////////////////////////////////////////////////////////////

Thank you for purchasing the Stylized Hit & Slash Package.
This note describes how this package is configured, how texture should be used, and how it works within a Particle System.

All the effect prepab is made of only two materials(Additive and Alpha Blended). These materials use only one texture.
I put all the elements of Hit & Slash into that one texture.

   ▷ Red channel is main texture.
   ▷ Green channel is dissolve texture. The main texture gradually dissolve into the shape of green texture.
   ▷ Blue channel gives UV distortion.

These RGB channels can be modified by Custom Data in the Particle System.
There are 4 Components of Custom Data.

   ▷ X value is for Dissolve. From 0 to 1, it gradually dissolves.
   ▷ Y value is for Dissolve Sharpness. The larger the number, the sharper the edges of dissolve.
   ▷ Z value is for Distortion. The larger the number, the stronger the distortion.
   ▷ W value is for Soft Particle Factor. As the number goes to zero, the overlap of mesh becomes transparent.

I also added "Emissive Power" parameter in additive material to adjust Bloom(Post Process).

Material and shader named "VFX_lab" are not used for Hit & Slash effects. It was used in the background of Scene just to show the effect.

This package can be used in projects that use Render Pipeline such as URP or HDRP.
The original shader I made may seem to work on the Render Pipeline, but that's not the shape I intended.
So if you're a developer working on the Render Pipeline project, Please replace the ShaderGraph shader on the each material.
(I put "_SG" after the file name of the ShaderGraph shader to distinguish it from the original shader.)

   ① Open the "Materials" folder. Then you can see 3 materials.
   ② You can click on each material to replace the shader on the Inspector tab, or open the "Shaders" folder and drag the shader into the material.
         ▷"VFX_lab_SG" shader → "VFX_lab" material
         ▷"Fx_Hit&Slash_add_SG" shader → "Mat_fx_Hit&Slash_add" material
         ▷"Fx_Hit&Slash_apb_SG" shader → "Mat_fx_Hit&Slash_apb" material
   ③ If you replace the shader of the material, it will be applied automatically to all effects.

Thank you once again, and I hope my effect will be useful for your development.
- KFX_Klaus