using UnityEngine;

[RequireComponent(typeof(DashAbility))]
[DisallowMultipleComponent]
public class DashRingSpawner : MonoBehaviour
{
	[Header("Refs")]
	[SerializeField] private DashAbility dash;
	[SerializeField] private UpgradeHandler handler;

	[Header("Ring (visual, ALWAYS AT START)")]
	[SerializeField] private RingIndicator ringPrefab;
	[SerializeField] private float ringLifetime = 1.2f;
	[SerializeField] private Color ringColor = new Color(1f, .35f, .1f, 0.9f);

	[Header("Anger (Burst @ START)")]
	[SerializeField] private AngerBurstZone angerBurstPrefab;
	[SerializeField] private float[] anger_aoePercent = { 0f, 0.10f, 0.25f, 0.40f };

	[Header("Joy (Crit Aura @ START)")]
	[SerializeField] private JoyCritZone joyCritPrefab;
	[SerializeField] private float[] joy_chance = { 0f, 0.20f, 0.35f, 0.50f };
	[SerializeField] private float[] joy_radius = { 2.5f, 3.0f, 3.5f, 4.0f };
	[SerializeField] private float[] joy_duration = { 1.5f, 2.0f, 2.5f, 3.0f };

	[Header("Sadness (Slow ZONE @ START)")]
	[SerializeField] private SlowZone slowZonePrefab;
	[SerializeField] private float[] sadness_slowPercent = { 0f, 0.25f, 0.35f, 0.45f };
	[SerializeField] private float[] sadness_slowSeconds = { 1.5f, 2.0f, 2.5f, 3.0f };
	// if your SlowZone uses a radius on the prefab, you don’t need a radius array here

	[Header("Love (Weakness @ START)")]
	[SerializeField] private WeaknessZone weaknessZonePrefab;
	[SerializeField] private float[] love_weaknessPercent = { 0f, 0.10f, 0.20f, 0.30f };
	[SerializeField] private float[] love_duration = { 2.5f, 3.0f, 3.5f, 4.0f };
	[SerializeField] private float[] love_radius = { 2.0f, 2.5f, 3.0f, 3.5f };

	[Header("Fear (Fear @ START, single instance)")]
	[SerializeField] private FearZone fearZonePrefab;
	[SerializeField] private float[] fear_seconds = { 2.5f, 3.0f, 4.0f, 5.0f };
	[SerializeField] private float[] fear_radius = { 2.0f, 2.5f, 3.0f, 3.5f };

	[Header("Debug")]
	[SerializeField] private bool debugLogs = true;

	void Awake()
	{
		if (!dash) dash = GetComponent<DashAbility>();
		if (!handler)
		{
			handler = GetComponent<UpgradeHandler>();
			if (!handler)
			{
				var p = GameObject.FindGameObjectWithTag("Player");
				if (p) handler = p.GetComponent<UpgradeHandler>();
			}
		}

		if (!dash)
		{
			Debug.LogError("[DashRingSpawner] Missing DashAbility; disabling.", this);
			enabled = false;
			return;
		}

		if (!handler)
			Debug.LogWarning("[DashRingSpawner] No UpgradeHandler found (levels default to 0).", this);
	}

	void OnEnable()
	{
		// we only need the finished event now (no sadness trail)
		dash.OnDashFinished += OnDashFinished;
	}

	void OnDisable()
	{
		dash.OnDashFinished -= OnDashFinished;
	}

	private void OnDashFinished(Vector3 start, Vector3 end)
	{
		// Always spawn the ring at start
		if (ringPrefab)
		{
			var ring = Instantiate(ringPrefab, start, Quaternion.identity);
			ring.Spawn(2f, ringColor, ringLifetime);
			if (debugLogs) Debug.Log($"[DashRingSpawner] [Ring] Spawned @ {start}", ring);
		}

		// Log current levels
		if (debugLogs && handler)
		{
			Debug.Log($"[DashRingSpawner] [Levels] Anger:{handler.dashAngerLvl} Joy:{handler.dashJoyLvl} Sad:{handler.dashSadnessLvl} Love:{handler.dashLoveLvl} Fear:{handler.dashFearLvl}", this);
		}

		// ---------- Anger @ START ----------
		{
			int lvl = GetLvl(handler?.dashAngerLvl ?? 0);
			if (lvl > 0 && angerBurstPrefab)
			{
				float pct = anger_aoePercent[Mathf.Clamp(lvl, 0, anger_aoePercent.Length - 1)];
				var burst = Instantiate(angerBurstPrefab, start, Quaternion.identity);
				burst.Configure(pct);
				if (debugLogs) Debug.Log($"[DashRingSpawner] [ANGER] BurstZone spawned @ {start} | +{pct * 100f:F0}% dmg scale | level={lvl}", burst);
			}
		}

		// ---------- Joy @ START ----------
		{
			int lvl = GetLvl(handler?.dashJoyLvl ?? 0);
			if (lvl > 0 && joyCritPrefab)
			{
				float ch = joy_chance[Mathf.Clamp(lvl, 0, joy_chance.Length - 1)];
				float rad = joy_radius[Mathf.Clamp(lvl, 0, joy_radius.Length - 1)];
				float life = joy_duration[Mathf.Clamp(lvl, 0, joy_duration.Length - 1)];
				var zone = Instantiate(joyCritPrefab, start, Quaternion.identity);
				zone.Configure(ch, rad, life);
				if (debugLogs) Debug.Log($"[DashRingSpawner] [JOY] CritZone spawned @ {start} | +crit={ch:P0} | radius={rad:F1} | life={life:F1}s | level={lvl}", zone);
			}
		}

		// ---------- Sadness @ START (single spawn) ----------
		{
			int lvl = GetLvl(handler?.dashSadnessLvl ?? 0);
			if (lvl > 0 && slowZonePrefab)
			{
				float slowPct = sadness_slowPercent[Mathf.Clamp(lvl, 0, sadness_slowPercent.Length - 1)];
				float slowSec = sadness_slowSeconds[Mathf.Clamp(lvl, 0, sadness_slowSeconds.Length - 1)];
				var z = Instantiate(slowZonePrefab, start, Quaternion.identity);
				z.Configure(slowPct, slowSec);
				if (debugLogs) Debug.Log($"[DashRingSpawner] [SADNESS] SlowZone spawned @ {start} | slow={slowPct:P0} | sec={slowSec:F1} | level={lvl}", z);
			}
		}

		// ---------- Love @ START ----------
		{
			int lvl = GetLvl(handler?.dashLoveLvl ?? 0);
			if (lvl > 0 && weaknessZonePrefab)
			{
				float pct = love_weaknessPercent[Mathf.Clamp(lvl, 0, love_weaknessPercent.Length - 1)];
				float dur = love_duration[Mathf.Clamp(lvl, 0, love_duration.Length - 1)];
				float rad = love_radius[Mathf.Clamp(lvl, 0, love_radius.Length - 1)];
				var wz = Instantiate(weaknessZonePrefab, start, Quaternion.identity);
				wz.Configure(pct, dur, rad);
				if (debugLogs) Debug.Log($"[DashRingSpawner] [LOVE] WeaknessZone spawned @ {start} | -dmg={pct:P0} | radius={rad:F1} | sec={dur:F1} | level={lvl}", wz);
			}
		}

		// ---------- Fear @ START (single instance globally) ----------
		{
			int lvl = GetLvl(handler?.dashFearLvl ?? 0);
			if (lvl > 0 && fearZonePrefab)
			{

				float sec = fear_seconds[Mathf.Clamp(lvl, 0, fear_seconds.Length - 1)];
				float rad = fear_radius[Mathf.Clamp(lvl, 0, fear_radius.Length - 1)];
				var fz = Instantiate(fearZonePrefab, start, Quaternion.identity);
				fz.Configure(sec, rad, transform);
				if (debugLogs) Debug.Log($"[DashRingSpawner] [FEAR] FearZone spawned @ {start} | sec={sec:F1} | radius={rad:F1} | level={lvl}", fz);

				else if (debugLogs)
				{
					Debug.Log("[DashRingSpawner] [FEAR] Skipped: FearZone already active.", this);
				}
			}
		}
	}

	private static int GetLvl(int raw) => Mathf.Clamp(raw, 0, 3);
}
