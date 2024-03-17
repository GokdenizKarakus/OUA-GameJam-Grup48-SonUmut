using NUnit.Framework;
using System;
using UnityEditor;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace UnityEngine.Rendering.UnifiedRayTracing.Tests
{
    [TestFixture("Compute")]
    [TestFixture("Hardware")]
    public class AccelStructTests
    {
        RayTracingBackend m_Backend;
        RayTracingContext m_Context;
        IRayTracingAccelStruct m_AccelStruct;
        IRayTracingShader m_Shader;

        public AccelStructTests(string backendAsString)
        {
            m_Backend = Enum.Parse<RayTracingBackend>(backendAsString);
        }

        [SetUp]
        public void SetUp()
        {
            if (!SystemInfo.supportsRayTracing && m_Backend == RayTracingBackend.Hardware)
            {
                Assert.Ignore("Cannot run test on this Graphics API. Hardware RayTracing is not supported");
            }

            if (!SystemInfo.supportsComputeShaders && m_Backend == RayTracingBackend.Compute)
            {
                Assert.Ignore("Cannot run test on this Graphics API. Compute shaders are not supported");
            }

            CreateRayTracingResources();
        }

        [TearDown]
        public void TearDown()
        {
            DisposeRayTracingResources();
        }

        [Test]
        public void FrontOrBackFaceCulling()
        {
            const int instanceCount = 4;
            CreateMatchingRaysAndInstanceDescs(instanceCount, out RayWithFlags[] rays, out MeshInstanceDesc[] instanceDescs);

            var raysDuplicated = new RayWithFlags[instanceCount * 3];
            Array.Copy(rays, 0, raysDuplicated, 0, instanceCount);
            Array.Copy(rays, 0, raysDuplicated, instanceCount, instanceCount);
            Array.Copy(rays, 0, raysDuplicated, 2* instanceCount, instanceCount);

            for (int i = 0; i < instanceCount; ++i)
            {
                raysDuplicated[i].culling = (uint)RayCulling.None;
                raysDuplicated[i + instanceCount].culling = (uint)RayCulling.CullFrontFace;
                raysDuplicated[i + instanceCount * 2].culling = (uint)RayCulling.CullBackFace;
            }

            instanceDescs[0].enableTriangleCulling = false;
            instanceDescs[0].frontTriangleCounterClockwise = true;

            instanceDescs[1].enableTriangleCulling = false;
            instanceDescs[1].frontTriangleCounterClockwise = false;

            instanceDescs[2].enableTriangleCulling = true;
            instanceDescs[2].frontTriangleCounterClockwise = true;

            instanceDescs[3].enableTriangleCulling = true;
            instanceDescs[3].frontTriangleCounterClockwise = false;

            for (int i = 0; i < instanceCount; ++i)
            {
                m_AccelStruct.AddInstance(instanceDescs[i]);
            }

            var hits = TraceRays(raysDuplicated);

            // No culling
            Assert.IsTrue(hits[0].Valid());
            Assert.IsTrue(hits[1].Valid());
            Assert.IsTrue(hits[2].Valid());
            Assert.IsTrue(hits[3].Valid());

            // FrontFace culling
            Assert.IsTrue(hits[4].Valid());
            Assert.IsTrue(hits[5].Valid());
            Assert.IsTrue(hits[6].Valid());
            Assert.IsTrue(!hits[7].Valid());

            // BackFace culling
            Assert.IsTrue(hits[8].Valid());
            Assert.IsTrue(hits[9].Valid());
            Assert.IsTrue(!hits[10].Valid());
            Assert.IsTrue(hits[11].Valid());
        }


        [Test]
        public void InstanceAndRayMask()
        {
            const int instanceCount = 8;
            CreateMatchingRaysAndInstanceDescs(instanceCount, out RayWithFlags[] rays, out MeshInstanceDesc[] instanceDescs);

            var rayAndInstanceMasks = new (uint instanceMask, uint rayMask)[]
            {
                (0, 0),
                (0xFFFFFFFF, 0xFFFFFFFF),
                (0, 0xFFFFFFFF),
                (0xFFFFFFFF, 0),
                (0x0F, 0x01),
                (0x0F, 0xF0),
                (0x90, 0xF0),
                (0xF0, 0x10),
            };

            for (int i = 0; i < instanceCount; ++i)
            {
                instanceDescs[i].mask = rayAndInstanceMasks[i].instanceMask;
                rays[i].instanceMask = rayAndInstanceMasks[i].rayMask;
            }

            for (int i = 0; i < instanceCount; ++i)
            {
                m_AccelStruct.AddInstance(instanceDescs[i]);
            }

            var hits = TraceRays(rays);

            for (int i = 0; i < instanceCount; ++i)
            {
                bool rayShouldHit = ((rayAndInstanceMasks[i].instanceMask & rayAndInstanceMasks[i].rayMask) != 0);
                bool rayHit = hits[i].Valid();

                var message = String.Format("Ray {0} hit for InstanceMask: 0x{1:X} & RayMask: 0x{2:X}",
                    rayShouldHit ? "should" : "shouldn't",
                    rayAndInstanceMasks[i].instanceMask,
                    rayAndInstanceMasks[i].rayMask);

                Assert.AreEqual(rayShouldHit, rayHit, message);
            }
        }

        [Test]
        public void AddAndRemoveInstances()
        {
            const int instanceCount = 4;
            CreateMatchingRaysAndInstanceDescs(instanceCount, out RayWithFlags[] rays, out MeshInstanceDesc[] instanceDescs);

            var instanceHandles = new int[instanceCount];
            var expectedVisibleInstances = new bool[instanceCount];

            for (int i = 0; i < instanceCount; ++i)
            {
                instanceHandles[i] = m_AccelStruct.AddInstance(instanceDescs[i]);
                expectedVisibleInstances[i] = true;
            }

            CheckVisibleInstances(rays, expectedVisibleInstances);

            m_AccelStruct.RemoveInstance(instanceHandles[0]); expectedVisibleInstances[0] = false;
            m_AccelStruct.RemoveInstance(instanceHandles[2]); expectedVisibleInstances[2] = false;

            CheckVisibleInstances(rays, expectedVisibleInstances);

            m_AccelStruct.ClearInstances();

            Array.Fill(expectedVisibleInstances, false);

            CheckVisibleInstances(rays, expectedVisibleInstances);

            m_AccelStruct.AddInstance(instanceDescs[3]);
            expectedVisibleInstances[3] = true;

            CheckVisibleInstances(rays, expectedVisibleInstances);
        }

        void CheckVisibleInstances(RayWithFlags[] rays, bool[] expectedVisibleInstances)
        {
            var hits = TraceRays(rays);
            for (int i = 0; i < rays.Length; ++i)
            {
                Assert.AreEqual(hits[i].Valid(), expectedVisibleInstances[i]);
            }
        }

        void CreateMatchingRaysAndInstanceDescs(uint instanceCount, out RayWithFlags[] rays, out MeshInstanceDesc[] instanceDescs)
        {
            Mesh mesh = CreateSingleTriangleMesh();

            instanceDescs = new MeshInstanceDesc[instanceCount];
            rays = new RayWithFlags[instanceCount];
            var ray = new RayWithFlags(new float3(0.0f, 0.0f, 1.0f), new float3(0.0f, 0.0f, -1.0f));
            float3 step = new float3(2.0f, 0.0f, 0.0f);

            for (int i = 0; i < instanceCount; ++i)
            {
                instanceDescs[i] = new MeshInstanceDesc(mesh);
                instanceDescs[i].localToWorldMatrix = float4x4.Translate(step * i);

                rays[i] = ray;
                rays[i].origin += step * i;
            }
        }

        Hit[] TraceRays(RayWithFlags[] rays)
        {
            var computeBufferTarget = GraphicsBuffer.Target.CopyDestination | GraphicsBuffer.Target.CopySource
                | GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.Raw;

            var rayCount = rays.Length;
            using var raysBuffer = new GraphicsBuffer(computeBufferTarget, rayCount, Marshal.SizeOf<RayWithFlags>());
            raysBuffer.SetData(rays);
            using var hitsBuffer = new GraphicsBuffer(computeBufferTarget, rayCount, Marshal.SizeOf<Hit>());

            using var scratchBuffer = RayTracingHelper.CreateScratchBufferForBuildAndDispatch(m_AccelStruct, m_Shader, (uint)rayCount, 1, 1);

            var cmd = new CommandBuffer();
            m_AccelStruct.Build(cmd, scratchBuffer);
            m_Shader.SetAccelerationStructure(cmd, "_AccelStruct", m_AccelStruct);
            m_Shader.SetBufferParam(cmd, Shader.PropertyToID("_Rays"), raysBuffer);
            m_Shader.SetBufferParam(cmd, Shader.PropertyToID("_Hits"), hitsBuffer);
            m_Shader.Dispatch(cmd, scratchBuffer, (uint)rayCount, 1, 1);
            Graphics.ExecuteCommandBuffer(cmd);

            var hits = new Hit[rayCount];
            hitsBuffer.GetData(hits);

            return hits;
        }

        void CreateRayTracingResources()
        {
            var resources = ScriptableObject.CreateInstance<RayTracingResources>();
            ResourceReloader.ReloadAllNullIn(resources, "Packages/com.unity.rendering.light-transport");
            Func<string, Type, Object> fileLoader = (filename, type) => AssetDatabase.LoadAssetAtPath("Packages/com.unity.rendering.light-transport/" + filename, type);

            m_Context = new RayTracingContext(m_Backend, resources);
            m_AccelStruct = m_Context.CreateAccelerationStructure(new AccelerationStructureOptions());
            m_Shader = m_Context.CreateRayTracingShader("Tests/Editor/UnifiedRayTracing/TraceRays", fileLoader);
        }

        void DisposeRayTracingResources()
        {
            m_AccelStruct?.Dispose();
            m_Context?.Dispose();
        }

        private Mesh CreateSingleTriangleMesh()
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(1.0f, -0.5f, 0),
                new Vector3(-0.5f, 1.0f, 0)
            };
            mesh.vertices = vertices;

            Vector3[] normals = new Vector3[]
            {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
            };
            mesh.normals = normals;

            Vector2[] uv = new Vector2[]
            {
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(0, 0)
            };
            mesh.uv = uv;

            int[] tris = new int[3]
            {
                0, 2, 1
            };
            mesh.triangles = tris;

            return mesh;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RayWithFlags
        {
            public float3 origin;
            public float minT;
            public float3 direction;
            public float maxT;
            public uint culling;
            public uint instanceMask;
            uint padding;
            uint padding2;

            public RayWithFlags(float3 origin, float3 direction)
            {
                this.origin = origin;
                this.direction = direction;
                minT = 0.0f;
                maxT = float.MaxValue;
                instanceMask = 0xFFFFFFFF;
                culling = 0;
                padding = 0;
                padding2 = 0;
            }
        }

        [System.Flags]
        enum RayCulling { None = 0, CullFrontFace = 0x10, CullBackFace = 0x20 }

        [StructLayout(LayoutKind.Sequential)]
        public struct Hit
        {
            public uint instanceIndex;
            public uint primitiveIndex;
            public float2 uvBarycentrics;
            public float hitDistance;
            public uint isFrontFace;

            public bool Valid() { return instanceIndex != 0xFFFFFFFF; }
        }
    }
}
