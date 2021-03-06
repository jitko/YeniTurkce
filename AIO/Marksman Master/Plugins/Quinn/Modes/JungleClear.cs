﻿#region Licensing
// ---------------------------------------------------------------------
// <copyright file="JungleClear.cs" company="EloBuddy">
// 
// Marksman Master
// Copyright (C) 2016 by gero
// All rights reserved
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
// </copyright>
// <summary>
// 
// Email: geroelobuddy@gmail.com
// PayPal: geroelobuddy@gmail.com
// </summary>
// ---------------------------------------------------------------------
#endregion
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;

namespace Marksman_Master.Plugins.Quinn.Modes
{
    internal class JungleClear : Quinn
    {
        public static void Execute()
        {
            if (!Settings.LaneClear.UseQInJungleClear || !Q.IsReady() ||
                !(Player.Instance.ManaPercent >= Settings.LaneClear.MinManaQ) || !IsAfterAttack)
                return;

            var jungleMinions = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, Q.Range).ToList();

            if (!jungleMinions.Any())
                return;

            string[] allowedMonsters =
            {
                "SRU_Gromp", "SRU_Blue", "SRU_Red", "SRU_Razorbeak", "SRU_Krug", "SRU_Murkwolf", "Sru_Crab",
                "SRU_RiftHerald", "SRU_Dragon_Fire", "SRU_Dragon_Earth", "SRU_Dragon_Air", "SRU_Dragon_Elder",
                "SRU_Dragon_Water", "SRU_Baron"
            };

            var farmLocation = EntityManager.MinionsAndMonsters.GetCircularFarmLocation(jungleMinions, 410, (int)Q.Range,
                Player.Instance.ServerPosition.To2D());

            if (farmLocation.HitNumber > 2)
            {
                Q.Cast(farmLocation.CastPosition);
                return;
            }

            if (jungleMinions.Count == 1 && jungleMinions.Any(b => allowedMonsters.Any(x => x.Contains(b.BaseSkinName))))
            {
                Q.Cast(jungleMinions.First());
            }
        }
    }
}
