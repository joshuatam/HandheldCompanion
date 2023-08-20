﻿using HandheldCompanion.Inputs;
using HandheldCompanion.Managers;
using HandheldCompanion.Utils;
using static JSL;

namespace HandheldCompanion.Controllers;

public class DualSenseController : JSController
{
    public DualSenseController()
    {
    }

    public DualSenseController(JOY_SETTINGS settings, PnPDetails details) : base(settings, details)
    {
        // Additional controller specific source buttons
        SourceButtons.Add(ButtonFlags.LeftPadClick);
        SourceButtons.Add(ButtonFlags.LeftPadTouch);
        SourceButtons.Add(ButtonFlags.RightPadTouch);

        SourceAxis.Add(AxisLayoutFlags.LeftPad);
        SourceAxis.Add(AxisLayoutFlags.RightPad);
        SourceAxis.Add(AxisLayoutFlags.Gyroscope);
    }

    public override void UpdateInputs(long ticks)
    {
        // skip if controller isn't connected
        if (!IsConnected())
            return;

        base.UpdateState();

        // Left Pad
        Inputs.ButtonState[ButtonFlags.LeftPadClick] = BitwiseUtils.HasByteSet(sTATE.buttons, ButtonMaskCapture);
        Inputs.ButtonState[ButtonFlags.LeftPadTouch] = JslGetTouchDown(UserIndex);

        if (Inputs.ButtonState[ButtonFlags.LeftPadTouch])
        {
            float joyShockX0 = JslGetTouchX(UserIndex);
            float joyShockY0 = JslGetTouchY(UserIndex);

            Inputs.AxisState[AxisFlags.LeftPadX] = (short)InputUtils.MapRange(joyShockX0, 0.0f, 1.0f, short.MinValue, short.MaxValue);
            Inputs.AxisState[AxisFlags.LeftPadY] = (short)InputUtils.MapRange(joyShockY0, 0.0f, 1.0f, short.MaxValue, short.MinValue);
        }
        else
        {
            Inputs.AxisState[AxisFlags.LeftPadX] = 0;
            Inputs.AxisState[AxisFlags.LeftPadY] = 0;
        }

        // Right Pad
        Inputs.ButtonState[ButtonFlags.RightPadTouch] = JslGetTouchDown(UserIndex, true);

        if (Inputs.ButtonState[ButtonFlags.RightPadTouch])
        {
            float joyShockX1 = JslGetTouchX(UserIndex, true);
            float joyShockY1 = JslGetTouchY(UserIndex, true);

            Inputs.AxisState[AxisFlags.RightPadX] = (short)InputUtils.MapRange(joyShockX1, 0.0f, 1.0f, short.MinValue, short.MaxValue);
            Inputs.AxisState[AxisFlags.RightPadY] = (short)InputUtils.MapRange(joyShockY1, 0.0f, 1.0f, short.MaxValue, short.MinValue);
        }
        else
        {
            Inputs.AxisState[AxisFlags.RightPadX] = 0;
            Inputs.AxisState[AxisFlags.RightPadY] = 0;
        }

        base.UpdateInputs(ticks);
    }

    public override void Plug()
    {
        TimerManager.Tick += UpdateInputs;
        base.Plug();
    }

    public override void Unplug()
    {
        TimerManager.Tick -= UpdateInputs;
        base.Unplug();
    }

    public override void Cleanup()
    {
        TimerManager.Tick -= UpdateInputs;
    }

    public override string GetGlyph(ButtonFlags button)
    {
        switch (button)
        {
            case ButtonFlags.B1:
                return "\u21E3"; // Cross
            case ButtonFlags.B2:
                return "\u21E2"; // Circle
            case ButtonFlags.B3:
                return "\u21E0"; // Square
            case ButtonFlags.B4:
                return "\u21E1"; // Triangle
            case ButtonFlags.L1:
                return "\u21B0";
            case ButtonFlags.R1:
                return "\u21B1";
            case ButtonFlags.Back:
                return "\u21E6";
            case ButtonFlags.Start:
                return "\u21E8";
            case ButtonFlags.L2Soft:
                return "\u21DC";
            case ButtonFlags.L2Full:
                return "\u21B2";
            case ButtonFlags.R2Soft:
                return "\u21DD";
            case ButtonFlags.R2Full:
                return "\u21B3";
            case ButtonFlags.Special:
                return "\uE000";
            case ButtonFlags.LeftPadClick:
            case ButtonFlags.RightPadClick:
                return "\u21E7";
        }

        return base.GetGlyph(button);
    }

    public override string GetGlyph(AxisFlags axis)
    {
        switch (axis)
        {
            case AxisFlags.L2:
                return "\u21B2";
            case AxisFlags.R2:
                return "\u21B3";
        }

        return base.GetGlyph(axis);
    }

    public override string GetGlyph(AxisLayoutFlags axis)
    {
        switch (axis)
        {
            case AxisLayoutFlags.L2:
                return "\u21B2";
            case AxisLayoutFlags.R2:
                return "\u21B3";
        }

        return base.GetGlyph(axis);
    }
}