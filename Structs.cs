using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using System.Runtime.InteropServices;

// There might be some unneeded code in here?
// Right now this is just copied from my sandbox project, where I test stuff.

// I am not too sure how I plan on handling structs like this in the future.
// Would a shared library make sense?
// I'm scared that if I ever need to tweak a value I would break old mods.
// Additionally I don't know how to even link two C# libraries together, or
// how to compile a library from scratch either, or would it be from the mod
// template...
// My lack of knowledge is the wall here, so for now, I have all this.

namespace nights.test.input.structs;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct PlayerSub {
	[FieldOffset(0x84)]
	public int Unknown84;

	[FieldOffset(0xEC)]
	public Player* Player;

	[FieldOffset(0xF0)]
	public PlayerSubType Type;

	[FieldOffset(0xF4)]
	public int UnknownF4;

	[FieldOffset(0x114)]
	public float Speed;

	[FieldOffset(0x88)]
	public Animation* Animation;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Player {
	[FieldOffset(0x58)]
	public byte Unknown58;

	[FieldOffset(0x59)]
	public byte Unknown59;

	[FieldOffset(0x5A)]
	public byte AnalogMovement;

	[FieldOffset(0x5C)]
	public short Unknown5C;

	[FieldOffset(0x60)]
	public PlayerSub* PlayerSub;

	[FieldOffset(0x64)]
	private PlayerSub* _playerSubsBegin;
	public PlayerSub* GetPlayerSub(PlayerSubType type) {
		fixed (PlayerSub** _playerSubs = &_playerSubsBegin) {
			return _playerSubs[(int)type];
		}
	}

	[FieldOffset(0x80)]
	public int Unknown80;

	[FieldOffset(0x84)]
	public int Unknown84;

	[FieldOffset(0x88)]
	public int Unknown88;

	[FieldOffset(0x8C)]
	public int Unknown8C;

	[FieldOffset(0x90)]
	public int Unknown90;
}

public enum PlayerSubType {
	Nights,
	Elliot,
	Claris,
	ElliotTwinSeeds,
	ClarisTwinSeeds,
	OtherNightsWizemanFight
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct GameInput {
	[StructLayout(LayoutKind.Explicit)]
	public struct Action {
		[FieldOffset(0x0)]
		public int State;
		[FieldOffset(0x4)]
		public int Down;
		[FieldOffset(0x8)]
		public sbyte AnalogCopy;
		[FieldOffset(0x9)]
		public sbyte Analog;
		[FieldOffset(0xC)]
		public int FramesDown;
	};
	[FieldOffset(0x4)]
	private Action _actionsBegin;
	public Action* GetAction(int action) {
		fixed (Action* actions = &_actionsBegin) {
			return &actions[action];
		}
	}
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct WorldManager {
	[FieldOffset(0x50)]
	public Player* Player;
}

public unsafe struct Globals {
	public static unsafe WorldManager** WorldManager  = (WorldManager**)0x24C4EC4;
	public static unsafe GameInput**    GameInput     = (GameInput**)   0x24C4E88;

	public static IReloadedHooks Hooks;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Animation {
	[FieldOffset(0x18)]
	public byte Frozen;
	[FieldOffset(0x20)]
	public Motion* Motion;
	[FieldOffset(0x2C)]
	public Motion* MotionCopy;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Motion {
	[FieldOffset(0x14)]
	public int Animation;
	[FieldOffset(0x18)]
	public int Frame;
	[FieldOffset(0x116C)]
	public JointArray* JointArray;
	[FieldOffset(0x1290)]
	public int FrameAlt;
	[FieldOffset(0x1294)]
	public int Unknown1294;
	[FieldOffset(0x1298)]
	public int Unknown1298;
	[FieldOffset(0x12AC)]
	public int SpeedSometimes;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct JointArray {
	[StructLayout(LayoutKind.Explicit)]
	private unsafe struct VFTable {
		[FieldOffset(0x0)]
		public long Dtor;
	}

	[FieldOffset(0x0)]
	private VFTable* _vftable;

	[Function(CallingConventions.MicrosoftThiscall)]
	private unsafe delegate void DtorT(JointArray* self, int a1);

	public void Dtor(int a1) {
		var fn = Globals.Hooks.CreateWrapper<DtorT>(_vftable->Dtor, out _);
		fixed (JointArray* self = &this) {
			fn(self, a1);
		}
	}
}
