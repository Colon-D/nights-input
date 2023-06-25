using System.ComponentModel;
using nights.test.input.Template.Configuration;

namespace nights.test.input.Configuration;

public class Config : Configurable<Config>
{
    /*
        User Properties:
            - Please put all of your configurable properties here.
    
        By default, configuration saves as "Config.json" in mod user config folder.    
        Need more config files/classes? See Configuration.cs
    
        Available Attributes:
        - Category
        - DisplayName
        - Description
        - DefaultValue

        // Technically Supported but not Useful
        - Browsable
        - Localizable

        The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
    */

    [Category("Deadzones")]
    [DisplayName("Discard Deadzone - Radius")]
    [Description("Radius that discards input inside of it.\n" +
        "Game's default: 30%. I recommend: 0%.\n" +
        "Value is the radius of a circle, with 0% being no circle, and 100% touching the edges.")]
    [DefaultValue(0.0)]
    public double DiscardDeadzoneRadius { get; set; } = 0.0;
	[Category("Deadzones")]
	[DisplayName("Lerped Deadzone - Plus")]
	[Description("Plus shape that lerps input outside of it.\n" +
        "Game's default: 24%. I recommend: 0%.\n" +
		"Value is the radius from an axis, with 0% being no radius, and 100% touching the edges.")]
	[DefaultValue(0f)]
	public float LerpedDeadzonePlus { get; set; } = 0f;
	[Category("Deadzones")]
	[DisplayName("Player's Deadzone - Diamond")]
	[Description("Diamond shape that discards input inside of it. Only applies to player.\n" +
        "Game's default: 62.5. I recommend: 25.\n" +
		"Value is the biggest radius in a diamond shape, ie along the X/Y axis, with 0% being no radius, and 100% touching the edges.")]
	[DefaultValue(25.0)]
	public double PlayerDeadzoneManhattan { get; set; } = 25.0;

	[Category("Visitor Precision")]
	[DisplayName("Enabled")]
	[Description("Allow Visitors to move slower with the Analog stick.\n" +
		"(Approximation of the Sega Saturn's Visitor movement)\n" +
		"(D-Pad is not supported)")]
	[DefaultValue(true)]
    public bool VisitorPrecision { get; set; } = true;

	[Category("Visitor Precision")]
	[DisplayName("Outer Deadzone")]
	[Description("Radius that applies maximum speed beyond it.\n" +
        "I recommend 90% as the game starts to accelerate you further when at maximum speed.\n" +
		"Value is the radius of a circle, with 0% being no circle, and 100% touching the edges.")]
	[DefaultValue(90f)]
	public float VisitorOuterDeadzone { get; set; } = 90f;

	[Category("Visitor Precision")]
	[DisplayName("Lerp Inner")]
	[Description("Linearly interpolate input up to Outer Deadzone.")]
	[DefaultValue(true)]
	public bool VisitorLerp { get; set; } = true;

	[Category("Visitor Precision")]
	[DisplayName("Animation")]
	[Description("Set Visitor's movement animations' speed to their speed. (Approximation)")]
	[DefaultValue(true)]
	public bool VisitorAnimation { get; set; } = true;

	[Category("Miscellaneous")]
	[DisplayName("Show Mouse Cursor")]
	[Description("Show the mouse cursor in-game.\n" +
		"Game's default: false"
	)]
	[DefaultValue(true)]
	public bool ShowMouseCursor { get; set; } = true;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}
