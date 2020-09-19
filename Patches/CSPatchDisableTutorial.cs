using System;
using System.Reflection;
using HarmonyLib;
using StoryMode;
using StoryMode.Behaviors;
using StoryMode.Behaviors.Quests.PlayerClanQuests;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;

namespace zCulturedStart
{
	// Token: 0x0200000C RID: 12
	[HarmonyPatch(typeof(MainStoryLine), "CompleteTutorialPhase")]
	internal class CSPatchDisableTutorial
	{
		// Token: 0x060000AA RID: 170 RVA: 0x0000563C File Offset: 0x0000383C
		private static bool Prefix(MainStoryLine __instance, bool isSkipped)
		{
			__instance.TutorialPhase.GetType().GetMethod("CompleteTutorial", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance.TutorialPhase, new object[]
			{
				isSkipped
			});
			CSPatchDisableTutorial.CSOnStoryModeEnded();
			__instance.GetType().GetProperty("FirstPhase").SetValue(__instance, new FirstPhase());
			CampaignEvents.RemoveListeners(Campaign.Current.GetCampaignBehavior<TutorialPhaseCampaignBehavior>());
			StoryModeEvents.RemoveListeners(Campaign.Current.GetCampaignBehavior<TutorialPhaseCampaignBehavior>());
			return false;
		}

		// Token: 0x060000AB RID: 171 RVA: 0x000056C4 File Offset: 0x000038C4
		private static void CSOnStoryModeEnded()
		{
			bool flag = CSCharCreationOption.CSGameOption == 2;
			if (flag)
			{
				Type type = typeof(RebuildPlayerClanQuestBehavior).Assembly.GetType("StoryMode.Behaviors.Quests.FirstPhase.RebuildPlayerClanQuestBehavior+RebuildPlayerClanQuest");
				bool flag2 = type != null;
				if (flag2)
				{
					QuestBase questBase = (QuestBase)Activator.CreateInstance(type, new object[]
					{
						Hero.MainHero
					});
					questBase.StartQuest();
					foreach (MobileParty obj in MobileParty.All)
					{
						Campaign.Current.VisualTrackerManager.RemoveTrackedObject(obj);
					}
				}
			}
			else
			{
				MbEvent mbEvent = (MbEvent)Traverse.Create(StoryModeEvents.Instance).Field("_onStoryModeTutorialEndedEvent").GetValue();
				mbEvent.Invoke();
			}
		}

		// Token: 0x0400003D RID: 61
		private readonly Type _rebuildPlayerClanQuest = typeof(RebuildPlayerClanQuestBehavior).GetNestedType("RebuildPlayerClanQuest", BindingFlags.NonPublic);
	}
}
