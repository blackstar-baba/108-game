using System;
using System.Collections.Generic;
using UnityEngine;

namespace Liangshan
{
    public sealed class GameUI : MonoBehaviour
    {
        public Canvas Canvas;
        public MenuView Menu;
        public HudView Hud;
        public BoonView Boons;
        public PauseView Pause;
        public EndView End;
        public CodexView Codex;

        Game _game;

        public bool IsCodexOpen => Codex != null && Codex.IsOpen;

        public void Initialize(Game game)
        {
            _game = game;
            Canvas = UIKit.CreateCanvas(transform);
            Menu = gameObject.AddComponent<MenuView>();
            Menu.Initialize(game, Canvas.transform);
            Hud = gameObject.AddComponent<HudView>();
            Hud.Initialize(game, Canvas.transform);
            Boons = gameObject.AddComponent<BoonView>();
            Boons.Initialize(game, Canvas.transform);
            Pause = gameObject.AddComponent<PauseView>();
            Pause.Initialize(game, Canvas.transform);
            End = gameObject.AddComponent<EndView>();
            End.Initialize(game, Canvas.transform);
            Codex = gameObject.AddComponent<CodexView>();
            Codex.Initialize(game, Canvas.transform);
        }

        public void ShowMenu(int selected)
        {
            Codex.Hide();
            Pause.Hide();
            End.Hide();
            Boons.Hide();
            Hud.Hide();
            Menu.Open(selected);
        }

        public void ShowCodex()
        {
            if (IsCodexOpen) return;
            Codex.Show();
        }

        public void HideCodex()
        {
            Codex.Hide();
        }

        public void ShowGameplay()
        {
            Menu.Close();
            Codex.Hide();
            Pause.Hide();
            End.Hide();
            Boons.Hide();
            Hud.Show();
        }

        public void ShowBoons(List<BoonDefinition> boons, Action<int> callback)
        {
            Boons.Show(boons, callback);
        }

        public void HideBoons()
        {
            Boons.Hide();
        }

        public void ShowPause()
        {
            Pause.Show();
        }

        public void HidePause()
        {
            Pause.Hide();
        }

        public void ShowEnd(bool victory, int score, int waves, float seconds, int kills)
        {
            Hud.Hide();
            Pause.Hide();
            Boons.Hide();
            End.Show(victory, score, waves, seconds, kills);
        }

        public void HideAll()
        {
            Menu.Close();
            Hud.Hide();
            Boons.Hide();
            Pause.Hide();
            End.Hide();
            Codex.Hide();
        }
    }
}
