using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;

namespace SkidMenu;

public class StreamerUI : MonoBehaviour
{
    private Camera      _captureCamera;
    private Camera      _uiCaptureCamera;
    private RenderTexture _renderTexture;
    private Texture2D   _captureTexture;
    private int         _lastWidth;
    private int         _lastHeight;
    private bool        _hostStarted;
    private bool        _captureQueued;

    private static readonly List<LineRenderer>       _lineRendererCache = new();
    private static readonly List<(TMPro.TMP_Text, string)> _nametagCache = new();
    private static int _cacheFrame = -1;
    private const  int CacheRefreshInterval = 90;

    public static int FpsCap         = 30;
    public static int ResolutionScale = 100;
    private float _timeSinceCapture = 0f;
    private static byte[] _conversionBuffer;

    private void OnDestroy()
    {
        Cleanup();
        StreamerNativeWindowHost.Stop();
        _hostStarted    = false;
        _captureQueued  = false;
    }

    private void LateUpdate()
    {
        if (!CheatToggles.streamerMode || SkidMenu.isPanicked)
        {
            Cleanup();
            StreamerNativeWindowHost.Stop();
            _hostStarted   = false;
            _captureQueued = false;
            return;
        }

        var sourceCamera = ResolveSourceCamera();
        if (!sourceCamera) { Cleanup(); return; }

        if (!_hostStarted || !StreamerNativeWindowHost.IsRunning)
        {
            StreamerNativeWindowHost.Start(Screen.width, Screen.height);
            _hostStarted = true;
        }

        if (_captureQueued || !isActiveAndEnabled || !gameObject.activeInHierarchy) return;

        float captureInterval = 1f / Mathf.Clamp(FpsCap, 1, 240);
        _timeSinceCapture += Time.unscaledDeltaTime;
        if (_timeSinceCapture < captureInterval) return;

        _captureQueued = true;
        this.StartCoroutine(CaptureFrame());
    }

    private IEnumerator CaptureFrame()
    {
        yield return new WaitForEndOfFrame();

        try
        {
            if (!CheatToggles.streamerMode || SkidMenu.isPanicked) yield break;

            var sourceCamera = ResolveSourceCamera();
            if (!sourceCamera) { Cleanup(); yield break; }

            EnsureCaptureCamera(sourceCamera);
            EnsureUiCaptureCamera();
            if (!_captureCamera || !_renderTexture) yield break;

            RefreshLineRendererCache();
            DisableESPLineRenderers();
            StripNametags();

            _captureCamera.transform.SetPositionAndRotation(
                sourceCamera.transform.position,
                sourceCamera.transform.rotation);
            _captureCamera.orthographic      = sourceCamera.orthographic;
            _captureCamera.orthographicSize  = 3f;
            _captureCamera.fieldOfView       = sourceCamera.fieldOfView;
            _captureCamera.nearClipPlane     = sourceCamera.nearClipPlane;
            _captureCamera.farClipPlane      = sourceCamera.farClipPlane;
            _captureCamera.backgroundColor   = sourceCamera.backgroundColor;
            _captureCamera.clearFlags        = sourceCamera.clearFlags;
            _captureCamera.cullingMask       = sourceCamera.cullingMask;
            _captureCamera.allowHDR          = sourceCamera.allowHDR;
            _captureCamera.allowMSAA         = sourceCamera.allowMSAA;
            _captureCamera.targetTexture     = _renderTexture;
            _captureCamera.Render();

            var hudManager = DestroyableSingleton<HudManager>.Instance;
            if (_uiCaptureCamera && hudManager && hudManager.UICamera != null)
            {
                var uic = hudManager.UICamera;
                _uiCaptureCamera.transform.SetPositionAndRotation(
                    uic.transform.position,
                    uic.transform.rotation);
                _uiCaptureCamera.orthographic     = uic.orthographic;
                _uiCaptureCamera.orthographicSize = uic.orthographicSize;
                _uiCaptureCamera.fieldOfView      = uic.fieldOfView;
                _uiCaptureCamera.nearClipPlane    = uic.nearClipPlane;
                _uiCaptureCamera.farClipPlane     = uic.farClipPlane;
                _uiCaptureCamera.backgroundColor  = uic.backgroundColor;
                _uiCaptureCamera.clearFlags       = CameraClearFlags.Depth;
                _uiCaptureCamera.cullingMask      = uic.cullingMask;
                _uiCaptureCamera.allowHDR         = uic.allowHDR;
                _uiCaptureCamera.allowMSAA        = uic.allowMSAA;
                _uiCaptureCamera.targetTexture    = _renderTexture;
                _uiCaptureCamera.Render();
            }

            int w = _renderTexture.width;
            int h = _renderTexture.height;

            if (_captureTexture == null || _captureTexture.width != w || _captureTexture.height != h)
            {
                if (_captureTexture != null) Destroy(_captureTexture);
                _captureTexture = new Texture2D(w, h, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            var prev = RenderTexture.active;
            RenderTexture.active = _renderTexture;
            _captureTexture.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
            _captureTexture.Apply(false, false);
            RenderTexture.active = prev;

            RestoreESPLineRenderers();
            RestoreNametags();

            int pixelCount = w * h;
            int needed     = pixelCount * 4;
            if (_conversionBuffer == null || _conversionBuffer.Length < needed)
                _conversionBuffer = new byte[needed];

            var pixels = _captureTexture.GetPixels32();
            lock (StreamerNativeWindowHost.SyncObject)
            {
                for (int i = 0, idx = 0; i < pixelCount; i++, idx += 4)
                {
                    var p = pixels[i];
                    _conversionBuffer[idx]     = p.b;
                    _conversionBuffer[idx + 1] = p.g;
                    _conversionBuffer[idx + 2] = p.r;
                    _conversionBuffer[idx + 3] = p.a;
                }
            }

            StreamerNativeWindowHost.UpdateFrame(_conversionBuffer, w, h);
        }
        finally
        {
            _captureQueued = false;
        }
    }

    private static void RefreshLineRendererCache()
    {
        if (Time.frameCount - _cacheFrame < CacheRefreshInterval) return;
        _cacheFrame = Time.frameCount;
        _lineRendererCache.Clear();
        foreach (var lr in UnityEngine.Object.FindObjectsByType<LineRenderer>(FindObjectsSortMode.None))
            if (lr) _lineRendererCache.Add(lr);
    }

    private static void DisableESPLineRenderers()
    {
        foreach (var lr in _lineRendererCache)
            if (lr) lr.enabled = false;
    }

    private static void RestoreESPLineRenderers()
    {
        foreach (var lr in _lineRendererCache)
            if (lr) lr.enabled = true;
    }

    private static void StripNametags()
    {
        _nametagCache.Clear();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            try
            {
                if (player == null || player.Data == null || player.cosmetics?.nameText == null) continue;
                var txt = player.cosmetics.nameText;
                _nametagCache.Add((txt, txt.text));
                txt.text = player.Data.DefaultOutfit.PlayerName;
            }
            catch { }
        }
    }

    private static void RestoreNametags()
    {
        foreach (var (txt, orig) in _nametagCache)
            try { if (txt) txt.text = orig; } catch { }
        _nametagCache.Clear();
    }

    private void EnsureCaptureCamera(Camera src)
    {
        if (_captureCamera && _renderTexture &&
            _lastWidth == Screen.width && _lastHeight == Screen.height) return;

        Cleanup();
        _lastWidth  = Screen.width;
        _lastHeight = Screen.height;

        int w = Mathf.Max(640, Screen.width  * ResolutionScale / 100);
        int h = Mathf.Max(360, Screen.height * ResolutionScale / 100);

        _renderTexture = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        _renderTexture.Create();

        var go = new GameObject("SkidMenu_StreamerCamera") { hideFlags = HideFlags.HideAndDontSave };
        _captureCamera = go.AddComponent<Camera>();
        _captureCamera.CopyFrom(src);
        _captureCamera.enabled       = false;
        _captureCamera.targetTexture = _renderTexture;

        StreamerNativeWindowHost.Resize(w, h);
    }

    private void EnsureUiCaptureCamera()
    {
        if (_uiCaptureCamera) return;
        var hud = DestroyableSingleton<HudManager>.Instance;
        if (!hud || hud.UICamera == null) return;
        var go = new GameObject("SkidMenu_StreamerUICamera") { hideFlags = HideFlags.HideAndDontSave };
        _uiCaptureCamera = go.AddComponent<Camera>();
        _uiCaptureCamera.CopyFrom(hud.UICamera);
        _uiCaptureCamera.enabled = false;
    }

    private void Cleanup()
    {
        if (_captureCamera)   { Destroy(_captureCamera.gameObject);   _captureCamera   = null; }
        if (_uiCaptureCamera) { Destroy(_uiCaptureCamera.gameObject); _uiCaptureCamera = null; }
        if (_renderTexture)   { _renderTexture.Release(); Destroy(_renderTexture); _renderTexture = null; }
        if (_captureTexture)  { Destroy(_captureTexture); _captureTexture = null; }
    }

    private static Camera ResolveSourceCamera()
    {
        var main = Camera.main;
        if (main && main.GetComponent<FollowerCamera>()) return main;
        foreach (var cam in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            if (cam && cam.isActiveAndEnabled && cam.GetComponent<FollowerCamera>()) return cam;
        return main;
    }
}

internal static class StreamerNativeWindowHost
{
    private const int  WsOverlappedWindow = 0x00CF0000;
    private const int  SwShow             = 5;
    private const uint WmDestroy          = 0x0002;
    private const uint WmPaint            = 0x000F;
    private const uint WmClose            = 0x0010;
    private const uint WmSize             = 0x0005;
    private const uint Srccopy            = 0x00CC0020;
    private const int  DibRgbColors       = 0;

    private static readonly object        Sync       = new();
    public  static object                 SyncObject => Sync;
    private static readonly WndProcDelegate WndProc = WndProcImpl;
    private static readonly IntPtr        HInstance = GetModuleHandle(null);

    private static Thread  _thread;
    private static IntPtr  _hwnd = IntPtr.Zero;
    private static volatile bool _running;

    private static byte[] _frameBytes;
    private static int    _frameWidth;
    private static int    _frameHeight;
    private static int    _targetWidth  = 1280;
    private static int    _targetHeight = 720;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WndClassEx
    {
        public uint cbSize; public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra, cbWndExtra;
        public IntPtr hInstance, hIcon, hCursor, hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpszClassName;
        public IntPtr hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Msg
    {
        public IntPtr hwnd; public uint message;
        public IntPtr wParam, lParam;
        public uint time; public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)] private struct POINT { public int x, y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct PaintStruct
    {
        public IntPtr hdc; public bool fErase; public RECT rcPaint;
        public bool fRestore, fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] rgbReserved;
    }

    [StructLayout(LayoutKind.Sequential)] private struct RECT { public int left, top, right, bottom; }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfoHeader
    {
        public uint biSize; public int biWidth, biHeight;
        public ushort biPlanes, biBitCount; public uint biCompression, biSizeImage;
        public int biXPelsPerMeter, biYPelsPerMeter; public uint biClrUsed, biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfo
    {
        public BitmapInfoHeader bmiHeader;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] public uint[] bmiColors;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public static bool IsRunning => _running && _thread != null && _thread.IsAlive;

    public static void Start(int width, int height)
    {
        if (_running) return;
        _targetWidth  = Mathf.Max(640, width);
        _targetHeight = Mathf.Max(360, height);
        _running = true;
        _thread = new Thread(WindowThread) { IsBackground = true, Name = "SkidMenu Streamer Window" };
        try { _thread.SetApartmentState(ApartmentState.STA); } catch { }
        _thread.Start();
    }

    public static void Stop()
    {
        _running = false;
        var hwnd = _hwnd;
        if (hwnd != IntPtr.Zero) PostMessage(hwnd, WmClose, IntPtr.Zero, IntPtr.Zero);
    }

    public static void Resize(int width, int height)
    {
        _targetWidth  = Mathf.Max(640, width);
        _targetHeight = Mathf.Max(360, height);
        var hwnd = _hwnd;
        if (hwnd != IntPtr.Zero)
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, _targetWidth, _targetHeight, 0x0002 | 0x0004);
    }

    public static void UpdateFrame(byte[] bgra, int width, int height)
    {
        if (!_running || bgra == null || bgra.Length == 0) return;
        lock (Sync) { _frameBytes = bgra; _frameWidth = width; _frameHeight = height; }
        var hwnd = _hwnd;
        if (hwnd != IntPtr.Zero) InvalidateRect(hwnd, IntPtr.Zero, false);
    }

    private static void WindowThread()
    {
        const string cls = "SkidMenuStreamerWindowClass";
        var wc = new WndClassEx
        {
            cbSize        = (uint)Marshal.SizeOf<WndClassEx>(),
            style         = 0x0003,
            lpfnWndProc   = Marshal.GetFunctionPointerForDelegate(WndProc),
            hInstance     = HInstance,
            hCursor       = LoadCursor(IntPtr.Zero, new IntPtr(32512)),
            hbrBackground = new IntPtr(5),
            lpszClassName = cls
        };
        RegisterClassEx(ref wc);

        _hwnd = CreateWindowEx(0x00040000, cls, "Streamer Mode Preview",
            WsOverlappedWindow,
            unchecked((int)0x80000000), unchecked((int)0x80000000),
            _targetWidth, _targetHeight,
            IntPtr.Zero, IntPtr.Zero, HInstance, IntPtr.Zero);

        if (_hwnd == IntPtr.Zero) { _running = false; return; }

        ShowWindow(_hwnd, SwShow);
        UpdateWindow(_hwnd);

        Msg msg;
        while (_running && GetMessage(out msg, IntPtr.Zero, 0, 0))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }

        _hwnd    = IntPtr.Zero;
        _running = false;
    }

    private static IntPtr WndProcImpl(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WmPaint:
                var ps = new PaintStruct();
                var hdc = BeginPaint(hWnd, ref ps);
                DrawFrame(hdc);
                EndPaint(hWnd, ref ps);
                return IntPtr.Zero;
            case WmClose:
                DestroyWindow(hWnd);
                return IntPtr.Zero;
            case WmDestroy:
                PostQuitMessage(0);
                return IntPtr.Zero;
            default:
                return DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }

    private static void DrawFrame(IntPtr hdc)
    {
        byte[] frame; int w, h;
        lock (Sync) { frame = _frameBytes; w = _frameWidth; h = _frameHeight; }
        if (frame == null || frame.Length == 0 || w <= 0 || h <= 0) return;

        GetClientRect(_hwnd, out var cr);
        int dw = cr.right  - cr.left;
        int dh = cr.bottom - cr.top;

        var bmi = new BitmapInfo
        {
            bmiHeader = new BitmapInfoHeader
            {
                biSize        = (uint)Marshal.SizeOf<BitmapInfoHeader>(),
                biWidth       = w,
                biHeight      = h,
                biPlanes      = 1,
                biBitCount    = 32,
                biCompression = 0,
                biSizeImage   = (uint)frame.Length
            },
            bmiColors = new uint[1]
        };

        var pin = GCHandle.Alloc(frame, GCHandleType.Pinned);
        try { StretchDIBits(hdc, 0, 0, dw, dh, 0, 0, w, h, pin.AddrOfPinnedObject(), ref bmi, DibRgbColors, Srccopy); }
        finally { pin.Free(); }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(int dwExStyle, string cls, string name, int style,
        int x, int y, int w, int h, IntPtr parent, IntPtr menu, IntPtr inst, IntPtr param);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern short RegisterClassEx(ref WndClassEx lpwcx);
    [DllImport("user32.dll")] private static extern bool DestroyWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int n);
    [DllImport("user32.dll")] private static extern bool UpdateWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool InvalidateRect(IntPtr hWnd, IntPtr r, bool e);
    [DllImport("user32.dll")] private static extern IntPtr DefWindowProc(IntPtr hWnd, uint m, IntPtr w, IntPtr l);
    [DllImport("user32.dll")] private static extern bool GetMessage(out Msg m, IntPtr hWnd, uint f, uint t);
    [DllImport("user32.dll")] private static extern bool TranslateMessage([In] ref Msg m);
    [DllImport("user32.dll")] private static extern IntPtr DispatchMessage([In] ref Msg m);
    [DllImport("user32.dll")] private static extern IntPtr BeginPaint(IntPtr hWnd, ref PaintStruct ps);
    [DllImport("user32.dll")] private static extern bool EndPaint(IntPtr hWnd, [In] ref PaintStruct ps);
    [DllImport("user32.dll")] private static extern IntPtr LoadCursor(IntPtr inst, IntPtr name);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr hWnd, uint m, IntPtr w, IntPtr l);
    [DllImport("user32.dll")] private static extern void PostQuitMessage(int code);
    [DllImport("user32.dll")] private static extern bool GetClientRect(IntPtr hWnd, out RECT r);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int w, int h, uint f);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr GetModuleHandle(string m);
    [DllImport("gdi32.dll")] private static extern int StretchDIBits(IntPtr hdc, int xd, int yd, int dw, int dh,
        int xs, int ys, int sw, int sh, IntPtr bits, ref BitmapInfo bmi, int usage, uint rop);
}
