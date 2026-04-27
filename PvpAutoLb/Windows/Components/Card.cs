using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace PvpAutoLb.Windows.Components;

internal static class Card
{
    public static IDisposable Begin(string id, float height, Vector4 background, Vector4 border, float borderSize = 1f)
    {
        var style = Styling.PushCardStyle();
        var bg = ImRaii.PushColor(ImGuiCol.ChildBg, background);
        var br = ImRaii.PushColor(ImGuiCol.Border, border);
        var sz = ImRaii.PushStyle(ImGuiStyleVar.ChildBorderSize, borderSize);
        var child = ImRaii.Child(id, new Vector2(-1, height), true,
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        return new CompositeDisposable(child, sz, br, bg, style);
    }

    private sealed class CompositeDisposable : IDisposable
    {
        private readonly IDisposable[] disposables;
        public CompositeDisposable(params IDisposable[] disposables) => this.disposables = disposables;
        public void Dispose()
        {
            for (var i = 0; i < disposables.Length; i++) disposables[i]?.Dispose();
        }
    }
}
