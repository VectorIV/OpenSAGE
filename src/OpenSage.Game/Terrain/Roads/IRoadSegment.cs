﻿using System.Collections.Generic;
using OpenSage.Graphics.Shaders;

namespace OpenSage.Terrain.Roads
{
    internal interface IRoadSegment
    {
        IEnumerable<RoadSegmentEndPoint> EndPoints { get; }

        void GenerateMesh(
            RoadTemplate template,
            HeightMap heightMap,
            List<RoadShaderResources.RoadVertex> vertices,
            List<ushort> indices);
    }
}
