﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Terrain
{
    public sealed class Bridge : DisposableBase
    {
        private readonly Model _model;
        private readonly ModelInstance _modelInstance;
        private readonly List<Tuple<ModelMesh, Matrix4x4>> _meshes;

        internal Bridge(
            ContentManager contentManager,
            HeightMap heightMap,
            BridgeTemplate template,
            in Vector3 startPosition,
            in Vector3 endPosition)
        {
            const float heightBias = 1f;

            var modelPath = Path.Combine("Art", "W3D", template.BridgeModelName + ".W3D");
            _model = contentManager.Load<Model>(modelPath);

            _modelInstance = AddDisposable(_model.CreateInstance(contentManager.GraphicsDevice));

            var bridgeLeft = _model.Meshes.First(x => x.Name == "BRIDGE_LEFT");
            var bridgeSpan = _model.Meshes.First(x => x.Name == "BRIDGE_SPAN");
            var bridgeRight = _model.Meshes.First(x => x.Name == "BRIDGE_RIGHT");

            // See how many spans we can fit in.
            var lengthLeft = GetLength(bridgeLeft.BoundingBox) * template.BridgeScale;
            var lengthSpan = GetLength(bridgeSpan.BoundingBox) * template.BridgeScale;
            var lengthRight = GetLength(bridgeRight.BoundingBox) * template.BridgeScale;

            var startPositionWithHeight = startPosition;
            startPositionWithHeight.Z = heightMap.GetHeight(startPosition.X, startPosition.Y) + heightBias;

            var endPositionWithHeight = endPosition;
            endPositionWithHeight.Z = heightMap.GetHeight(endPosition.X, endPosition.Y) + heightBias;

            var distance = Vector3.Distance(startPosition, endPosition);

            var spanLength = distance - lengthLeft - lengthRight;

            // There is always at least one span in the middle of a bridge,
            // and more if space permits.
            var numSpans = Math.Max(1, (int) (spanLength / lengthSpan));

            var totalLength = lengthLeft + (lengthSpan * numSpans) + lengthRight;
            var scaleX = distance / totalLength;

            // Necessary, to set RelativeBoneTransforms.
            _modelInstance.Update(new GameTime());

            var rotationAroundZ = QuaternionUtility.CreateRotation(Vector3.UnitX, Vector3.Normalize(endPosition - startPosition));

            var inclineSkew = GetSkewMatrixForDifferentBridgeEndHeights(
                startPositionWithHeight,
                endPositionWithHeight,
                distance);

            var worldMatrix =
                Matrix4x4.CreateScale(template.BridgeScale) *
                inclineSkew *
                Matrix4x4.CreateFromQuaternion(rotationAroundZ) *
                Matrix4x4.CreateTranslation(startPositionWithHeight);

            _modelInstance.SetWorldMatrix(worldMatrix);

            _meshes = new List<Tuple<ModelMesh, Matrix4x4>>();

            var currentOffset = 0f;
            Matrix4x4 GetLocalTranslation(float w3dOffset)
            {
                return
                    Matrix4x4.CreateScale(scaleX, 1, 1) *
                    Matrix4x4.CreateTranslation(new Vector3(currentOffset, 0, 0)) *
                    Matrix4x4.CreateTranslation(new Vector3(-w3dOffset, 0, 0)) *
                    worldMatrix;
            }

            _meshes.Add(Tuple.Create(bridgeLeft, GetLocalTranslation(0)));
            currentOffset += lengthLeft;

            for (var i = 0; i < numSpans; i++)
            {
                _meshes.Add(Tuple.Create(bridgeSpan, GetLocalTranslation(lengthLeft)));
                currentOffset += lengthSpan;
            }

            _meshes.Add(Tuple.Create(bridgeRight, GetLocalTranslation(lengthLeft + lengthSpan)));
        }

        private static float GetLength(in BoundingBox box)
        {
            return box.Max.X - box.Min.X;
        }

        private static Matrix4x4 GetSkewMatrixForDifferentBridgeEndHeights(
            in Vector3 startPosition,
            in Vector3 endPosition,
            float horizontalDistance)
        {
            // We don't want to rotate the bridge to account for different heights at each end.
            // That would result in non-vertical bridge supports. So instead we skew it.
            var result = Matrix4x4.Identity;
            result.M13 = (endPosition.Z - startPosition.Z) / horizontalDistance;
            return result;
        }

        internal void BuildRenderList(RenderList renderList, Camera camera)
        {
            foreach (var meshMatrix in _meshes)
            {
                meshMatrix.Item1.BuildRenderListWithWorldMatrix(
                    renderList,
                    camera,
                    _modelInstance,
                    meshMatrix.Item2,
                    true);
            }
        }
    }
}
