#ifndef CUSTOM_SURFACE_INCLUDED
#define CUSTOM_SURFACE_INCLUDED
//仿造URPLit，记录物体表面属性
struct Surface {
    half3 color;
    half3 normalWS;
    half  metallic;
    half  roughness;
    half ambientOcclusion;
    half alpha;
};

#endif