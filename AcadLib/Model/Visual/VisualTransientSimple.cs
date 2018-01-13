﻿using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace AcadLib.Visual
{
    public class VisualTransientSimple : VisualTransient
    {
        private readonly List<Entity> ents;

        public VisualTransientSimple(List<Entity> ents)
        {
            this.ents = ents;
        }

        public override List<Entity> CreateVisual()
        {
            return ents;
        }
    }
}
