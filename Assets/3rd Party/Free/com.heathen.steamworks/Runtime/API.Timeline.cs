#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;

namespace Heathen.SteamworksIntegration.API
{
    public static class Timeline
    {
        public enum EventClipPriority : int
        {
            Invalid = 0,
            /// <summary>
            /// This event is not appropriate as a clip.
            /// </summary>
            None = 1,
            /// <summary>
            /// The user may want to make a clip around this event.
            /// </summary>
            Standard = 2,
            /// <summary>
            /// The player will be likely to want a clip around event, and those clips should be promoted more prominently than clips with the <see cref="Standard"/> priority
            /// </summary>
            Featured = 3,
        }

        public enum GameMode : int
        {
            Invalid = 0,
            /// <summary>
            /// The player is fully loaded into the game and playing.
            /// </summary>
            Playing = 1,
            /// <summary>
            /// The player is in a multiplayer lobby.
            /// </summary>
            Staging = 2,
            /// <summary>
            /// The player is in the game's main menu or a pause menu.
            /// </summary>
            Menus = 3,
            /// <summary>
            /// The player is waiting for a loading screen.
            /// </summary>
            LoadingScreen = 4,

            Max, // one past the last valid value
        }

        public static class Client
        {
            /// <summary>
            /// Sets a description for the current game state in the timeline. These help the user to find specific
            /// moments in the timeline when saving clips.Setting a new state description replaces any previous
            /// description.
            /// </summary>
            /// <param name="description">A localized string in the language returned by <see cref="Utilities.Client.SteamUILanguage"/></param>
            /// <param name="deltaTime">The time offset in seconds to apply to this state change. Negative times indicate an event that happened in the past.</param>
            public static void SetStateDescription(string description, float deltaTime) => SteamTimeline.SetTimelineStateDescription(description, deltaTime);

            /// <summary>
            /// Clears the current state description
            /// </summary>
            /// <param name="deltaTime">The time offset in seconds to apply to this state change. Negative times indicate an event that happened in the past.</param>
            public static void ClearStateDescription(float deltaTime) => SteamTimeline.ClearTimelineStateDescription(deltaTime);

            /// <summary>
            /// Records an event that will be displayed on the clip timeline via an icon with an optional suggestion for th possible clip
            /// </summary>
            /// <param name="iconName">The name of the icon to show at the timeline at this point. This can be one of the icons uploaded through the Steamworks partner Site for your title, or one of the provided icons that start with steam_.</param>
            /// <param name="eventTitle">Title-provided localized string in the language returned by <see cref="Utilities.Client.SteamUILanguage"/></param>
            /// <param name="eventDescription">Description-provided localized string in the language returned by <see cref="Utilities.Client.SteamUILanguage"/></param>
            /// <param name="priority">Provide the priority to use when the UI is deciding which icons to display in crowded parts of the timeline. Events with larger priority values will be displayed more prominently than events with smaller priority values.</param>
            /// <param name="deltaTime">The time offset in seconds to apply to the start of the event. Negative times indicate an event that happened in the past. 
            /// <para>One use of this parameter is to handle events whose significance is not clear until after the fact. For instance if the player starts a damage over time effect on another player, which kills them 3.5 seconds later, the game could pass -3.5 as the start offset and cause the event to appear in the timeline where the effect started.</para></param>
            /// <param name="duration">The duration of the event, in seconds. Pass 0 for instantaneous events.</param>
            /// <param name="possibleClip">Allows the game to describe events that should be suggested to the user as possible video clips.</param>
            public static void AddEvent(string iconName, string eventTitle, string eventDescription, uint priority, float deltaTime, float duration, EventClipPriority possibleClip) => SteamTimeline.AddTimelineEvent(iconName, eventTitle, eventDescription, priority, deltaTime, duration, (ETimelineEventClipPriority)possibleClip);

            /// <summary>
            /// Sets the mode that the game is in
            /// </summary>
            /// <param name="mode">The mode the game is in</param>
            public static void SetGameMode(GameMode mode) => SteamTimeline.SetTimelineGameMode((ETimelineGameMode)mode);
        }
    }
}
#endif