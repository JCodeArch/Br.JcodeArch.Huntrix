using System;
using HuntrX.Data;
namespace HuntrX.Gameplay.Narrative
{
    public interface INarrativePresentationPolicy
    {
        bool TryResolve(NarrativeSequenceDefinition sequence,int line,bool english,out string text);
        event Action PolicyChanged;
    }
}
