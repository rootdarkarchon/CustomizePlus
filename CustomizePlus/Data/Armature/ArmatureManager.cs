﻿// © Customize+.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using CustomizePlus.Data.Profile;
using CustomizePlus.Memory;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;

namespace CustomizePlus.Data.Armature
{
    public sealed class ArmatureManager
    {
        private readonly HashSet<Armature> _armatures = new();

        public void RenderCharacterProfiles(params CharacterProfile[] profiles)
        {
            RefreshActiveArmatures(profiles);
            RefreshArmatureVisibility();
            ApplyArmatureTransforms();
        }

        public unsafe void RenderArmatureByObject(GameObject obj)
        {
            if (_armatures.FirstOrDefault(x => x.ObjectRef == RenderObject.FromActor(obj)) is Armature arm &&
                arm != null)
            {
                if (arm.Visible)
                {
                    arm.ApplyTransformation();
                }
            }
        }

        private void RefreshActiveArmatures(params CharacterProfile[] profiles)
        {
            foreach (var prof in profiles)
            {
                if (!_armatures.Any(x => x.Profile == prof))
                {
                    var newArm = new Armature(prof);
                    _armatures.Add(newArm);
                    PluginLog.LogDebug($"Added '{newArm}' to cache");
                }
            }

            foreach (var arm in _armatures.Except(profiles.Select(x => x.Armature)))
            {
                if (arm != null)
                {
                    _armatures.Remove(arm);
                    PluginLog.LogDebug($"Removed '{arm}' from cache");
                }
            }
        }

        private void RefreshArmatureVisibility()
        {
            foreach (var arm in _armatures)
            {
                //TODO this is yucky
                arm.Visible = Plugin.ProfileManager.GetEnabledProfiles().Contains(arm.Profile) && arm.TryLinkSkeleton();
            }
        }

        private void ApplyArmatureTransforms()
        {
            foreach (var arm in _armatures.Where(x => x.Visible))
            {
                arm.ApplyTransformation();
            }
        }
    }
}