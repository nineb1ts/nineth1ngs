using System.Windows;

namespace nineth1ngs.Services;

public static class MiniModeLayoutService
{
    public static Point ClampToWorkingArea(
        Point position,
        Size windowSize,
        Rect workingArea)
    {
        var maximumLeft = Math.Max(
            workingArea.Left,
            workingArea.Right - windowSize.Width);
        var maximumTop = Math.Max(
            workingArea.Top,
            workingArea.Bottom - windowSize.Height);

        return new Point(
            Math.Clamp(position.X, workingArea.Left, maximumLeft),
            Math.Clamp(position.Y, workingArea.Top, maximumTop));
    }

    public static Point SnapToWorkingArea(
        Point position,
        Size windowSize,
        Rect workingArea,
        double snapDistance = 12)
    {
        var clamped = ClampToWorkingArea(position, windowSize, workingArea);
        var left = clamped.X;
        var top = clamped.Y;

        if (Math.Abs(left - workingArea.Left) <= snapDistance)
        {
            left = workingArea.Left;
        }
        else if (Math.Abs(left + windowSize.Width - workingArea.Right) <= snapDistance)
        {
            left = workingArea.Right - windowSize.Width;
        }

        if (Math.Abs(top - workingArea.Top) <= snapDistance)
        {
            top = workingArea.Top;
        }
        else if (Math.Abs(top + windowSize.Height - workingArea.Bottom) <= snapDistance)
        {
            top = workingArea.Bottom - windowSize.Height;
        }

        return new Point(left, top);
    }

    public static Point GetQuickInputPosition(
        Rect miniBounds,
        Size inputSize,
        Rect workingArea,
        double gap = 4)
    {
        var left = Math.Clamp(
            miniBounds.Left,
            workingArea.Left,
            Math.Max(workingArea.Left, workingArea.Right - inputSize.Width));

        var below = miniBounds.Bottom + gap;
        var above = miniBounds.Top - gap - inputSize.Height;
        var top = below + inputSize.Height <= workingArea.Bottom
            ? below
            : above;

        top = Math.Clamp(
            top,
            workingArea.Top,
            Math.Max(workingArea.Top, workingArea.Bottom - inputSize.Height));

        return new Point(left, top);
    }
}