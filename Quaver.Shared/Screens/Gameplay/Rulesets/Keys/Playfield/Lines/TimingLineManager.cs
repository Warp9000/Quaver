/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Linq;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using System.Collections.Concurrent;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield.Lines
{
    public class TimingLineManager
    {
        /// <summary>
        ///     Timing Line object pool.
        /// </summary>
        private ConcurrentBag<TimingLine> Pool { get; set; }

        /// <summary>
        ///     Timing Line information. Generated by this class with qua object.
        /// </summary>
        // private Queue<TimingLineInfo> CachedInfo { get; set; }

        /// <summary>
        ///     Queue that timing line information is dequeued from. Resets to a copy of <see cref="Info"/> on object pool (re)initialization.
        /// </summary>
        // private Queue<TimingLineInfo> Info { get; set; }

		private SpatialHash<TimingLineInfo> SpatialHash { get; set; }

		private List<TimingLineInfo> RenderedLineInfos { get; set; }

        /// <summary>
        ///     Reference to the HitObjectManager
        /// </summary>
        public HitObjectManagerKeys HitObjectManager { get; }

        /// <summary>
        ///     Reference to the current Ruleset
        /// </summary>
        public GameplayRulesetKeys Ruleset { get; }

        /// <summary>
        ///     Initial size for the object pool
        /// </summary>
        private int InitialPoolSize { get; } = 6;

        /// <summary>
        ///     The Scroll Direction of every Timing Line
        /// </summary>
        public ScrollDirection ScrollDirection { get; }

        /// <summary>
        ///     Target position when TrackPosition = 0
        /// </summary>
        private float TrackOffset { get; }

        /// <summary>
        ///     Size of every Timing Line
        /// </summary>
        private float SizeX { get; }

        /// <summary>
        ///     Position of every Timing Line
        /// </summary>
        private float PositionX { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="map"></param>
        /// <param name="ruleset"></param>
        public TimingLineManager(GameplayRulesetKeys ruleset, ScrollDirection direction, float targetY, float size, float offset)
        {
            TrackOffset = targetY;
            SizeX = size;
            PositionX = offset;
            ScrollDirection = direction;
            Ruleset = ruleset;
            HitObjectManager = (HitObjectManagerKeys)ruleset.HitObjectManager;
            GenerateTimingLineInfo(ruleset.Map);
            InitializeObjectPool();
        }

        /// <summary>
        ///     Generate Timing Line Information for the map
        /// </summary>
        /// <param name="map"></param>
        public void GenerateTimingLineInfo(Qua map)
        {
            // List<TimingLineInfo> temp = new List<TimingLineInfo>();
			SpatialHash = new SpatialHash<TimingLineInfo>(HitObjectManager.RenderThreshold * 2);

            for (var i = 0; i < map.TimingPoints.Count; i++)
            {
                if (map.TimingPoints[i].Hidden)
                    continue;

                // Get target position and increment
                // Target position has tolerance of 1ms so timing points dont overlap by chance
                var target = i + 1 < map.TimingPoints.Count ? map.TimingPoints[i + 1].StartTime - 1 : map.Length;

                var signature = (int)map.TimingPoints[i].Signature;

                // Max possible sane value for timing lines
                const float maxBpm = 9999f;

                var msPerBeat = 60000 / Math.Min(Math.Abs(map.TimingPoints[i].Bpm), maxBpm);
                var increment = signature * msPerBeat;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (increment <= 0)
                    continue;

                // Initialize timing lines between current timing point and target position
                for (var songPos = map.TimingPoints[i].StartTime; songPos < target; songPos += increment)
                {
                    var offset = HitObjectManager.GetPositionFromTime(songPos);
					SpatialHash.Add(offset, new TimingLineInfo(songPos, offset));

                    // // Do not initialize any timing lines that do not appear in gameplay
                    // if (!(HitObjectManager.CurrentTrackPosition - offset > HitObjectManager.RecycleObjectPositionThreshold && songPos < HitObjectManager.CurrentAudioPosition))
                    //     temp.Add(new TimingLineInfo(songPos, offset));
                }
            }

            // Sort timing lines by position instead of time
            // CachedInfo = new Queue<TimingLineInfo>(temp.OrderBy(line => line.TrackOffset));
        }

        /// <summary>
        ///     Initialize the Timing Line Object Pool
        /// </summary>
        public void InitializeObjectPool()
        {
            Pool = new ConcurrentBag<TimingLine>();
			RenderedLineInfos = new List<TimingLineInfo>();

			for (int i = 0; i < InitialPoolSize; i++)
			{
				Pool.Add(new TimingLine(Ruleset, ScrollDirection, TrackOffset, SizeX, PositionX));
			}
        }

        /// <summary>
        ///     Update every object in the Timing Line Object Pool and create new objects if necessary
        /// </summary>
        public void UpdateObjectPool()
        {
            IEnumerable<TimingLineInfo> inRangeLines = SpatialHash.GetValues(HitObjectManager.CurrentTrackPosition - HitObjectManager.RenderThreshold);
			inRangeLines = inRangeLines.Concat(SpatialHash.GetValues(HitObjectManager.CurrentTrackPosition + HitObjectManager.RenderThreshold));
			inRangeLines = inRangeLines.Distinct().ToList();

			var outsideLines = RenderedLineInfos.Except(inRangeLines).ToList();
			foreach (var info in outsideLines)
			{
				Pool.Add(info.Unlink());
				RenderedLineInfos.Remove(info);
			}

			foreach (var info in inRangeLines)
			{
				if (info.Line != null)
					continue;

				TimingLine line;
				if (!Pool.TryTake(out line))
				{
					line = new TimingLine(Ruleset, ScrollDirection, TrackOffset, SizeX, PositionX);
				}

				info.Link(line);
				RenderedLineInfos.Add(info);
			}

			foreach (var info in RenderedLineInfos)
			{
				info.Line.UpdateSpritePosition(HitObjectManager.CurrentTrackPosition);
			}
        }

        /// <summary>
        ///     Create and add new Timing Line Object to the Object Pool
        /// </summary>
        /// <param name="info"></param>
        // private void CreatePoolObject(TimingLineInfo info)
        // {
        //     var line = new TimingLine(Ruleset, info, ScrollDirection, TrackOffset, SizeX, PositionX);
        //     line.UpdateSpritePosition(HitObjectManager.CurrentTrackPosition);
        //     Pool.Enqueue(line);
        // }
    }
}
