using System;
using System.Drawing;
using System.Linq;
using CaitlynTheTroll.Utility;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Activator = CaitlynTheTroll.Utility.Activator;

namespace CaitlynTheTroll
{
    public static class Program
    {
        public static string Version = "Version 1.6 27/10/2016";
        public static AIHeroClient Target = null;
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Targeted R;
        public static int CurrentSkin;

        public static readonly AIHeroClient Player = ObjectManager.Player;

   
        public static bool Boom()
        {
            return ObjectManager.Player.HasBuff("caitlynheadshot");
        }


        internal static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
            Bootstrap.Init(null);
        }


        private static void OnLoadingComplete(EventArgs args)
        {
            if (Player.ChampionName != "Caitlyn") return;
            Chat.Print(
                "<font color=\"#6909aa\" >MeLoSenpai Presents </font><font color=\"#fffffff\" >Caitlyn </font><font color=\"#6909aa\" >The Troll Ceviri TekinTR</font>");
            Chat.Print("Yuklenen versiyon 1.6 (27-10-2016)", Color.Red);
            CaitlynTheTrollMeNu.LoadMenu();
            Game.OnTick += GameOnTick;
            Activator.LoadSpells();
            Game.OnUpdate += OnGameUpdate;

            #region Skill

            Q = new Spell.Skillshot(SpellSlot.Q, 1200, SkillShotType.Linear, 625, 2200, 90)
            {
                AllowedCollisionCount = -1
            };
            W = new Spell.Skillshot(SpellSlot.W, 800, SkillShotType.Circular, 500, int.MaxValue, 20);
            E = new Spell.Skillshot(SpellSlot.E, 800, SkillShotType.Linear, 150, 1600, 80)
            {
                AllowedCollisionCount = 0
            };
            R = new Spell.Targeted(SpellSlot.R, 2000);

            #endregion

            Gapcloser.OnGapcloser += AntiGapCloser;
            Interrupter.OnInterruptableSpell += Interupt;
            Obj_AI_Base.OnBuffGain += OnBuffGain;
            Orbwalker.OnPostAttack += OnAfterAttack;
            Drawing.OnDraw += GameOnDraw;
            DamageIndicator.Initialize(SpellDamage.GetTotalDamage);
        }

        private static void GameOnDraw(EventArgs args)
        {
            if (CaitlynTheTrollMeNu.Nodraw()) return;

            {
                if (CaitlynTheTrollMeNu.DrawingsQ())
                {
                    new Circle {Color = Color.Purple, Radius = Q.Range, BorderWidth = 2f}.Draw(Player.Position);
                }
                if (CaitlynTheTrollMeNu.DrawingsW())
                {
                    new Circle {Color = Color.Purple, Radius = W.Range, BorderWidth = 2f}.Draw(Player.Position);
                }
                if (CaitlynTheTrollMeNu.DrawingsE())
                {
                    new Circle {Color = Color.Purple, Radius = E.Range, BorderWidth = 2f}.Draw(Player.Position);
                }
                if (CaitlynTheTrollMeNu.DrawingsR())
                {
                    new Circle {Color = Color.Purple, Radius = R.Range, BorderWidth = 2f}.Draw(Player.Position);
                }
                DamageIndicator.HealthbarEnabled =
                    CaitlynTheTrollMeNu.DrawMeNu["healthbar"].Cast<CheckBox>().CurrentValue;
                DamageIndicator.PercentEnabled = CaitlynTheTrollMeNu.DrawMeNu["percent"].Cast<CheckBox>().CurrentValue;
            }
        }

        private static
            void OnGameUpdate(EventArgs args)
        {
            if (Activator.Heal != null)
                Heal();
            if (Activator.Ignite != null)
                Ignite();
            if (CaitlynTheTrollMeNu.CheckSkin())
            {
                if (CaitlynTheTrollMeNu.SkinId() != CurrentSkin)
                {
                    Player.SetSkinId(CaitlynTheTrollMeNu.SkinId());
                    CurrentSkin = CaitlynTheTrollMeNu.SkinId();
                }
            }
        }


        private static void AntiGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (CaitlynTheTrollMeNu.GapcloserE() && sender.IsEnemy && sender.IsValidTarget(E.Range) &&
                e.End.Distance(Player) <= 250)
            {
                E.Cast(e.End);
            }
            if (CaitlynTheTrollMeNu.GapcloserW() && sender.IsEnemy && sender.IsValidTarget(W.Range) &&
                e.End.Distance(Player) <= 250)
            {
                W.Cast(e.End);
            }
        }

        public static void Interupt(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs interruptableSpellEventArgs)
        {
            if (!sender.IsEnemy) return;

            if (interruptableSpellEventArgs.DangerLevel == DangerLevel.Medium && CaitlynTheTrollMeNu.InterupteW() &&
                W.IsReady())
            {
                W.Cast(sender.Position);
            }
        }

        private static void Ignite()
        {
            var autoIgnite = TargetSelector.GetTarget(Activator.Ignite.Range, DamageType.True);
            if (autoIgnite != null && autoIgnite.Health <= Player.GetSpellDamage(autoIgnite, Activator.Ignite.Slot) ||
                autoIgnite != null && autoIgnite.HealthPercent <= CaitlynTheTrollMeNu.SpellsIgniteFocus())
                Activator.Ignite.Cast(autoIgnite);
        }

        private static void Heal()
        {
            if (Activator.Heal != null && Activator.Heal.IsReady() &&
                Player.HealthPercent <= CaitlynTheTrollMeNu.SpellsHealHp()
                && Player.CountEnemiesInRange(600) > 0 && Activator.Heal.IsReady())
            {
                Activator.Heal.Cast();
            }
        }

        private static void GameOnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                OnCombo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                OnHarrass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                OnLaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                OnJungle();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                OnFlee();
            }
            KillSteal();
            AutoCc();
            AutoPotions();
            UseEq();
            SmartR();
        }

        private static void UseEq()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target != null &&
                (CaitlynTheTrollMeNu.ComboEq() && E.IsReady() && Q.IsReady() &&
                 target.IsValid && !Player.HasBuff("CaitlynE")))
            {
                var predEq = E.GetPrediction(target);
                if (predEq.HitChance >= HitChance.Medium)
                {
                    E.Cast(predEq.CastPosition);
                    EloBuddy.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }
                var predQ = Q.GetPrediction(target);
                if (predQ.HitChance >= HitChance.High)
                {
                    Q.Cast(predQ.CastPosition);
                    EloBuddy.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }
            }
        }

        public static void OnAfterAttack(AttackableUnit target, EventArgs args)
        {
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    if (target == null || !(target is AIHeroClient) || target.IsDead || target.IsInvulnerable ||
                        !target.IsEnemy || target.IsPhysicalImmune || target.IsZombie)
                        return;

                var enemy = target as AIHeroClient;
                if (enemy == null)
                    return;
                if (CaitlynTheTrollMeNu.ComboQ() && CaitlynTheTrollMeNu.Aaq())
                {
                    if (Q.IsReady())
                    {
                        Q.Cast(enemy);
                    }
                    if (CaitlynTheTrollMeNu.ComboE() && CaitlynTheTrollMeNu.AaE())
                    {
                        if (E.IsReady())
                        {
                            E.Cast(enemy);
                        }
                    }
                }
            }
        }

        private static
            void AutoPotions()
        {
            if (CaitlynTheTrollMeNu.SpellsPotionsCheck() && !Player.IsInShopRange() &&
                Player.HealthPercent <= CaitlynTheTrollMeNu.SpellsPotionsHp() &&
                !(Player.HasBuff("RegenerationPotion") || Player.HasBuff("ItemCrystalFlaskJungle") ||
                  Player.HasBuff("ItemMiniRegenPotion") || Player.HasBuff("ItemCrystalFlask") ||
                  Player.HasBuff("ItemDarkCrystalFlask")))
            {
                if (Activator.HuntersPot.IsReady() && Activator.HuntersPot.IsOwned())
                {
                    Activator.HuntersPot.Cast();
                }
                if (Activator.CorruptPot.IsReady() && Activator.CorruptPot.IsOwned())
                {
                    Activator.CorruptPot.Cast();
                }
                if (Activator.Biscuit.IsReady() && Activator.Biscuit.IsOwned())
                {
                    Activator.Biscuit.Cast();
                }
                if (Activator.HpPot.IsReady() && Activator.HpPot.IsOwned())
                {
                    Activator.HpPot.Cast();
                }
                if (Activator.RefillPot.IsReady() && Activator.RefillPot.IsOwned())
                {
                    Activator.RefillPot.Cast();
                }
            }
            if (CaitlynTheTrollMeNu.SpellsPotionsCheck() && !Player.IsInShopRange() &&
                Player.ManaPercent <= CaitlynTheTrollMeNu.SpellsPotionsM() &&
                !(Player.HasBuff("RegenerationPotion") || Player.HasBuff("ItemCrystalFlaskJungle") ||
                  Player.HasBuff("ItemMiniRegenPotion") || Player.HasBuff("ItemCrystalFlask") ||
                  Player.HasBuff("ItemDarkCrystalFlask")))
            {
                if (Activator.CorruptPot.IsReady() && Activator.CorruptPot.IsOwned())
                {
                    Activator.CorruptPot.Cast();
                }
            }
        }

        private static void OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            if (!sender.IsMe) return;

            if (args.Buff.Type == BuffType.Taunt && CaitlynTheTrollMeNu.Taunt())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Stun && CaitlynTheTrollMeNu.Stun())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Snare && CaitlynTheTrollMeNu.Snare())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Polymorph && CaitlynTheTrollMeNu.Polymorph())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Blind && CaitlynTheTrollMeNu.Blind())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Flee && CaitlynTheTrollMeNu.Fear())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Charm && CaitlynTheTrollMeNu.Charm())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Suppression && CaitlynTheTrollMeNu.Suppression())
            {
                DoQss();
            }
            if (args.Buff.Type == BuffType.Silence && CaitlynTheTrollMeNu.Silence())
            {
                DoQss();
            }
        }

        private static void DoQss()
        {
            if (Activator.Qss.IsOwned() && Activator.Qss.IsReady())
            {
                Activator.Qss.Cast();
            }

            if (Activator.Mercurial.IsOwned() && Activator.Mercurial.IsReady())
            {
                Activator.Mercurial.Cast();
            }
        }

        private static void KillSteal()
        {
            var enemies = EntityManager.Heroes.Enemies.OrderByDescending
                (a => a.HealthPercent)
                .Where(
                    a =>
                        !a.IsMe && a.IsValidTarget() && a.Distance(Player) <= Q.Range && !a.IsDead && !a.IsZombie &&
                        a.HealthPercent <= 35);
            foreach (
                var target in
                    enemies)
            {
                if (!target.IsValidTarget())
                {
                    return;
                }

                if (CaitlynTheTrollMeNu.KillstealQ() && target.Distance(Player) >= 615 && Q.IsReady() &&
                    target.Health + target.AttackShield <
                    Player.GetSpellDamage(target, SpellSlot.Q))
                {
                    var predQks = (Q.GetPrediction(target));
                    if (predQks.HitChance >= HitChance.High)
                    {
                        Q.Cast(predQks.CastPosition);
                    }
                }
            }
        }

        private static
            void AutoCc()
        {
            if (!CaitlynTheTrollMeNu.ComboMenu["combo.CC"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }
            var autoTarget =
                EntityManager.Heroes.Enemies.FirstOrDefault(
                    x =>
                        x.HasBuffOfType(BuffType.Charm) || x.HasBuffOfType(BuffType.Knockup) ||
                        x.HasBuffOfType(BuffType.Stun) || x.HasBuffOfType(BuffType.Suppression) ||
                        x.HasBuffOfType(BuffType.Snare) || x.HasBuff("zhonyasringshield") || x.HasBuff("Recall")
                        || x.HasBuff("teleport") || x.HasBuff("crowstorm") || x.HasBuff("katarinar") ||
                        x.HasBuff("MissFortuneBulletTime"));
            if (autoTarget != null)
            {
                E.Cast(autoTarget.ServerPosition);
            }
            if (!CaitlynTheTrollMeNu.ComboMenu["combo.CCQ"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }
            if (autoTarget != null)
            {
                Q.Cast(autoTarget.ServerPosition);
            }
            if (!CaitlynTheTrollMeNu.ComboMenu["combo.CCW"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }
            if (autoTarget != null)
            {
                W.Cast(autoTarget.ServerPosition);
            }
        }

        private static void OnFlee()
        {
            if (E.IsReady() && CaitlynTheTrollMeNu.UseEmouse())
            {
                E.Cast(Game.CursorPos.Extend(Player.Position, Player.Distance(Game.CursorPos) + 250).To3D());
            }
        }


        private static void OnLaneClear()
        {
            if (CaitlynTheTrollMeNu.LaneQ() && Player.ManaPercent > CaitlynTheTrollMeNu.LaneMana())
            {
                var minions =
                    EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                        t =>
                            t.IsEnemy && !t.IsDead && t.IsValid && !t.IsInvulnerable &&
                            t.IsInRange(Player.Position, Q.Range));
                foreach (var m in minions)
                {
                    if (
                        Q.GetPrediction(m)
                            .CollisionObjects.Where(t => t.IsEnemy && !t.IsDead && t.IsValid && !t.IsInvulnerable)
                            .Count() >= 0)
                    {
                        Q.Cast(m);
                        break;
                    }
                }
            }
        }


        private static
            void OnJungle()
        {
            if (CaitlynTheTrollMeNu.JungleQ())
            {
                var minions =
                    EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Position, Q.Range)
                        .Where(t => !t.IsDead && t.IsValid && !t.IsInvulnerable);
                if (minions.Count() > 0)
                {
                    Q.Cast(minions.First());
                }
            }
        }

        private static void OnHarrass()
        {
            var enemies = EntityManager.Heroes.Enemies.OrderByDescending
                (a => a.HealthPercent).Where(a => !a.IsMe && a.IsValidTarget() && a.Distance(Player) <= Q.Range);
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (!target.IsValidTarget())
            {
                return;
            }

            if (Q.IsReady() && target.IsValidTarget(700))
                foreach (var eenemies in enemies)
                {
                    var useQ = CaitlynTheTrollMeNu.HarassMeNu["harass.Q"
                                                              + eenemies.ChampionName].Cast<CheckBox>().CurrentValue;
                    if (useQ && Player.ManaPercent >= CaitlynTheTrollMeNu.HarassQe())
                    {
                        var predQharass = Q.GetPrediction(target);
                        if (predQharass.HitChance >= HitChance.High)
                        {
                            Q.Cast(predQharass.CastPosition);
                        }
                    }
                }
            if (W.IsReady() && target.IsValidTarget(W.Range) && CaitlynTheTrollMeNu.HarassW() &&
                Player.ManaPercent >= CaitlynTheTrollMeNu.HarassQe())
            {
                var predWharass = W.GetPrediction(target);
                if (predWharass.HitChance >= HitChance.High)
                {
                    W.Cast(predWharass.CastPosition);
                }
            }
        }

        private static
            void SmartR()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            if (!target.IsValidTarget(R.Range) || target == null)
            {
                return;
            }
            var rdamage = Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (new float[] {0, 250, 475, 700}[R.Level] + (2f*Player.FlatMagicDamageMod))) -
                          target.FlatPhysicalReduction;
            if (R.IsReady() && CaitlynTheTrollMeNu.ComboR() && Player.CountEnemiesInRange(R.Range) == 1 &&
                target.Distance(Player) >= 800 && rdamage >= target.Health && !target.IsInvulnerable)
            {
                R.Cast(target);
            }
        }


        private static
            void OnCombo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (!target.IsValidTarget(Q.Range) || target == null)
            {
                return;
            }
            if (CaitlynTheTrollMeNu.ComboE() && CaitlynTheTrollMeNu.LogicE() && !Boom())
            {
                if (E.IsReady() && E.CanCast(target))
                {
                    var predE = E.GetPrediction(target);
                    if (predE.HitChancePercent >= CaitlynTheTrollMeNu.PredE())
                    {
                        E.Cast(predE.CastPosition);
                    }
                    else
                    {
                        if (target.IsValidTarget(200))
                        {
                            E.Cast(predE.CastPosition);
                        }
                    }
                }
            }
            if (CaitlynTheTrollMeNu.ComboQ() && CaitlynTheTrollMeNu.LogicQ() && !Boom())
            {
                if (Q.IsReady() && Q.CanCast(target))
                {
                    var predq = W.GetPrediction(target);
                    if (predq.HitChancePercent >= CaitlynTheTrollMeNu.PredQ())
                    {
                        Q.Cast(predq.CastPosition);
                    }
                }
            }
            if (CaitlynTheTrollMeNu.ComboW() && W.Handle.Ammo >= CaitlynTheTrollMeNu.LimitTrap())
            {
                if (W.IsReady())
                {
                    var predictionPos = Prediction.Position.PredictUnitPosition(target, 500).To3D();

                    if (W.IsInRange(predictionPos))
                    {
                        W.Cast(predictionPos);
                    }
                }
            }
            if ((ObjectManager.Player.CountEnemiesInRange(ObjectManager.Player.AttackRange) >=
                 CaitlynTheTrollMeNu.YoumusEnemies() ||
                 Player.HealthPercent >= CaitlynTheTrollMeNu.ItemsYoumuShp()) &&
                Activator.Youmus.IsReady() && CaitlynTheTrollMeNu.Youmus() && Activator.Youmus.IsOwned())
            {
                Activator.Youmus.Cast();
                return;
            }
            if (Player.HealthPercent <= CaitlynTheTrollMeNu.BilgewaterHp() &&
                CaitlynTheTrollMeNu.Bilgewater() &&
                Activator.Bilgewater.IsReady() && Activator.Bilgewater.IsOwned())
            {
                Activator.Bilgewater.Cast(target);
                return;
            }

            if (Player.HealthPercent <= CaitlynTheTrollMeNu.BotrkHp() && CaitlynTheTrollMeNu.Botrk() &&
                Activator.Botrk.IsReady() &&
                Activator.Botrk.IsOwned())
            {
                Activator.Botrk.Cast(target);
            }
        }
    }
}