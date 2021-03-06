﻿using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedField.Global

namespace Assets.Core.Data
{
    [Serializable]
    public class Localization
    {
        public string Culture;
        public KeyValuePair<string, string>[] Translations;
    }
}
