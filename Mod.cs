using Reloaded.Mod.Interfaces;
using nights.test.input.Template;
using nights.test.input.Configuration;
using Reloaded.Hooks.Definitions;
using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.Sources;
using nights.test.input.structs;
using Reloaded.Memory.Interop;

namespace nights.test.input;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
	/// <summary>
	/// Provides access to the mod loader API.
	/// </summary>
	private readonly IModLoader _modLoader;

	/// <summary>
	/// Provides access to the Reloaded.Hooks API.
	/// </summary>
	/// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
	private readonly IReloadedHooks _hooks;

	/// <summary>
	/// Provides access to the Reloaded logger.
	/// </summary>
	private readonly ILogger _logger;

	/// <summary>
	/// Entry point into the mod, instance that created this class.
	/// </summary>
	private readonly IMod _owner;

	/// <summary>
	/// Provides access to this mod's configuration.
	/// </summary>
	private Config _configuration;

	/// <summary>
	/// The configuration of the currently executing mod.
	/// </summary>
	private readonly IModConfig _modConfig;

	public Mod(ModContext context) {
		_modLoader = context.ModLoader;
		_hooks = context.Hooks;
		_logger = context.Logger;
		_owner = context.Owner;
		_configuration = context.Configuration;
		_modConfig = context.ModConfig;

		Globals.Hooks = _hooks;

		unsafe {
			// jump past code that hides cursor
			if (_configuration.ShowMouseCursor) {
				const byte JMP_REL8 = 0xEB;
				Memory.Instance.SafeWrite(0x40A88F, JMP_REL8);
			}

			// deadzone 1
			Memory.Instance.SafeWrite(
				0x712C58,
				_configuration.DiscardDeadzoneRadius *
				_configuration.DiscardDeadzoneRadius
				/ 10000.0
			);
			// deadzone 2
			// probably memory leak if mod is unloaded? oh no! I don't care
			var lerpedDeadzonePlus =
				new Pinnable<float>(_configuration.LerpedDeadzonePlus / 100f);
			Memory.Instance.SafeWrite(
				0x69CB33 + 0x2, (IntPtr)lerpedDeadzonePlus.Pointer
			);
			Memory.Instance.SafeWrite(
				0x69CBF6 + 0x2, (IntPtr)lerpedDeadzonePlus.Pointer
			);
			Memory.Instance.SafeWrite(
				0x69CC1B + 0x2, (IntPtr)lerpedDeadzonePlus.Pointer
			);
			Memory.Instance.SafeWrite(
				0x69CC3F + 0x2, (IntPtr)lerpedDeadzonePlus.Pointer
			);
			Memory.Instance.SafeWrite(
				0x69CC66 + 0x2, (IntPtr)lerpedDeadzonePlus.Pointer
			);
			// deadzone 3
			var playerDeadzoneManhattan = new Pinnable<double>(
				128.0 * _configuration.PlayerDeadzoneManhattan / 100.0
			);
			Memory.Instance.SafeWrite(
				0x4A21B7 + 0x2, (IntPtr)playerDeadzoneManhattan.Pointer
			);
			Memory.Instance.SafeWrite(
				0x55B307 + 0x2, (IntPtr)playerDeadzoneManhattan.Pointer
			);

			if (_configuration.VisitorPrecision) {
				GameInputAnyDirectionWrapper =
					_hooks.CreateWrapper<GameInputAnyDirection>(
						0x4A2240, out _
					);
				IncreaseSpeedHook = _hooks.CreateHook<IncreaseSpeed>(
					IncreaseSpeedImpl, 0x5705D0
				).Activate();

				if (_configuration.VisitorAnimation) {
					AnimationUpdateHook = _hooks.CreateHook<AnimationUpdate>(
						AnimationUpdateImpl, 0x47D180
					).Activate();
				}
			}
		}
	}

	[Function(CallingConventions.MicrosoftThiscall)]
	public unsafe delegate void IncreaseSpeed(PlayerSub* playerSub);
	public IHook<IncreaseSpeed> IncreaseSpeedHook;
	public unsafe void IncreaseSpeedImpl(PlayerSub* playerSub) {
		var Player = playerSub->Player;
		bool wantsToMove = GameInputAnyDirectionWrapper(Player);
		if (!wantsToMove) {
			return;
		}
		if (Player->Unknown59 == 0) {
			return;
		}
		const float MinSpeed = 0.2f;
		var speed = playerSub->Speed;
		if (Player->AnalogMovement != 0) {
			if (speed < MinSpeed) {
				playerSub->Speed = MinSpeed;
			}
			const float Acceleration = 0.000390625f;
			var increasedSpeed = playerSub->Speed + Acceleration;
			playerSub->Speed = increasedSpeed;

			float maxSpeed = 0.3f;
			// CUSTOM CODE
			const int leftXAction = 6;
			const int leftYAction = 7;
			sbyte leftXRaw =
				(*Globals.GameInput)->GetAction(leftXAction)->Analog;
			sbyte leftYRaw =
				(*Globals.GameInput)->GetAction(leftYAction)->Analog;
			float leftX = leftXRaw / 127f;
			float leftY = leftYRaw / 127f;
			float leftMagnitude =
				Math.Min((float)Math.Sqrt(leftX * leftX + leftY * leftY), 1f);
			var outer = _configuration.VisitorOuterDeadzone / 100f;
			if (leftMagnitude < outer) {
				var magnitudeLerped = leftMagnitude;
				if (_configuration.VisitorLerp) {
					magnitudeLerped /= outer;
				}
				maxSpeed = magnitudeLerped * MinSpeed;
				WalkAnimationSpeed = magnitudeLerped;
			} else {
				WalkAnimationSpeed = 1f;
			}
			// ORIGINAL CODE

			if (increasedSpeed > maxSpeed) {
				playerSub->Speed = maxSpeed;
			}
			return;
		}
		if (speed < MinSpeed) {
			const float Acceleration2 = 0.003125f;
			playerSub->Speed += Acceleration2;
		}
		if (playerSub->Speed >= MinSpeed) {
			const float Acceleration = 0.000390625f;
			var increasedSpeed = speed + Acceleration;
			playerSub->Speed = increasedSpeed;
			const float MaxSpeed = 0.3f;
			if (increasedSpeed > MaxSpeed) {
				playerSub->Speed = MaxSpeed;
			}
		}
		// CUSTOM CODE - DPad animation speed is not supported as out of scope
		WalkAnimationSpeed = 1f;
		return;
	}

	[Function(CallingConventions.Stdcall)]
	public unsafe delegate bool GameInputAnyDirection(Player* player);
	public GameInputAnyDirection GameInputAnyDirectionWrapper;

	[Function(CallingConventions.MicrosoftThiscall)]
	public unsafe delegate void AnimationUpdate(Animation* animation);
	public IHook<AnimationUpdate> AnimationUpdateHook;
	public static float WalkAnimationSpeed = 1f;
	public static float WalkAnimationAccumulator = 0f;
	public unsafe void AnimationUpdateImpl(Animation* animation) {
		if (animation->Frozen == 0) {
			var motion = animation->MotionCopy;
			if (motion->FrameAlt <= 0) {
				// CUSTOM CODE
				// if animation is one of many walking animations
				// (I will have probably missed some)
				if (
					// claris
					motion->Animation == 0xF
					|| motion->Animation == 0x10
					|| motion->Animation == 0x14
					|| motion->Animation == 0x15
					|| motion->Animation == 0x16
					|| motion->Animation == 0x18 // idk where this occurs
					|| motion->Animation == 0x19
					|| motion->Animation == 0x2C // swamp in mystic forest
					// elliot, is maybe missing claris' 0x18 equivalent?
					|| motion->Animation == 0x79
					|| motion->Animation == 0x7a
					|| motion->Animation == 0x7b
					|| motion->Animation == 0x7c
					|| motion->Animation == 0x7e
					|| motion->Animation == 0x7f
					|| motion->Animation == 0x92 // swamp in mystic forest
				) {
					WalkAnimationAccumulator += WalkAnimationSpeed;
					var incrementBy = (int)WalkAnimationAccumulator;
					motion->Frame += incrementBy;
					WalkAnimationAccumulator -= incrementBy;
				} else {
					// ORIGINAL CODE
					motion->Frame += motion->SpeedSometimes;
				}
			} else {
				if (motion->Unknown1298 == 3) {
					++motion->Frame;
				}
				if (--motion->FrameAlt == 0) {
					var jointArray = motion->JointArray;
					if (jointArray != null) {
						jointArray->Dtor(1);
						motion->JointArray = null;
					}
				}
			}
		}
	}

	#region Standard Overrides
	public override void ConfigurationUpdated(Config configuration)
	{
		// Apply settings from configuration.
		// ... your code here.
		_configuration = configuration;
		_logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
	}
	#endregion

	#region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public Mod() { }
#pragma warning restore CS8618
	#endregion
}
