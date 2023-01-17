﻿using Sirenix.OdinInspector;

namespace LccModel
{
    [Effect("行为禁制", 2)]
    public class ActionControlEffect : Effect
    {
        public override string Label => "行为禁制";

        [ToggleGroup("Enabled")]
        public ActionControlType ActionControlType;
    }
}