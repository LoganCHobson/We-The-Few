using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace FMCOLOR
{
    [CustomEditor(typeof(FMColor))]
    [CanEditMultipleObjects]
    public class FMColor_Editor : Editor
    {
        private Texture2D logo;
        private FMColor FC;

        private bool ShowPreset = false;
        private bool ShowBasicSettings = false;
        private bool ShowFresnelSettings = false;
        private bool ShowFogSettings = false;
        private bool ShowGrainSettings = false;
        private bool ShowVignetteSettings = false;
        private bool ShowOutlineSettings = false;
        private bool ShowScanlineSettings = false;
        private bool ShowFXAASettings = false;

        SerializedProperty LutModeProp;
        SerializedProperty LutContributionProp;
        SerializedProperty LookupTextureProp;

        SerializedProperty PixelSizeProp;
        SerializedProperty CelCutsProp;
        SerializedProperty HorizontalFlipProp;
        SerializedProperty VerticalFlipProp;

        SerializedProperty DebugSliderProp;
        SerializedProperty EnableTouchSliderProp;

        SerializedProperty BasicSettingsTintColorProp;
        SerializedProperty BasicSettingsHueProp;
        SerializedProperty BasicSettingsSaturationProp;
        SerializedProperty BasicSettingsBrightnessProp;
        SerializedProperty BasicSettingsContrastProp;
        SerializedProperty BasicSettingsSharpnessProp;

        SerializedProperty GrainSettingsContributionProp;
        SerializedProperty GrainSettingsSizeProp;

        SerializedProperty VignetteSettingsContributionProp;
        SerializedProperty VignetteSettingsColorProp;

        SerializedProperty ScanlineSettingsContributionProp;
        SerializedProperty ScanlineSettingsColorProp;
        SerializedProperty ScanlineSettingsScanlineXProp;
        SerializedProperty ScanlineSettingsScanlineYProp;

        //------------------------------------------------
        SerializedProperty PaintRadiusProp;
        SerializedProperty PaintBlurLevelProp;

        SerializedProperty FresnelSettingsContributionProp;
        SerializedProperty FresnelSettingsFresnelPowerProp;
        SerializedProperty FresnelSettingsFresnelColorProp;
        SerializedProperty FresnelSettingsFresnelScanlineXProp;
        SerializedProperty FresnelSettingsFresnelScanlineYProp;


        SerializedProperty FogSettingsContributionProp;
        SerializedProperty FogSettingsDepthLevelProp;
        SerializedProperty FogSettingsDepthClippingFarProp;
        SerializedProperty FogSettingsFogColorProp;
        SerializedProperty FogSettingsForegroundColorProp;


        SerializedProperty OutlineSettingsContributionProp;
        SerializedProperty OutlineSettingsColorProp;
        SerializedProperty OutlineSettingsNormalMultProp;
        SerializedProperty OutlineSettingsNormalBiasProp;
        SerializedProperty OutlineSettingsDepthMultProp;
        SerializedProperty OutlineSettingsDepthBiasProp;

        SerializedProperty FXAASettingsEnableProp;
        SerializedProperty FXAASettingsSubpixelBlendingProp;
        SerializedProperty FXAASettingsLowQualityProp;

        SerializedProperty IgnoreSkyboxProp;

        SerializedProperty ModeProp;
        //------------------------------------------------

        private void OnEnable()
        {
            ModeProp = serializedObject.FindProperty("Mode");
            LutModeProp = serializedObject.FindProperty("LutMode");
            LutContributionProp = serializedObject.FindProperty("LutContribution");
            LookupTextureProp = serializedObject.FindProperty("LookupTexture");

            PixelSizeProp = serializedObject.FindProperty("PixelSize");
            CelCutsProp = serializedObject.FindProperty("CelCuts");
            HorizontalFlipProp = serializedObject.FindProperty("HorizontalFlip");
            VerticalFlipProp = serializedObject.FindProperty("VerticalFlip");

            DebugSliderProp = serializedObject.FindProperty("DebugSlider");
            EnableTouchSliderProp = serializedObject.FindProperty("EnableTouchSlider");

            BasicSettingsTintColorProp = serializedObject.FindProperty("BasicSettings.TintColor");
            BasicSettingsHueProp = serializedObject.FindProperty("BasicSettings.Hue");
            BasicSettingsSaturationProp = serializedObject.FindProperty("BasicSettings.Saturation");
            BasicSettingsBrightnessProp = serializedObject.FindProperty("BasicSettings.Brightness");
            BasicSettingsContrastProp = serializedObject.FindProperty("BasicSettings.Contrast");
            BasicSettingsSharpnessProp = serializedObject.FindProperty("BasicSettings.Sharpness");

            GrainSettingsContributionProp = serializedObject.FindProperty("GrainSettings.Contribution");
            GrainSettingsSizeProp = serializedObject.FindProperty("GrainSettings.Size");

            VignetteSettingsContributionProp = serializedObject.FindProperty("VignetteSettings.Contribution");
            VignetteSettingsColorProp = serializedObject.FindProperty("VignetteSettings.Color");


            ScanlineSettingsContributionProp = serializedObject.FindProperty("ScanlineSettings.Contribution");
            ScanlineSettingsColorProp = serializedObject.FindProperty("ScanlineSettings.Color");
            ScanlineSettingsScanlineXProp = serializedObject.FindProperty("ScanlineSettings.ScanlineX");
            ScanlineSettingsScanlineYProp = serializedObject.FindProperty("ScanlineSettings.ScanlineY");

            //------------------------------------------------
            PaintRadiusProp = serializedObject.FindProperty("PaintRadius");
            PaintBlurLevelProp = serializedObject.FindProperty("PaintBlurLevel");


            FresnelSettingsContributionProp = serializedObject.FindProperty("FresnelSettings.Contribution");
            FresnelSettingsFresnelPowerProp = serializedObject.FindProperty("FresnelSettings.FresnelPower");
            FresnelSettingsFresnelColorProp = serializedObject.FindProperty("FresnelSettings.FresnelColor");
            FresnelSettingsFresnelScanlineXProp = serializedObject.FindProperty("FresnelSettings.FresnelScanlineX");
            FresnelSettingsFresnelScanlineYProp = serializedObject.FindProperty("FresnelSettings.FresnelScanlineY");


            FogSettingsContributionProp = serializedObject.FindProperty("FogSettings.Contribution");
            FogSettingsDepthLevelProp = serializedObject.FindProperty("FogSettings.DepthLevel");
            FogSettingsDepthClippingFarProp = serializedObject.FindProperty("FogSettings.DepthClippingFar");
            FogSettingsFogColorProp = serializedObject.FindProperty("FogSettings.FogColor");
            FogSettingsForegroundColorProp = serializedObject.FindProperty("FogSettings.ForegroundColor");

            OutlineSettingsContributionProp = serializedObject.FindProperty("OutlineSettings.Contribution");
            OutlineSettingsColorProp = serializedObject.FindProperty("OutlineSettings.Color");
            OutlineSettingsNormalMultProp = serializedObject.FindProperty("OutlineSettings.NormalMult");
            OutlineSettingsNormalBiasProp = serializedObject.FindProperty("OutlineSettings.NormalBias");
            OutlineSettingsDepthMultProp = serializedObject.FindProperty("OutlineSettings.DepthMult");
            OutlineSettingsDepthBiasProp = serializedObject.FindProperty("OutlineSettings.DepthBias");

            FXAASettingsEnableProp = serializedObject.FindProperty("FXAASettings.Enable");
            FXAASettingsSubpixelBlendingProp = serializedObject.FindProperty("FXAASettings.SubpixelBlending");
            FXAASettingsLowQualityProp = serializedObject.FindProperty("FXAASettings.LowQuality");

            IgnoreSkyboxProp = serializedObject.FindProperty("IgnoreSkybox");
            //------------------------------------------------
        }

        void Action_SetSymbol(FMColorMode _mode)
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = definesString.Split(';').ToList();

            //remove FMCOLOR symbol
            for (int i = 0; i < allDefines.Count; i++)
            {
                if (allDefines[i].Contains("FMCOLOR")) allDefines.RemoveAt(i);
            }

            List<string> newDefines = new List<string>();
            for (int i = 0; i < allDefines.Count; i++)
            {
                for (int j = 0; j < newDefines.Count; j++)
                {
                    if (allDefines[i] == newDefines[j]) break;
                }
                newDefines.Add(allDefines[i]);
            }

            switch (FC.Mode)
            {
                case FMColorMode.FMCOLOR_FULL: break;
                case FMColorMode.FMCOLOR_FAST: break;
                case FMColorMode.FMCOLOR_URP: newDefines.Add(_mode.ToString()); break;
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", newDefines.ToArray()));

        }


        public override void OnInspectorGUI()
        {
            if (FC == null) FC = (FMColor)target;
            if (logo == null) logo = Resources.Load<Texture2D>("Logo/" + "Logo_FMColor");
            if (logo != null)
            {
                const float maxLogoWidth = 430.0f;
                EditorGUILayout.Separator();
                float w = EditorGUIUtility.currentViewWidth;
                Rect r = new Rect();
                r.width = Math.Min(w - 40.0f, maxLogoWidth);
                r.height = r.width / 4.886f;
                Rect r2 = GUILayoutUtility.GetRect(r.width, r.height);
                r.x = r2.x;
                r.y = r2.y;
                GUI.DrawTexture(r, logo, ScaleMode.ScaleToFit);
                if (GUI.Button(r, "", new GUIStyle())) Application.OpenURL("http://frozenmist.com");
                EditorGUILayout.Separator();
            }

            serializedObject.Update();

            GUI.skin.button.alignment = TextAnchor.MiddleLeft;

            //Mode
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ModeProp, new GUIContent("RenderMode"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.yellow;
                GUILayout.Label(" Experiment: URP & HDRP supported", style);
                GUILayout.EndHorizontal();

                if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline == null)
                {
                    if (FC.Mode == FMColorMode.FMCOLOR_URP) FC.Mode = FMColorMode.FMCOLOR_FULL;
                }
                else
                {
                    if (FC.Mode != FMColorMode.FMCOLOR_URP) FC.Mode = FMColorMode.FMCOLOR_URP;
                }
                Action_SetSymbol(FC.Mode);
            }
            GUILayout.EndVertical();

            //Templates
            switch (FC.Mode)
            {
                case FMColorMode.FMCOLOR_FULL: TemplateGrp_FULL(); break;
                case FMColorMode.FMCOLOR_FAST: TemplateGrp_Fast(); break;
                case FMColorMode.FMCOLOR_URP: TemplateGrp_URP(); break;
            }

            //Shortcuts
            GUILayout.BeginHorizontal("box");
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(" < ")) FC.Action_PreviousLUT();
                GUILayout.Label(" Change LUT ");
                if (GUILayout.Button(" > ")) FC.Action_NextLUT();
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            //Lut
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(LutContributionProp, new GUIContent("LUT Contribution"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(LutModeProp, new GUIContent("LUT Mode"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(LookupTextureProp, new GUIContent("Look Up Texture"));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            //Cel
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(PixelSizeProp, new GUIContent("Pixel Size"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(CelCutsProp, new GUIContent("Cel Cuts"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(HorizontalFlipProp, new GUIContent("Horizontal Flip"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(VerticalFlipProp, new GUIContent("Vertical Flip"));
                GUILayout.EndHorizontal();

                if (FC.Mode == FMColorMode.FMCOLOR_FULL || FC.Mode == FMColorMode.FMCOLOR_URP)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(PaintRadiusProp, new GUIContent("Paint: Radius", "Recommended: 1~3 for mobile for performance"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(PaintBlurLevelProp, new GUIContent("Paint: Blur Level", "Recommended: 2~3 with Paint Effect 1~3"));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
            //Basic Settings
            GUILayout.BeginVertical("box");
            {
                if (!ShowBasicSettings)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("+ Basic Settings")) ShowBasicSettings = true;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("- Basic Settings")) ShowBasicSettings = false;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(BasicSettingsTintColorProp, new GUIContent("Tint Color"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(BasicSettingsHueProp, new GUIContent("Hue"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(BasicSettingsSaturationProp, new GUIContent("Saturation"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(BasicSettingsBrightnessProp, new GUIContent("Brightness"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(BasicSettingsContrastProp, new GUIContent("Contrast"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(BasicSettingsSharpnessProp, new GUIContent("Sharpness"));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            if (FC.Mode == FMColorMode.FMCOLOR_FULL)
            {
                //Fresnel Settings
                GUILayout.BeginVertical("box");
                {
                    if (!ShowFresnelSettings)
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("+ Fresnel Settings")) ShowFresnelSettings = true;
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("- Fresnel Settings")) ShowFresnelSettings = false;
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(FresnelSettingsContributionProp, new GUIContent("Contribution"));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(FresnelSettingsFresnelPowerProp, new GUIContent("Power"));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(FresnelSettingsFresnelColorProp, new GUIContent("Color"));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(FresnelSettingsFresnelScanlineXProp, new GUIContent("ScanlineX"));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(FresnelSettingsFresnelScanlineYProp, new GUIContent("ScanlineY"));
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();
            }

            //Fog Settings
            if (FC.Mode == FMColorMode.FMCOLOR_FULL)
            {
                GUILayout.BeginVertical("box");
                {
                    if (!ShowFogSettings)
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("+ Fog Settings")) ShowFogSettings = true;
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("- Fog Settings")) ShowFogSettings = false;
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(FogSettingsContributionProp, new GUIContent("Contribution"));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(FogSettingsDepthLevelProp, new GUIContent("Depth Level"));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(FogSettingsDepthClippingFarProp, new GUIContent("Depth Clipping Far"));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(FogSettingsFogColorProp, new GUIContent("Fog Color"));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(FogSettingsForegroundColorProp, new GUIContent("Foreground Color"));
                        GUILayout.EndHorizontal();

                    }
                }
                GUILayout.EndVertical();
            }

            //Grain Settings
            GUILayout.BeginVertical("box");
            {
                if (!ShowGrainSettings)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("+ Grain Settings")) ShowGrainSettings = true;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("- Grain Settings")) ShowGrainSettings = false;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(GrainSettingsContributionProp, new GUIContent("Contribution"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(GrainSettingsSizeProp, new GUIContent("Size"));
                    GUILayout.EndHorizontal();

                }
            }
            GUILayout.EndVertical();

            //Vignette Settings
            GUILayout.BeginVertical("box");
            {
                if (!ShowVignetteSettings)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("+ Vignette Settings")) ShowVignetteSettings = true;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("- Vignette Settings")) ShowVignetteSettings = false;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(VignetteSettingsContributionProp, new GUIContent("Contribution"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(VignetteSettingsColorProp, new GUIContent("Color"));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            //Outline Settings
            if (FC.Mode == FMColorMode.FMCOLOR_FULL)
            {
                GUILayout.BeginVertical("box");
                {
                    if (!ShowOutlineSettings)
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("+ Outline Settings")) ShowOutlineSettings = true;
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("- Outline Settings")) ShowOutlineSettings = false;
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(OutlineSettingsContributionProp, new GUIContent("Contribution"));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(OutlineSettingsColorProp, new GUIContent("Color"));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(OutlineSettingsNormalMultProp, new GUIContent("Normal Mult"));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(OutlineSettingsNormalBiasProp, new GUIContent("Normal Bias"));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(OutlineSettingsDepthMultProp, new GUIContent("Depth Mult"));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(OutlineSettingsDepthBiasProp, new GUIContent("Depth Bias"));
                        GUILayout.EndHorizontal();

                    }
                }
                GUILayout.EndVertical();
            }

            //Scanline Settings
            GUILayout.BeginVertical("box");
            {
                if (!ShowScanlineSettings)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("+ Scanline Settings")) ShowScanlineSettings = true;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("- Scanline Settings")) ShowScanlineSettings = false;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ScanlineSettingsContributionProp, new GUIContent("Contribution"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ScanlineSettingsColorProp, new GUIContent("Color"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ScanlineSettingsScanlineXProp, new GUIContent("Scanline X"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(ScanlineSettingsScanlineYProp, new GUIContent("Scanline Y"));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            //FXAA Settings
            if (FC.Mode == FMColorMode.FMCOLOR_FULL || FC.Mode == FMColorMode.FMCOLOR_URP)
            {
                GUILayout.BeginVertical("box");
                if (!ShowFXAASettings)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("+ FXAA Settings")) ShowFXAASettings = true;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("- FXAA Settings")) ShowFXAASettings = false;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FXAASettingsEnableProp, new GUIContent("Enable"));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FXAASettingsSubpixelBlendingProp, new GUIContent("Subpixel Blending"));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(FXAASettingsLowQualityProp, new GUIContent("Low Quality"));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }

            //Debug
            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(DebugSliderProp, new GUIContent("Debug Slider"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(EnableTouchSliderProp, new GUIContent("Touch Slide"));
                GUILayout.EndHorizontal();

                if (FC.Mode == FMColorMode.FMCOLOR_FULL)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(IgnoreSkyboxProp, new GUIContent("Ignore Skybox"));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            //DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }

        void TemplateGrp_FULL()
        {
            GUILayout.BeginVertical("box");
            {
                if (!ShowPreset)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("+ Templates")) ShowPreset = true;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("- Templates")) ShowPreset = false;
                    GUILayout.EndHorizontal();

                    GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(FC.LutMode.ToString());
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Minimum")) FC.Action_TemplateMinimum();
                    if (GUILayout.Button("Mono")) FC.Action_TemplateMono();
                    if (GUILayout.Button("Toon")) FC.Action_TemplateToon();
                    if (GUILayout.Button("Contrast")) FC.Action_TemplateContrast();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Scanline")) FC.Action_TemplateScanline();
                    if (GUILayout.Button("Grid")) FC.Action_TemplateGrid();
                    if (GUILayout.Button("Pixelate")) FC.Action_TemplatePixelate();
                    if (GUILayout.Button("Film Grain")) FC.Action_TemplateFilmGrain();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Fresnel line")) FC.Action_TemplateFresnelScanline();
                    if (GUILayout.Button("Fresnel Grid")) FC.Action_TemplateFresnelGrid();
                    if (GUILayout.Button("Fresnel")) FC.Action_TemplateFresnelColor();
                    if (GUILayout.Button("Outline")) FC.Action_TemplateOutline();
                    GUILayout.EndHorizontal();


                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Paint Soft")) FC.Action_TemplatePaintSoft();
                    if (GUILayout.Button("Paint Hard")) FC.Action_TemplatePaintHard();
                    if (GUILayout.Button("Paint Mono")) FC.Action_TemplatePaintMono();
                    if (GUILayout.Button("Paint Draft")) FC.Action_TemplatePaintDraft();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Paint Soft(M)")) FC.Action_TemplatePaintSoftMobile();
                    if (GUILayout.Button("Paint Hard(M)")) FC.Action_TemplatePaintHardMobile();
                    if (GUILayout.Button("Paint Mono(M)")) FC.Action_TemplatePaintMonoMobile();
                    if (GUILayout.Button("Paint Draft(M)")) FC.Action_TemplatePaintDraftMobile();
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }

        void TemplateGrp_URP()
        {
            GUILayout.BeginVertical("box");
            {
                if (!ShowPreset)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("+ Templates")) ShowPreset = true;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("- Templates")) ShowPreset = false;
                    GUILayout.EndHorizontal();

                    GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(FC.LutMode.ToString());
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Minimum")) FC.Action_TemplateMinimum();
                    if (GUILayout.Button("Mono")) FC.Action_TemplateMono();
                    if (GUILayout.Button("Toon")) FC.Action_TemplateToon();
                    if (GUILayout.Button("Contrast")) FC.Action_TemplateContrast();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Scanline")) FC.Action_TemplateScanline();
                    if (GUILayout.Button("Grid")) FC.Action_TemplateGrid();
                    if (GUILayout.Button("Pixelate")) FC.Action_TemplatePixelate();
                    if (GUILayout.Button("Film Grain")) FC.Action_TemplateFilmGrain();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Paint Soft")) FC.Action_TemplatePaintSoft();
                    if (GUILayout.Button("Paint Hard")) FC.Action_TemplatePaintHard();
                    if (GUILayout.Button("Paint Mono")) FC.Action_TemplatePaintMono();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Paint Soft(M)")) FC.Action_TemplatePaintSoftMobile();
                    if (GUILayout.Button("Paint Hard(M)")) FC.Action_TemplatePaintHardMobile();
                    if (GUILayout.Button("Paint Mono(M)")) FC.Action_TemplatePaintMonoMobile();
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }

        void TemplateGrp_Fast()
        {
            GUILayout.BeginVertical("box");
            {
                if (!ShowPreset)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("+ Templates")) ShowPreset = true;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("- Templates")) ShowPreset = false;
                    GUILayout.EndHorizontal();

                    GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(FC.LutMode.ToString());
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Minimum")) FC.Action_TemplateMinimum();
                    if (GUILayout.Button("Mono")) FC.Action_TemplateMono();
                    if (GUILayout.Button("Toon")) FC.Action_TemplateToon();
                    if (GUILayout.Button("Contrast")) FC.Action_TemplateContrast();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Scanline")) FC.Action_TemplateScanline();
                    if (GUILayout.Button("Grid")) FC.Action_TemplateGrid();
                    if (GUILayout.Button("Pixelate")) FC.Action_TemplatePixelate();
                    if (GUILayout.Button("Film Grain")) FC.Action_TemplateFilmGrain();
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }
    }
}
