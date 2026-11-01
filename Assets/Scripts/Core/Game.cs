using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Liangshan
{
    public sealed class Game : MonoBehaviour
    {
        public static Game Instance { get; private set; }
        public WorldRefs World { get; private set; }
        public Camera GameCamera { get; private set; }
        public GameState State { get; private set; } = GameState.Boot;

        public CameraDirector CameraRig;
        public VFXManager Vfx;
        public AudioDirector Audio;
        public GameUI UI;
        public PlayerController Player;
        public EnemyController Boss;
        public readonly List<EnemyController> Enemies = new List<EnemyController>();
        public int SelectedHeroIndex;
        public int Wave;
        public int Score;
        public int Kills;
        public float RunTime;
        public bool WaveActive;

        public HudView Hud => UI != null ? UI.Hud : null;

        readonly List<ProceduralRig> _menuCast = new List<ProceduralRig>();
        readonly List<string> _boonNames = new List<string>();
        System.Random _random;
        Coroutine _spawnRoutine;
        Coroutine _hitStopRoutine;
        float _hitStopUntil;
        bool _spawning;
        bool _runEnded;
        List<BoonDefinition> _currentBoons;

        public void Initialize(WorldRefs world, Camera camera)
        {
            Instance = this;
            World = world;
            GameCamera = camera;
            _random = new System.Random(1080);

            CameraRig = camera.gameObject.AddComponent<CameraDirector>();
            CameraRig.Initialize(this, camera);

            var fxGo = new GameObject("VFX Director");
            fxGo.transform.SetParent(transform, false);
            Vfx = fxGo.AddComponent<VFXManager>();
            Vfx.Initialize(world.Effects);

            Audio = gameObject.AddComponent<AudioDirector>();
            Audio.Initialize();

            UI = gameObject.AddComponent<GameUI>();
            UI.Initialize(this);

            Time.timeScale = 1f;
            SelectedHeroIndex = 0;
            BuildMenuCast();
            ShowMenu();

            string[] args = Environment.GetCommandLineArgs();
            if (HasArg(args, "-shots"))
                StartCoroutine(ShotRoutine(GetArg(args, "-shots", "/tmp/liangshan-shots")));
            if (HasArg(args, "-autotest"))
                StartCoroutine(AutoTestRoutine(GetArg(args, "-shots", "/tmp/liangshan-shots")));
            if (HasArg(args, "-uitest"))
                StartCoroutine(UiTestRoutine(GetArg(args, "-uidir", "/tmp/liangshan-ui")));
            if (HasArg(args, "-flowtest"))
                StartCoroutine(FlowTestRoutine(GetArg(args, "-flowdir", "/tmp/liangshan-flow")));
            Debug.Log("[Liangshan] Game initialized. State=" + State);
        }

        static bool HasArg(string[] args, string arg)
        {
            for (int i = 0; i < args.Length; i++)
                if (string.Equals(args[i], arg, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        static string GetArg(string[] args, string arg, string fallback)
        {
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], arg, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            return fallback;
        }

        IEnumerator ShotRoutine(string directory)
        {
            Directory.CreateDirectory(directory);
            yield return new WaitForSeconds(1.8f);
            yield return Capture(Path.Combine(directory, "01-menu.png"));
            UI.ShowCodex();
            yield return new WaitForSeconds(0.8f);
            yield return Capture(Path.Combine(directory, "02-codex.png"));
            UI.HideCodex();
            SelectHero(1);
            yield return new WaitForSeconds(0.4f);
            StartRun();
            yield return new WaitForSeconds(5.0f);
            yield return Capture(Path.Combine(directory, "03-gameplay.png"));
            DebugSpawn(EnemyType.Blade, new Vector3(0f, 0f, -3.2f));
            DebugSpawn(EnemyType.Spear, new Vector3(-2.2f, 0f, -2.5f));
            yield return new WaitForSeconds(0.8f);
            Player.DebugAttack();
            yield return new WaitForSeconds(0.6f);
            Player.DebugAttack();
            yield return new WaitForSeconds(0.7f);
            yield return Capture(Path.Combine(directory, "04-combat.png"));
            Player.DebugSkill();
            yield return new WaitForSeconds(0.8f);
            yield return Capture(Path.Combine(directory, "05-skill.png"));
            Debug.Log("[Liangshan] Shots complete in " + directory);
        }


        IEnumerator UiTestRoutine(string directory)
        {
            Directory.CreateDirectory(directory);
            yield return new WaitForSeconds(1.2f);
            UI.Menu.Close();
            var boons = BoonDefinition.Roll(3, _random);
            UI.ShowBoons(boons, _ => { });
            yield return new WaitForSeconds(0.5f);
            yield return Capture(Path.Combine(directory, "ui-boon.png"));
            UI.HideBoons();
            UI.ShowPause();
            yield return new WaitForSeconds(0.5f);
            yield return Capture(Path.Combine(directory, "ui-pause.png"));
            UI.HidePause();
            UI.ShowEnd(true, 12400, 5, 428f, 37);
            yield return new WaitForSeconds(0.5f);
            yield return Capture(Path.Combine(directory, "ui-victory.png"));
            UI.ShowEnd(false, 3820, 3, 212f, 9);
            yield return new WaitForSeconds(0.5f);
            yield return Capture(Path.Combine(directory, "ui-defeat.png"));
            Debug.Log("[Liangshan] UI test complete");
        }

        IEnumerator FlowTestRoutine(string directory)
        {
            Directory.CreateDirectory(directory);
            yield return new WaitForSeconds(1.0f);
            StartRun();
            yield return new WaitForSeconds(4.0f);
            for (int w = 1; w <= 4; w++)
            {
                yield return new WaitForSeconds(1.8f);
                yield return Capture(Path.Combine(directory, "flow-wave-" + w + ".png"));
                for (int attempt = 0; attempt < 3 && State == GameState.Playing; attempt++)
                {
                    DebugKillAll();
                    yield return new WaitForSeconds(1.1f);
                }
                if (State == GameState.Boon)
                {
                    yield return Capture(Path.Combine(directory, "flow-boon-" + w + ".png"));
                    DebugChooseBoon(0);
                }
                yield return new WaitForSeconds(3.4f);
            }
            yield return new WaitForSeconds(3.2f);
            yield return Capture(Path.Combine(directory, "flow-boss.png"));
            DebugKillAll();
            yield return new WaitForSecondsRealtime(3.2f);
            yield return Capture(Path.Combine(directory, "flow-victory.png"));
            Debug.Log("[Liangshan] FLOWTEST state=" + State + " wave=" + Wave + " kills=" + Kills + " score=" + Score);
        }

        public void DebugKillAll()
        {
            var copy = new List<EnemyController>(Enemies);
            for (int i = 0; i < copy.Count; i++)
            {
                if (copy[i] == null || copy[i].Combat == null) continue;
                copy[i].Combat.TakeDamage(new DamageInfo { Amount = 999999f, Source = Player != null ? Player.Combat : null, Heavy = true });
            }
        }

        public void DebugChooseBoon(int index)
        {
            ChooseBoon(index);
        }

        IEnumerator AutoTestRoutine(string directory)
        {
            Directory.CreateDirectory(directory);
            yield return new WaitForSeconds(1.0f);
            StartRun();
            yield return new WaitForSeconds(3.6f);
            DebugSpawn(EnemyType.Blade, new Vector3(0f, 0f, -2.4f));
            DebugSpawn(EnemyType.Blade, new Vector3(1.6f, 0f, -2.0f));
            yield return new WaitForSeconds(0.8f);
            int before = Enemies.Count;
            Player.DebugAttack();
            yield return new WaitForSeconds(0.6f);
            Player.DebugAttack();
            yield return new WaitForSeconds(0.8f);
            Player.DebugSkill();
            yield return new WaitForSeconds(0.5f);
            Player.DebugUltimate();
            yield return new WaitForSeconds(2.0f);
            yield return Capture(Path.Combine(directory, "autotest.png"));
            Debug.Log("[Liangshan] AUTOTEST enemies=" + before + "->" + Enemies.Count + " health=" + Player.Combat.Health + " kills=" + Kills + " ult=" + Player.UltimateMeter);
        }

        IEnumerator Capture(string path)
        {
            ScreenCapture.CaptureScreenshot(path, 1);
            yield return new WaitForSeconds(1.0f);
            Debug.Log("[Liangshan] captured " + path);
        }

        public void DebugSpawn(EnemyType type, Vector3 offset)
        {
            if (Player == null) return;
            SpawnEnemy(type, Player.transform.position + offset);
        }

        void Update()
        {
            if (State == GameState.Playing)
            {
                RunTime += Time.deltaTime;
                Hud?.SetTime(RunTime);
                Hud?.SetEnemies(Enemies.Count);
                if (Boss != null) Hud?.UpdateBoss(Boss);
                if (WaveActive && !_spawning && Enemies.Count == 0)
                    CheckWaveComplete();
            }
            else if (State == GameState.Menu)
            {
                bool codex = UI != null && UI.IsCodexOpen;
                if (codex)
                {
                    if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
                    {
                        UI.HideCodex();
                        Audio.PlayClick();
                    }
                    return;
                }

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    UI.ShowCodex();
                    Audio.PlayClick();
                    return;
                }
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                    SelectHero((SelectedHeroIndex + HeroDatabase.Playable.Count - 1) % HeroDatabase.Playable.Count);
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                    SelectHero((SelectedHeroIndex + 1) % HeroDatabase.Playable.Count);
                if (Input.GetKeyDown(KeyCode.Alpha1)) SelectHero(0);
                if (Input.GetKeyDown(KeyCode.Alpha2)) SelectHero(1);
                if (Input.GetKeyDown(KeyCode.Alpha3)) SelectHero(2);
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    StartRun();
            }
            else if (State == GameState.Boon)
            {
                if (InputReader.AnyNumberDown(out int number) && _currentBoons != null && number <= _currentBoons.Count)
                    ChooseBoon(number - 1);
            }
            else if (State == GameState.Paused)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    TogglePause();
            }
            else if (State == GameState.Victory || State == GameState.Defeat)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                    RestartRun();
            }
        }

        public void SelectHero(int index)
        {
            if (State != GameState.Menu || (UI != null && UI.IsCodexOpen)) return;
            SelectedHeroIndex = Mathf.Clamp(index, 0, HeroDatabase.Playable.Count - 1);
            UI.Menu.Select(SelectedHeroIndex);
            for (int i = 0; i < _menuCast.Count; i++)
            {
                if (_menuCast[i] == null) continue;
                bool selected = i == SelectedHeroIndex;
                _menuCast[i].transform.localScale = Vector3.one * (selected ? 1.035f : 0.975f);
                _menuCast[i].transform.localRotation = Quaternion.Euler(0f, selected ? 0f : (i < SelectedHeroIndex ? 8f : -8f), 0f);
            }
            Audio.PlayClick();
        }

        void ShowMenu()
        {
            State = GameState.Menu;
            Time.timeScale = 1f;
            InputReader.Enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            CameraRig.SetTarget(null, true);
            UI.ShowMenu(SelectedHeroIndex);
            SelectHero(SelectedHeroIndex);
        }

        void BuildMenuCast()
        {
            for (int i = 0; i < _menuCast.Count; i++)
                if (_menuCast[i] != null) Destroy(_menuCast[i].gameObject);
            _menuCast.Clear();
            if (World == null) return;
            for (int i = 0; i < HeroDatabase.Playable.Count && i < World.MenuHeroSpots.Count; i++)
            {
                var hero = HeroDatabase.Playable[i];
                var spot = World.MenuHeroSpots[i];
                var rig = CharacterFactory.BuildHero(spot, hero, Vector3.zero, Quaternion.identity);
                rig.transform.localPosition = Vector3.zero;
                rig.transform.localRotation = Quaternion.Euler(0f, i == 0 ? 5f : (i == 2 ? -5f : 0f), 0f);
                rig.transform.localScale = Vector3.one;
                rig.SetLocomotion(0f);
                _menuCast.Add(rig);
            }
        }

        public void StartRun()
        {
            if (State != GameState.Menu || (UI != null && UI.IsCodexOpen)) return;
            StartCoroutine(StartRunRoutine());
        }

        IEnumerator StartRunRoutine()
        {
            State = GameState.Boot;
            Audio.PlayClick();
            UI.Menu.Close();
            for (int i = 0; i < _menuCast.Count; i++)
                if (_menuCast[i] != null) Destroy(_menuCast[i].gameObject);
            _menuCast.Clear();
            ClearEnemies();

            Score = 0;
            Kills = 0;
            Wave = 0;
            RunTime = 0f;
            _runEnded = false;
            _boonNames.Clear();
            Boss = null;
            Hud?.SetBoss(null);
            Hud?.SetScore(0);

            var playerGo = new GameObject("Player " + HeroDatabase.Playable[SelectedHeroIndex].Name);
            playerGo.transform.SetParent(World.Characters, false);
            Player = playerGo.AddComponent<PlayerController>();
            Player.Initialize(this, HeroDatabase.Playable[SelectedHeroIndex]);
            CameraRig.SetTarget(Player.transform, false);
            CameraRig.SnapBehind();

            UI.ShowGameplay();
            Vfx.SetSnow(true);
            State = GameState.Playing;
            InputReader.Enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            Hud?.SetWave(1, 5);
            Hud?.SetEnemies(0);
            Hud?.ShowWaveBanner("梁 山 聚 义", "高俅引兵来犯 · 守住忠义堂", 2.2f);
            Audio.PlayWave();
            yield return new WaitForSeconds(2.5f);
            if (State == GameState.Playing)
                StartWave(1);
        }

        void StartWave(int wave)
        {
            Wave = wave;
            WaveActive = true;
            Hud?.SetWave(wave, 5);
            Hud?.ShowWaveBanner("第 " + GameConfig.ChineseNumber(wave) + " 波", wave == 5 ? "高俅亲至 · 死战不退" : "官兵压境 · 替天行道", 2.2f);
            Audio.PlayWave();
            _spawnRoutine = StartCoroutine(SpawnWaveRoutine(WaveComposition(wave)));
        }

        List<EnemyType> WaveComposition(int wave)
        {
            var list = new List<EnemyType>();
            switch (wave)
            {
                case 1:
                    for (int i = 0; i < 6; i++) list.Add(EnemyType.Blade);
                    break;
                case 2:
                    for (int i = 0; i < 5; i++) list.Add(EnemyType.Blade);
                    for (int i = 0; i < 3; i++) list.Add(EnemyType.Spear);
                    break;
                case 3:
                    for (int i = 0; i < 4; i++) list.Add(EnemyType.Blade);
                    for (int i = 0; i < 3; i++) list.Add(EnemyType.Shield);
                    for (int i = 0; i < 3; i++) list.Add(EnemyType.Archer);
                    break;
                case 4:
                    for (int i = 0; i < 4; i++) list.Add(EnemyType.Blade);
                    for (int i = 0; i < 3; i++) list.Add(EnemyType.Spear);
                    for (int i = 0; i < 2; i++) list.Add(EnemyType.Shield);
                    for (int i = 0; i < 2; i++) list.Add(EnemyType.Archer);
                    list.Add(EnemyType.Captain);
                    break;
                default:
                    list.Add(EnemyType.Boss);
                    for (int i = 0; i < 4; i++) list.Add(EnemyType.Blade);
                    for (int i = 0; i < 2; i++) list.Add(EnemyType.Archer);
                    list.Add(EnemyType.Captain);
                    break;
            }
            return list;
        }

        IEnumerator SpawnWaveRoutine(List<EnemyType> composition)
        {
            _spawning = true;
            yield return new WaitForSeconds(0.35f);
            for (int i = 0; i < composition.Count; i++)
            {
                if (_runEnded) break;
                EnemyType type = composition[i];
                SpawnEnemy(type, SpawnPosition(type));
                if (type == EnemyType.Boss)
                    yield return new WaitForSeconds(1.1f);
                else
                    yield return new WaitForSeconds(0.38f);
            }
            _spawning = false;
            _spawnRoutine = null;
            if (WaveActive && Enemies.Count == 0)
                CheckWaveComplete();
        }

        Vector3 SpawnPosition(EnemyType type)
        {
            Vector3 p = World.RandomSpawn();
            float radius = type == EnemyType.Boss ? 1.2f : UnityEngine.Random.Range(1.5f, 4.2f);
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            p += new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Vector2 flat = new Vector2(p.x, p.z);
            if (flat.magnitude > GameConfig.PlayerBoundary - 1f)
                flat = flat.normalized * (GameConfig.PlayerBoundary - 1f);
            p.x = flat.x;
            p.z = flat.y;
            p.y = 0f;
            return p;
        }

        public void SpawnEnemy(EnemyType type, Vector3 position)
        {
            EnemyDefinition def = EnemyDefinition.Get(type);
            Vector3 toPlayer = Player != null ? Player.transform.position - position : Vector3.back;
            toPlayer.y = 0f;
            Quaternion rotation = toPlayer.sqrMagnitude > 0.01f ? Quaternion.LookRotation(toPlayer.normalized, Vector3.up) : Quaternion.identity;
            var rig = CharacterFactory.BuildEnemy(World.Characters, def, position, rotation);
            var enemy = rig.gameObject.AddComponent<EnemyController>();
            enemy.Initialize(def, Wave);
            Enemies.Add(enemy);
            Vfx.InkBurst(position + Vector3.up * 0.2f, def.Secondary, def.IsBoss ? 3f : 0.9f);
            Audio.PlaySpawn(position);
            if (def.IsBoss)
            {
                Boss = enemy;
                Hud?.SetBoss(enemy);
                Hud?.ShowWaveBanner("高 俅", "殿前太尉 · 祸乱朝纲", 2.8f);
            }
        }

        public void SpawnAdds(int count, Vector3 center)
        {
            StartCoroutine(SpawnAddsRoutine(count, center));
        }

        IEnumerator SpawnAddsRoutine(int count, Vector3 center)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2f / count + UnityEngine.Random.Range(-0.3f, 0.3f);
                Vector3 p = center + new Vector3(Mathf.Cos(angle) * 3.2f, 0f, Mathf.Sin(angle) * 3.2f);
                SpawnEnemy(i == count - 1 ? EnemyType.Shield : EnemyType.Blade, p);
                yield return new WaitForSeconds(0.18f);
            }
        }

        public void OnEnemyKilled(EnemyController enemy)
        {
            if (enemy == null) return;
            Enemies.Remove(enemy);
            Kills++;
            int points = enemy.IsBoss ? 2000 : (enemy.Definition.Type == EnemyType.Captain ? 250 : 100);
            Score += points;
            Hud?.SetScore(Score);
            Hud?.SetEnemies(Enemies.Count);
            Player?.AddUltimate(enemy.IsBoss ? 25f : 3f);
            if (enemy == Boss)
            {
                Boss = null;
                Hud?.SetBoss(null);
                if (!_runEnded) StartCoroutine(VictoryRoutine());
            }
            else if (WaveActive && !_spawning && Enemies.Count == 0)
            {
                CheckWaveComplete();
            }
        }

        void CheckWaveComplete()
        {
            if (_runEnded || !WaveActive || _spawning || Enemies.Count > 0) return;
            WaveActive = false;
            if (Wave >= 5)
            {
                if (!_runEnded) StartCoroutine(VictoryRoutine());
                return;
            }
            StartCoroutine(BoonRoutine());
        }

        IEnumerator BoonRoutine()
        {
            WaveActive = false;
            yield return new WaitForSeconds(0.8f);
            if (_runEnded) yield break;
            State = GameState.Boon;
            InputReader.Enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;
            _currentBoons = BoonDefinition.Roll(3, _random);
            UI.ShowBoons(_currentBoons, ChooseBoon);
            Audio.PlayBoon();
        }

        void ChooseBoon(int index)
        {
            if (State != GameState.Boon || _currentBoons == null || index < 0 || index >= _currentBoons.Count) return;
            var boon = _currentBoons[index];
            _currentBoons = null;
            _boonNames.Add(boon.Name);
            boon.Apply(Player.Boons);
            Player.ApplyBoons(boon.Id == "iron");
            Hud?.SetBoons(string.Join("  ·  ", _boonNames));
            UI.HideBoons();
            Audio.PlayBoon();
            State = GameState.Playing;
            InputReader.Enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Hud?.ShowWaveBanner("机 缘 加 身", boon.Name + " · " + boon.Description, 1.8f);
            StartCoroutine(NextWaveRoutine());
        }

        IEnumerator NextWaveRoutine()
        {
            yield return new WaitForSeconds(1.4f);
            if (State == GameState.Playing && !_runEnded)
                StartWave(Wave + 1);
        }

        public void OnBossSpawned(EnemyController boss)
        {
            Boss = boss;
            Hud?.SetBoss(boss);
        }

        public void OnPlayerDamaged(float amount)
        {
            Hud?.FlashInk(Mathf.Clamp01(amount / 40f) * 1.2f);
        }

        public void OnPlayerDeath()
        {
            if (_runEnded) return;
            _runEnded = true;
            State = GameState.Defeat;
            InputReader.Enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0.35f;
            Audio.PlayDefeat();
            CameraRig.Shake(0.8f);
            StartCoroutine(DefeatRoutine());
        }

        IEnumerator DefeatRoutine()
        {
            yield return new WaitForSecondsRealtime(1.6f);
            Time.timeScale = 0f;
            UI.ShowEnd(false, Score, Mathf.Max(1, Wave), RunTime, Kills);
        }

        IEnumerator VictoryRoutine()
        {
            if (_runEnded) yield break;
            _runEnded = true;
            State = GameState.Victory;
            InputReader.Enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0.35f;
            Audio.PlayVictory();
            if (Player != null) Player.Rig.PlayVictory();
            CameraRig.Cinematic(3f, 0.8f);
            CameraRig.Shake(0.5f);
            yield return new WaitForSecondsRealtime(2.2f);
            Time.timeScale = 0f;
            UI.ShowEnd(true, Score, 5, RunTime, Kills);
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                State = GameState.Paused;
                Time.timeScale = 0f;
                InputReader.Enabled = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                UI.ShowPause();
                Audio.PlayPause();
            }
            else if (State == GameState.Paused)
            {
                State = GameState.Playing;
                Time.timeScale = 1f;
                InputReader.Enabled = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                UI.HidePause();
                Audio.PlayPause();
            }
        }

        public void RestartRun()
        {
            Time.timeScale = 1f;
            StopAllCoroutines();
            _spawnRoutine = null;
            _runEnded = false;
            _spawning = false;
            _hitStopRoutine = null;
            ClearRunObjects();
            UI.HideAll();
            State = GameState.Menu;
            StartRun();
        }

        public void QuitToMenu()
        {
            Time.timeScale = 1f;
            StopAllCoroutines();
            _spawnRoutine = null;
            _runEnded = false;
            _spawning = false;
            _hitStopRoutine = null;
            ClearRunObjects();
            UI.HideAll();
            State = GameState.Menu;
            BuildMenuCast();
            ShowMenu();
        }

        void ClearRunObjects()
        {
            if (Player != null)
            {
                Destroy(Player.gameObject);
                Player = null;
            }
            ClearEnemies();
            Boss = null;
            Hud?.SetBoss(null);
            for (int i = World.Characters.childCount - 1; i >= 0; i--)
                Destroy(World.Characters.GetChild(i).gameObject);
        }

        void ClearEnemies()
        {
            for (int i = 0; i < Enemies.Count; i++)
                if (Enemies[i] != null) Destroy(Enemies[i].gameObject);
            Enemies.Clear();
            _spawning = false;
            WaveActive = false;
        }

        public void HitStop(float duration)
        {
            if (State != GameState.Playing) return;
            _hitStopUntil = Mathf.Max(_hitStopUntil, Time.unscaledTime + duration);
            if (_hitStopRoutine == null)
                _hitStopRoutine = StartCoroutine(HitStopRoutine());
        }

        IEnumerator HitStopRoutine()
        {
            Time.timeScale = 0.055f;
            while (Time.unscaledTime < _hitStopUntil)
                yield return null;
            if (State == GameState.Playing) Time.timeScale = 1f;
            _hitStopRoutine = null;
        }
    }
}
