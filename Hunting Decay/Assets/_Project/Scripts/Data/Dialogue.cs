using System;
using System.Collections.Generic;
using UnityEngine;

namespace HuntingDecay
{
    [Serializable]
    public class DialogueLine
    {
        [Tooltip("Shown in the name tag. Leave empty for narration.")]
        public string speaker;
        [TextArea(2, 4)] public string text;

        public DialogueLine() { }

        public DialogueLine(string speaker, string text)
        {
            this.speaker = speaker;
            this.text = text;
        }
    }

    /// <summary>Breakfast always runs in this order, with the hunt request from the day appended.</summary>
    [Serializable]
    public class Breakfast
    {
        public List<DialogueLine> greeting = new();
        public List<DialogueLine> villageNews = new();
    }

    [Serializable]
    public class Dinner
    {
        public List<DialogueLine> question = new();
        [Tooltip("The Hunter's answers. They only add personality and never change the game.")]
        public List<Reply> replies = new();
    }

    [Serializable]
    public class Reply
    {
        public string text;
        public List<DialogueLine> reaction = new();
    }
}
