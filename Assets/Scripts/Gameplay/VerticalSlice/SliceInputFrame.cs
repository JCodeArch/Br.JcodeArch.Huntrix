using UnityEngine;
namespace HuntrX.Gameplay.VerticalSlice
{
    public enum GameplayControl { MoveLeft, MoveRight, MoveUp, MoveDown, Jump, Dash, Attack, Parry, SwitchCharacter, Interact, FoodSelf, FoodFan, Special, Pause }
    public interface IInputPreferenceSource
    {
        string KeyboardBinding(GameplayControl control);
        bool ToggleModeFor(GameplayControl control);
    }
    public interface IInputAimPreferenceSource { bool AssistedAim { get; } }
    public struct SliceInputFrame
    {
        public Vector2 Move;
        public bool JumpPressed, JumpReleased, Dash, Attack, Parry, Switch, Interact, InteractHeld, InteractReleased, FoodSelf, FoodFan, Special, Pause;
    }
}
