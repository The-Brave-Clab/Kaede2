namespace Kaede2.Input
{
    public enum InputDeviceType
    {
        KeyboardAndMouse,
        Touchscreen,

        // TODO: we might be able to further differentiate between different gamepads
        XboxController, // Xbox One? Xbox Elite? Xbox 360?
        PlayStationController, // DualShock 4? DualSense? DualSense Edge?
        NintendoController, // Single Joy-Con? Dual Joy-Con? Pro Controller?

        GeneralGamepad,
    }
}