using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RPGCreationKit.SaveSystem
{
    [Serializable]
    public class MutableIDMutableDictionary : SerializableDictionary<string, MutableData> { }

    /// <summary>
    /// Represents a single quest data saved in the game
    /// </summary>
    [Serializable]
    public class MutableData
    {
        public bool isMutated;

        public MutableData(bool isMutated)
        {
            this.isMutated = isMutated;
        }
    }

    /// <summary>
    /// Represents and contains all the Quests saved in the game.
    /// </summary>
    [Serializable]
    public class MutablesData
    {
        public MutableIDMutableDictionary allMutables = new MutableIDMutableDictionary();
    }
}