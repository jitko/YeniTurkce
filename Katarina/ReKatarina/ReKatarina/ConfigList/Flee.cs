﻿using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace ReKatarina.ConfigList
{
    public static class Flee
    {
        public static readonly Menu Menu;
        private static readonly CheckBox _JumpToAlly;
        private static readonly CheckBox _JumpToDagger;
        private static readonly CheckBox _JumpToAllyMinion;
        private static readonly CheckBox _JumpToEnemyMinion;
        private static readonly CheckBox _JumpToMonster;
        private static readonly Slider _JumpToMonsterHP;
        private static readonly Slider _JumpCursorRange;

        public static bool JumpToAlly { get { return _JumpToAlly.CurrentValue; } }
        public static bool JumpToDagger { get { return _JumpToDagger.CurrentValue; } }
        public static bool JumpToAllyMinion { get { return _JumpToAllyMinion.CurrentValue; } }
        public static bool JumpToEnemyMinion { get { return _JumpToEnemyMinion.CurrentValue; } }
        public static bool JumpToMonster { get { return _JumpToMonster.CurrentValue; } }
        public static int JumpToMonsterHP { get { return _JumpToMonsterHP.CurrentValue; } }
        public static int JumpCursorRange { get { return _JumpCursorRange.CurrentValue; } }
        

        static Flee()
        {
            Menu = Config.Menu.AddSubMenu("Flee");
            Menu.AddGroupLabel("Flee settings");
            _JumpToAlly = Menu.Add("Flee.Ally", new CheckBox("Muttefiklere atla."));
            _JumpToDagger = Menu.Add("Flee.Dagger", new CheckBox("Bicaga atla."));
            _JumpToAllyMinion = Menu.Add("Flee.AllyMinion", new CheckBox("Dost minyona atla."));
            _JumpToEnemyMinion = Menu.Add("Flee.EnemyMinion", new CheckBox("Dusman minyona atla."));
            _JumpToMonster = Menu.Add("Flee.Monster", new CheckBox("Canavarlara atla."));
            _JumpToMonsterHP = Menu.Add("Flee.MonsterHP", new Slider("Canim sundan dusukse canavara atla HP > {0}%", 15, 1, 100));
            _JumpCursorRange = Menu.Add("Flee.CursorRange", new Slider("Imlecin hedef algilama araliginda atla. ", 200, 100, 250));
        }

        public static void Initialize()
        {
        }
    }
}