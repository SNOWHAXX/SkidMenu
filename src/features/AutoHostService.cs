using UnityEngine;

namespace SkidMenu.features;

public static class AutoHostService
{
    private enum State { Idle, Warmup, Countdown, Backoff }

    private static State _state = State.Idle;
    private static float _warmupTimer    = 0f;
    private static float _countdownTimer = 0f;
    private static float _backoffTimer   = 0f;
    private static float _lobbyAge       = 0f;
    private static bool  _inLobby        = false;

    public static string StatusText        { get; private set; } = "Idle";
    public static int    ConnectedPlayers  { get; private set; } = 0;
    public static float  WarmupRemaining    => _warmupTimer;
    public static float  CountdownRemaining => _countdownTimer;
    public static float  LoadGraceRemaining => 0f;
    public static float  BackoffRemaining   => _backoffTimer;
    public static float  LobbyAgeSeconds    => _lobbyAge;

    public static void OnLobbyOpened()
    {
        _state          = State.Idle;
        _warmupTimer    = 0f;
        _countdownTimer = 0f;
        _backoffTimer   = 0f;
        _lobbyAge       = 0f;
        _inLobby        = true;
    }

    public static void OnGameEnded()
    {
        _inLobby        = true;
        _lobbyAge       = 0f;
        _backoffTimer   = SkidMenu.autoHostBackoffSeconds;
        _state          = State.Backoff;
    }

    public static void Tick(GameStartManager instance)
    {
        if (!_inLobby) return;

        float dt    = Time.unscaledDeltaTime;
        _lobbyAge  += dt;

        ConnectedPlayers = 0;
        try
        {
            if (GameData.Instance != null)
                foreach (var p in GameData.Instance.AllPlayers)
                    if (p != null && !p.Disconnected) ConnectedPlayers++;
        }
        catch { }

        if (!SkidMenu.autoHostEnabled || AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
        {
            _state     = State.Idle;
            StatusText = "Idle";
            return;
        }

        int  minPlayers = SkidMenu.autoHostMinPlayers;
        bool hasEnough  = ConnectedPlayers >= minPlayers;

        switch (_state)
        {
            case State.Backoff:
                StatusText    = "Backoff";
                _backoffTimer -= dt;
                if (_backoffTimer <= 0f) { _backoffTimer = 0f; _state = State.Idle; }
                break;

            case State.Idle:
                StatusText = hasEnough ? "Idle — starting warmup" : "Waiting for players";
                if (hasEnough)
                {
                    _warmupTimer = SkidMenu.autoHostWarmupSeconds;
                    _state       = State.Warmup;
                }
                break;

            case State.Warmup:
                if (!hasEnough && SkidMenu.autoHostCancelBelowMin)
                { _state = State.Idle; _warmupTimer = 0f; break; }

                StatusText = "Warmup";
                if (SkidMenu.autoHostInstantStart)
                    _warmupTimer = 0f;
                else
                {
                    _warmupTimer -= dt;
                    int fastAt = SkidMenu.autoHostFastStartPlayers;
                    if (fastAt > 0 && ConnectedPlayers >= fastAt)
                    {
                        float fastCap = SkidMenu.autoHostFastStartDelaySeconds;
                        if (_warmupTimer > fastCap) _warmupTimer = fastCap;
                    }
                }

                if (_warmupTimer <= 0f)
                {
                    _countdownTimer = SkidMenu.autoHostStartDelaySeconds;
                    _state          = State.Countdown;
                }
                break;

            case State.Countdown:
                if (!hasEnough && SkidMenu.autoHostCancelBelowMin)
                { _state = State.Idle; _countdownTimer = 0f; break; }

                StatusText       = "Starting";
                _countdownTimer -= dt;

                int forceAfterM = SkidMenu.autoHostForceAfterMinutes;
                if (forceAfterM > 0 && SkidMenu.autoHostForceLastMinute && _lobbyAge >= forceAfterM * 60f)
                    _countdownTimer = 0f;

                if (_countdownTimer <= 0f && ConnectedPlayers >= SkidMenu.autoHostForceMinPlayers)
                {
                    try { AmongUsClient.Instance.SendStartGame(); } catch { }
                    _inLobby   = false;
                    StatusText = "Started";
                }
                break;
        }
    }
}

