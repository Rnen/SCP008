﻿using Smod2.Commands;
using SCP008PLUGIN;
using System;
using Smod2.API;
using System.Collections.Generic;
using System.Linq;

namespace SCP008PLUGIN.Command
{
	/// <summary>
	/// Command for infecting <see cref="Player"/>s
	/// </summary>
	class InfectCommand : ICommandHandler
	{
		private readonly SCP008 plugin;
		private Server Server => plugin.PluginManager.Server;

		public InfectCommand(SCP008 plugin) => this.plugin = plugin;
		public string GetCommandDescription() => "Infects / removes infection";
		public string GetUsage() => "INFECT (PLAYER)";

		bool IsAllowed(ICommandSender sender)
		{
			//Checking if the ICommandSender is a player and setting the player variable if it is
			Player player = (sender is Player) ? sender as Player : null;

			//Checks if its a player, if not run the command as usual
			if (player != null)
			{
				//Making a list of all roles in config, converted to UPPERCASE
				string[] configList = plugin.GetConfigList(SCP008.ranksAllowedConfigKey);
				List<string> roleList = (configList != null && configList.Length > 0) ?
					configList.Select(role => role.ToUpper()).ToList() : new List<string>();

				//Checks if there is any entries, if empty, let anyone use it
				if (roleList != null && roleList.Count > 0
					&& (roleList.Contains(player.GetUserGroup().Name.ToUpper()) || roleList.Contains(player.GetRankName().ToUpper())))
				{
					//Config contained rank
					return true;
				}
				else if (roleList == null || roleList.Count == 0)
					return true; // config was empty
				else
					return false;
			}
			else
				return true;
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (IsAllowed(sender))
			{
				if (args.Length == 0 && sender is Player p)
					if (SCP008.playersToDamage.Contains(p.UserId))
					{
						SCP008.playersToDamage.Remove(p.UserId);
						return new string[] { "Cured infected " + p.Name };
					}
					else
					{
						SCP008.playersToDamage.Add(p.UserId);
						return new string[] { "Infected " + p.Name };
					}
				else if (args.Length > 0)
				{
					if (args[0].ToLower() == "all" || args[0] == "*")
					{
						int x = 0;
						foreach(Player pl in Server.GetPlayers()
							.Where(ply => 
							ply.TeamRole.Role != Smod2.API.RoleType.SPECTATOR &&
							ply.TeamRole.Role != Smod2.API.RoleType.UNASSIGNED &&
							ply.TeamRole.Role != Smod2.API.RoleType.ZOMBIE))
						{
							string arg = (args.Length > 1 && !string.IsNullOrEmpty(args[1])) ? args[1].ToLower() : "";
							if (SCP008.playersToDamage.Contains(pl.UserId) && arg != "infect")
								SCP008.playersToDamage.Remove(pl.UserId);
							else if(!SCP008.playersToDamage.Contains(pl.UserId) && arg != "infect")
									SCP008.playersToDamage.Add(pl.UserId);
							x++;
						}
						return new string[] { "Toggled infection on " + x + " players!" };
					}
					else
					{
						List<Player> players = Server.GetPlayers(args[0]);
						Player player;
						if (players == null || players.Count == 0) return new string[] { "No players on the server called " + args[0] };
						player = players.OrderBy(pl => pl.Name.Length).First();

						if (!SCP008.playersToDamage.Contains(player.UserId))
						{
							SCP008.playersToDamage.Add(player.UserId);
							return new string[] { "Infected " + player.Name };
						}
						else if (SCP008.playersToDamage.Contains(player.UserId))
						{
							SCP008.playersToDamage.Remove(player.UserId);
							return new string[] { "Cured infected " + player.Name };
						}
						else
							return new string[] { this.plugin.Details.id + " INFECT ERROR" };
					}
				}
				else
					return new string[] { GetUsage() };
			}
			else
				return new string[] { "You dont have the required permission to run " + GetUsage() };
		}
	}
}
