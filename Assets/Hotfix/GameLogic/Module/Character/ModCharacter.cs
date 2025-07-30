using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class CharacterData
    {
        public int GroupId { get; set; }
        public int Tid { get; set; }
        public int Star { get; set; }
        public int Name { get; set; }
    }

    [Model]
    public class ModCharacter : ModelTemplate
    {

    }
}