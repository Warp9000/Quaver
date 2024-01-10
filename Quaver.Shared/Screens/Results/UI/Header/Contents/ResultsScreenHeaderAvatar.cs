using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
// using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
// using Steamworks;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Results.UI.Header.Contents
{
    public class ResultsScreenHeaderAvatar : Sprite
    {
        /// <summary>
        ///     The offset of the size/position. Also the size of the outer layer/border
        /// </summary>
        public static int OFFSET { get; } = 10;

        /// <summary>
        /// </summary>
        private SpriteAlphaMaskBlend Avatar { get; }

        public ResultsScreenHeaderAvatar(float size, Bindable<ScoreProcessor> processor)
        {
            Size = new ScalableVector2(size, size);
            Image = SkinManager.Skin?.Results?.ResultsAvatarBorder ?? UserInterface.ResultsAvatarBorder;

            var avatarSize = size - OFFSET * 2;

            Avatar = new SpriteAlphaMaskBlend()
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(avatarSize, avatarSize),
                Image = UserInterface.UnknownAvatar
            };

            // if (SteamManager.UserAvatars != null)
            // {
            //     var steamId = processor.Value.SteamId;

            //     if (ConfigManager.Username.Value == processor.Value.PlayerName)
            //         steamId = SteamUser.GetSteamID().m_SteamID;

            //     if (steamId != 0 && SteamManager.UserAvatarsLarge.ContainsKey(steamId))
            //     {
            //         var tex = SteamManager.UserAvatarsLarge[steamId];

            //         if (tex != UserInterface.UnknownAvatar && SteamManager.UserAvatarsLarge.ContainsKey(steamId))
            //             Avatar.Image = tex;
            //         else if (tex == UserInterface.UnknownAvatar && SteamManager.UserAvatars.ContainsKey(steamId))
            //             Avatar.Image = SteamManager.UserAvatars[steamId];
            //         else
            //             Avatar.Image = UserInterface.UnknownAvatar;
            //     }
            // }

            GameBase.Game.ScheduledRenderTargetDraws.Add(() =>
            {
                var mask = SkinManager.Skin?.Results?.ResultsAvatarMask ?? UserInterface.ResultsAvatarMask;
                Avatar.Image = Avatar.PerformBlend(Avatar.Image, mask);
            });
        }
    }
}