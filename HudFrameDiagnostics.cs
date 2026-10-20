using System;
using System.Collections.Generic;
using UnityEngine;
namespace DiceVadersUltrawide;
public partial class HudSizing
{
    readonly bool _checkHudFrame = Environment.GetEnvironmentVariable("DICEVADERS_UW_HUD_FRAME_TEST") == "1";
    float _checkFrameAfter, _menuReadyAt;
    bool _checkedFrame, _pressedFrame, _resumedFrame;
    void CheckHudFrame()
    {
        if (!_checkHudFrame || _checkedFrame || !Plugin.Enabled.Value) return;
        if (_targets.Count == 0)
        {
            if (Time.unscaledTime < 10f) return;
            var boot = UnityEngine.Object.FindObjectOfType<StarVaders.BootupController>();
            if (boot != null && boot.IsFinishedLoadingGame && !_pressedFrame)
            { _pressedFrame = true; _menuReadyAt = Time.unscaledTime + 5f; boot.OnPressAnything(); return; }
            if (!_resumedFrame && Time.unscaledTime > _menuReadyAt)
            {
                var menu = UnityEngine.Object.FindObjectOfType<StarVaders.MainMenuController>();
                if (menu != null) { _resumedFrame = true; menu.OnPressResume(); }
            }
            return;
        }
        if (_checkFrameAfter == 0) _checkFrameAfter = Time.unscaledTime + 7f;
        if (Time.unscaledTime < _checkFrameAfter) return;
        _checkedFrame = true;
        foreach(var v in UnityEngine.Object.FindObjectsOfType<StarVaders.EncounterView>())
        {
            Plugin.Instance.Log.LogInfo("DICE BUTTON ROOTS: buttons="+PathOf(v.ButtonsArea)+" constellation="+PathOf(v.ConstellationButton?.transform)+" reroll="+PathOf(v.RerollButton?.transform)+" abilities="+PathOf(v.AbilitiesHolder)+" pause="+PathOf(v.PauseButton?.transform)+" map="+PathOf(v.MapPanel?.transform));
            DumpPart(v.AllHolder?.Find("BottomPanel"),0);
            DumpPart(FindPanel(v.AllHolder,"LeftPanel"),0);
            DumpPart(FindPanel(v.AllHolder,"RightPanel"),0);
        }
        bool savedFrame = _centerHud.Value;
        float savedSize = _hudScale.Value;
        try
        {
            foreach (float size in new[] {1f, 1.23f, 1.35f})
            {
                Restore(); _hudScale.Value=size; _centerHud.Value=false; ApplySizing();
                var full = Snapshot();
                Restore(); _centerHud.Value=true; ApplySizing();
                if (_frameWrappers.Count < 2) throw new Exception("HUD frame roots missing.");
                if (_board != null && full.TryGetValue(_board,out var board) &&
                    ((board.Scale-_board.localScale).sqrMagnitude>.0001f || (board.Position-_board.localPosition).sqrMagnitude>.0001f))
                    throw new Exception("HUD frame changed board transform.");
                CheckSwayRegression();
                var centered=Snapshot();
                for(int repeat=0;repeat<10;repeat++) ApplySizing();
                Compare(centered,"Repeated sizing drift");
                Restore(); _centerHud.Value=false; ApplySizing();
                Compare(full,"Full-width restoration");
                Plugin.Instance.Log.LogInfo($"HUD FRAME CHECK PASS: {size:P0}; native group anchors; board unchanged; repeated sizing stable; full width restored.");
            }
            Plugin.Instance.Log.LogInfo("HUD FRAME TEST PASS");
        }
        catch(Exception error) { Plugin.Instance.Log.LogError("HUD FRAME TEST FAIL: "+error); }
        finally { Restore(); _hudScale.Value=savedSize; _centerHud.Value=savedFrame; ApplySizing(); }
    }
    Dictionary<Transform,(Vector3 Scale,Vector3 Position)> Snapshot()
    {
        var state=new Dictionary<Transform,(Vector3,Vector3)>();
        foreach(var target in _targets) if(target!=null && target.gameObject.activeInHierarchy) state[target]=(target.localScale,target.localPosition);
        return state;
    }
    void Compare(Dictionary<Transform,(Vector3 Scale,Vector3 Position)> expected,string label)
    {
        foreach(var entry in expected)
            if ((entry.Key.localScale-entry.Value.Scale).sqrMagnitude>.0001f || (entry.Key.localPosition-entry.Value.Position).sqrMagnitude>.0001f)
                throw new Exception(label+": "+entry.Key.name+" expected "+entry.Value.Position+" actual "+entry.Key.localPosition);
    }
    // The game animates its side panels (X included). Centering must never
    // write those panels, only their wrappers, so animation can't capture or
    // accumulate a centering offset, and full width must be an exact zero.
    void CheckSwayRegression()
    {
        var wrappers=new List<RectTransform>(_frameWrappers.Values);
        var expected=new Dictionary<int,Vector2>();
        foreach(var wrapper in wrappers) expected[wrapper.GetInstanceID()]=wrapper.anchoredPosition;
        foreach(var wrapper in wrappers)
        {
            var panel=wrapper.GetChild(0);
            var native=panel.localPosition;
            try
            {
                for(int repeat=1;repeat<=20;repeat++)
                {
                    var animated=native+new Vector3(.5f*repeat,.125f*repeat,.01f*repeat);
                    panel.localPosition=animated;
                    ApplySizing(); Restore();
                    if((panel.localPosition-animated).sqrMagnitude>.0001f) throw new Exception("Centering wrote an animated panel: "+panel.name+" expected "+animated+" actual "+panel.localPosition);
                    if((wrapper.anchoredPosition-expected[wrapper.GetInstanceID()]).sqrMagnitude>.0001f) throw new Exception("Centering offset drifted during animation: "+panel.name);
                }
            }
            finally { panel.localPosition=native; }
        }
        _centerHud.Value=false; ApplySizing(); Restore();
        foreach(var wrapper in wrappers)
        {
            var parent=wrapper.parent.TryCast<RectTransform>();
            if(wrapper.localPosition.sqrMagnitude>.0001f || (wrapper.rect.size-parent.rect.size).sqrMagnitude>.0001f) throw new Exception("Full width wrapper is not an exact match for its parent: "+wrapper.name);
        }
        _centerHud.Value=true; ApplySizing();
        Plugin.Instance.Log.LogInfo("HUD SWAY REGRESSION PASS: animated panel X/Y/Z untouched; centering offset stable; full-width wrappers exactly match their parents.");
    }
    static string PathOf(Transform t)
    {
        string path=t?.name;
        for(var p=t?.parent;p!=null;p=p.parent) path=p.name+"/"+path;
        return path;
    }
    void DumpPart(Transform t,int depth)
    {
        if(t==null || depth>4) return;
        var r=t.TryCast<RectTransform>();
        string screen="";
        if(r!=null)
        {
            var canvas=r.GetComponentInParent<Canvas>();
            var cam=canvas==null || canvas.renderMode==RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            r.GetWorldCorners(_corners);
            float a=float.MaxValue,b=float.MinValue;
            for(int i=0;i<4;i++){ float x=RectTransformUtility.WorldToScreenPoint(cam,_corners[i]).x; a=Mathf.Min(a,x); b=Mathf.Max(b,x); }
            screen=$" screenX={a:0}..{b:0}";
        }
        Plugin.Instance.Log.LogInfo($"HUD TREE {new string(' ',depth*2)}{t.name} active={t.gameObject.activeSelf} pos={t.localPosition} scale={t.localScale} rect={r?.rect} anchors={r?.anchorMin}/{r?.anchorMax}{screen}");
        for(int i=0;i<t.childCount;i++) DumpPart(t.GetChild(i),depth+1);
    }}