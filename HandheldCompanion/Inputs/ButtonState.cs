﻿using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace HandheldCompanion.Inputs;

[Serializable]
public partial class ButtonState : ICloneable
{
    public ConcurrentDictionary<ButtonFlags, bool> State = new();

    public ButtonState(ConcurrentDictionary<ButtonFlags, bool> State)
    {
        foreach (var state in State)
            this[state.Key] = state.Value;
    }

    public ButtonState()
    {
        foreach (ButtonFlags flags in Enum.GetValues(typeof(ButtonFlags)))
            State[flags] = false;
    }

    public bool this[ButtonFlags button]
    {
        get => State.ContainsKey(button) && State[button];

        set => State[button] = value;
    }

    [JsonIgnore]
    public IEnumerable<ButtonFlags> Buttons => State.Where(a => a.Value).Select(a => a.Key);

    public object Clone()
    {
        return new ButtonState(State);
    }

    public bool IsEmpty()
    {
        return !Buttons.Any();
    }

    public void Clear()
    {
        State.Clear();
    }

    public bool Contains(ButtonState buttonState)
    {
        return buttonState.State.All(state => this[state.Key] == state.Value);
    }

    public bool ContainsTrue(ButtonState buttonState)
    {
        if (IsEmpty() || buttonState.IsEmpty())
            return false;

        return buttonState.State.Where(a => a.Value).All(state => this[state.Key] == state.Value);
    }

    public void AddRange(ButtonState buttonState)
    {
        // only add pressed button
        foreach (KeyValuePair<ButtonFlags, bool> state in buttonState.State.Where(a => a.Value))
            this[state.Key] = state.Value;
    }

    public void Overwrite(ButtonState buttonState)
    {
        foreach (KeyValuePair<ButtonFlags, bool> state in State)
            buttonState[state.Key] = this[state.Key];
    }

    public override bool Equals(object obj)
    {
        if (obj is ButtonState buttonState)
            return Buttons.SequenceEqual(buttonState.Buttons);

        return false;
    }
}