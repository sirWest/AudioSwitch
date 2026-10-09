using System.Runtime.InteropServices;
using AudioSwitch.Core.Settings;
using AudioSwitch.Input;
using AudioSwitch.Shell;
using Xunit;

namespace AudioSwitch.Tests;

public sealed class ScrollModifierSuppressionTests
{
    [Theory]
    [InlineData(ScrollKeys.LWin, 0x5b)]
    [InlineData(ScrollKeys.RWin, 0x5c)]
    [InlineData(ScrollKeys.Alt, 0xa4)]
    [InlineData(ScrollKeys.Alt, 0xa5)]
    [InlineData(ScrollKeys.Control, 0xa2)]
    [InlineData(ScrollKeys.Control, 0xa3)]
    [InlineData(ScrollKeys.Shift, 0xa0)]
    [InlineData(ScrollKeys.Shift, 0xa1)]
    public void ScrollingMasksHeldModifierUntilReleaseThenAllowsOrdinaryPress(
        ScrollKeys modifier,
        int key
    )
    {
        var suppression = new ScrollModifierSuppression();
        Assert.False(suppression.ShouldSuppress(key, false));
        Assert.False(suppression.ShouldSuppress(key, true));

        Assert.True(suppression.MarkUsed(modifier, candidate => candidate == key));
        Assert.True(suppression.ShouldSuppress(key, false));
        Assert.True(suppression.ShouldSuppress(key, false));
        Assert.False(suppression.ShouldSuppress(0x41, false));
        Assert.False(suppression.ShouldSuppress(0x41, true));
        Assert.False(suppression.ShouldSuppress(key, true));
        Assert.False(suppression.ShouldSuppress(key, false));
    }

    [Fact]
    public void MultipleModifiersAndBothSidesReleaseIndependently()
    {
        var suppression = new ScrollModifierSuppression();
        int[] keys = [0xa4, 0xa5, 0x5b];
        Assert.True(suppression.MarkUsed(ScrollKeys.Alt | ScrollKeys.LWin, keys.Contains));
        Assert.True(suppression.MarkUsed(ScrollKeys.Alt | ScrollKeys.LWin, keys.Contains));

        Assert.False(suppression.ShouldSuppress(0xa4, true));
        Assert.False(suppression.ShouldSuppress(0xa4, false));
        Assert.True(suppression.ShouldSuppress(0xa5, false));
        Assert.True(suppression.ShouldSuppress(0x5b, false));
        Assert.False(suppression.ShouldSuppress(0xa5, true));
        Assert.False(suppression.ShouldSuppress(0x5b, true));
        Assert.False(suppression.ShouldSuppress(0xa5, false));
        Assert.False(suppression.ShouldSuppress(0x5b, false));
    }

    [Theory]
    [InlineData((ScrollKeys)0)]
    [InlineData(ScrollKeys.LeftMouseButton)]
    [InlineData(ScrollKeys.RightMouseButton)]
    public void MouseOnlyScrollingDoesNotMaskKeyboard(ScrollKeys modifiers)
    {
        var suppression = new ScrollModifierSuppression();
        Assert.False(suppression.MarkUsed(modifiers, _ => true));
        Assert.False(suppression.ShouldSuppress(0x5b, false));
        Assert.False(suppression.ShouldSuppress(0xa4, false));
    }

    [Fact]
    public void UnheldModifiersAreNotConsumedAndDisablingClearsSuppression()
    {
        var suppression = new ScrollModifierSuppression();
        Assert.False(suppression.MarkUsed(ScrollKeys.Alt, _ => false));
        Assert.False(suppression.ShouldSuppress(0xa4, false));
        Assert.True(suppression.MarkUsed(ScrollKeys.Alt, key => key == 0xa4));
        Assert.False(suppression.ShouldSuppress(0xa5, false));
        suppression.Clear();
        Assert.False(suppression.ShouldSuppress(0xa4, false));
    }

    [Fact]
    public void NativeInputLayoutMatchesWindowsAbi()
    {
        Assert.Equal(nint.Size == 8 ? 40 : 28, Marshal.SizeOf<Native.Input>());
        Assert.Equal(
            nint.Size == 8 ? 8 : 4,
            Marshal.OffsetOf<Native.Input>(nameof(Native.Input.Data)).ToInt32()
        );
        Assert.Equal(
            nint.Size == 8 ? 16 : 12,
            Marshal.OffsetOf<Native.KeyboardInput>(nameof(Native.KeyboardInput.ExtraInfo)).ToInt32()
        );
    }
}
