# Device Hotkeys (v3.1)

These options are included in the current [v3.1 release](https://github.com/sirWest/AudioSwitch/releases/tag/3.1).

In Settings > Hotkeys, add a shortcut with the action **Select audio devices**.
Choose either or both devices, leaving the other direction unchanged when needed.
Each selected device can keep its current mute state, be muted, or be unmuted.
You can also mute other devices in that category. With no device selected, that
option mutes all devices in the category without changing its default.

For example, a speakers shortcut can leave recording unchanged and mute all
microphones. A headset shortcut can select both headset endpoints and either
keep the microphone muted or explicitly unmute it. Keeping the current state
does not restore the state from before switching away.

The Playback and Recording tabs offer **Exclude from hotkey mute/unmute** for
devices that should never be muted or unmuted by shortcuts, including the toggle
mute shortcuts. Excluded devices can still be selected. Hidden devices participate
unless excluded. The hotkey editor's **Also change the communications default**
checkbox initially matches General settings; saving stores that choice for the hotkey.

Missing targets are skipped silently and retained by device ID for the next
press after reconnecting or resuming. Available members of a pair still work.
An attempted operation that fails is logged and reported with a Windows notification;
other pair members can still succeed. The flyout refreshes actual Windows state.
If an explicit target is missing, its category's mute-other-devices action is
also skipped. Changes apply when the shortcut is pressed; this is not a rule
that continuously mutes newly connected devices.
