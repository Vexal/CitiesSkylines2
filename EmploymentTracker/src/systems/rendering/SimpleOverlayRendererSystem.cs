using Colossal.Collections;
using Colossal.Mathematics;
using Game;
using Game.Prefabs;
using Game.Rendering;
using Game.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace EmploymentTracker
{
	public partial class SimpleOverlayRendererSystem  : GameSystemBase
	{
		public struct CurveData
		{
			public Matrix4x4 m_Matrix;

			public Matrix4x4 m_InverseMatrix;

			public Matrix4x4 m_Curve;

			public Color m_OutlineColor;

			public Color m_FillColor;

			public float2 m_Size;

			public float2 m_DashLengths;

			public float2 m_Roundness;

			public float m_OutlineWidth;

			public float m_FillStyle;
		}

		public struct BoundsData
		{
			public Bounds3 m_CurveBounds;
		}

		[Flags]
		public enum StyleFlags
		{
			Grid = 1,
			Projected = 2
		}

		public struct Buffer
		{
			private NativeList<CurveData> m_ProjectedCurves;

			private NativeList<CurveData> m_AbsoluteCurves;

			private NativeValue<BoundsData> m_Bounds;

			private float m_PositionY;

			private float m_ScaleY;

			public Buffer(NativeList<CurveData> projectedCurves, NativeList<CurveData> absoluteCurves, NativeValue<BoundsData> bounds, float positionY, float scaleY)
			{
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				m_ProjectedCurves = projectedCurves;
				m_AbsoluteCurves = absoluteCurves;
				m_Bounds = bounds;
				m_PositionY = positionY;
				m_ScaleY = scaleY;
			}

			public void DrawCurve(Color color, Bezier4x3 curve, float width, float2 roundness)
			{
				Bezier4x2 inp = curve.xz;
				float num = MathUtil.Length(ref inp);
				this.DrawCurveImpl(color, color, 0f, (StyleFlags)0, curve, width, num + width * 2f, 0f, roundness, num);
			}

			private void DrawCurveImpl(Color outlineColor, Color fillColor, float outlineWidth, StyleFlags styleFlags, Bezier4x3 curve, float width, float dashLength, float gapLength, float2 roundness, float length)
			{
				if (!(length < 0.01f))
				{
					CurveData value = default(CurveData);
					value.m_Size = new float2(width, length);
					value.m_DashLengths = new float2(gapLength, dashLength);
					value.m_Roundness = roundness;
					value.m_OutlineWidth = outlineWidth;
					value.m_FillStyle = (float)(styleFlags & StyleFlags.Grid);
					MathUtil.BuildCurveMatrix(ref curve, length, out value.m_Curve);
					value.m_OutlineColor = outlineColor.linear;
					value.m_FillColor = fillColor.linear;
					Bounds3 bounds;
					MathUtil.FitQuad(ref curve, width, out bounds, out value.m_Matrix);
					value.m_InverseMatrix = value.m_Matrix.inverse;
					m_AbsoluteCurves.Add(in value);

					BoundsData value2 = m_Bounds.value;
					value2.m_CurveBounds |= bounds;
					m_Bounds.value = value2;
				}
			}
		}

		private RenderingSystem m_RenderingSystem;

		private TerrainSystem m_TerrainSystem;

		private PrefabSystem m_PrefabSystem;

		private EntityQuery m_SettingsQuery;

		private Mesh m_BoxMesh;

		private Mesh m_QuadMesh;

		private Material m_ProjectedMaterial;

		private Material m_AbsoluteMaterial;

		private ComputeBuffer m_ArgsBuffer;

		private ComputeBuffer m_ProjectedBuffer;

		private ComputeBuffer m_AbsoluteBuffer;

		private List<uint> m_ArgsArray;

		private int m_ProjectedInstanceCount;

		private int m_AbsoluteInstanceCount;

		private int m_CurveBufferID;

		private int m_GradientScaleID;

		private int m_ScaleRatioAID;

		private int m_FaceDilateID;

		private NativeList<CurveData> m_ProjectedData;

		private NativeList<CurveData> m_AbsoluteData;

		private NativeValue<BoundsData> m_BoundsData;

		private JobHandle m_BufferWriters;

		[Preserve]
		protected override void OnCreate()
		{
			base.OnCreate();
			m_RenderingSystem = base.World.GetOrCreateSystemManaged<RenderingSystem>();
			m_TerrainSystem = base.World.GetOrCreateSystemManaged<TerrainSystem>();
			m_PrefabSystem = base.World.GetOrCreateSystemManaged<PrefabSystem>();
			m_SettingsQuery = GetEntityQuery(ComponentType.ReadOnly<OverlayConfigurationData>());
			m_CurveBufferID = Shader.PropertyToID("colossal_OverlayCurveBuffer");
			m_GradientScaleID = Shader.PropertyToID("_GradientScale");
			m_ScaleRatioAID = Shader.PropertyToID("_ScaleRatioA");
			m_FaceDilateID = Shader.PropertyToID("_FaceDilate");
			RenderPipelineManager.beginContextRendering += Render;
		}

		[Preserve]
		protected override void OnDestroy()
		{
			RenderPipelineManager.beginContextRendering -= Render;
			if (m_BoxMesh != null)
			{
				UnityEngine.Object.Destroy(m_BoxMesh);
			}

			if (m_QuadMesh != null)
			{
				UnityEngine.Object.Destroy(m_QuadMesh);
			}

			if (m_ProjectedMaterial != null)
			{
				UnityEngine.Object.Destroy(m_ProjectedMaterial);
			}

			if (m_AbsoluteMaterial != null)
			{
				UnityEngine.Object.Destroy(m_AbsoluteMaterial);
			}

			if (m_ArgsBuffer != null)
			{
				m_ArgsBuffer.Release();
			}

			if (m_ProjectedBuffer != null)
			{
				m_ProjectedBuffer.Release();
			}

			if (m_AbsoluteBuffer != null)
			{
				m_AbsoluteBuffer.Release();
			}

			if (m_ProjectedData.IsCreated)
			{
				m_ProjectedData.Dispose();
			}

			if (m_AbsoluteData.IsCreated)
			{
				m_AbsoluteData.Dispose();
			}

			if (m_BoundsData.IsCreated)
			{
				m_BoundsData.Dispose();
			}

			base.OnDestroy();
		}

		public Buffer GetBuffer(out JobHandle dependencies)
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			if (!m_ProjectedData.IsCreated)
			{
				m_ProjectedData = new NativeList<CurveData>(Allocator.Persistent);
			}

			if (!m_AbsoluteData.IsCreated)
			{
				m_AbsoluteData = new NativeList<CurveData>(Allocator.Persistent);
			}

			if (!m_BoundsData.IsCreated)
			{
				m_BoundsData = new NativeValue<BoundsData>(Allocator.Persistent);
			}

			dependencies = m_BufferWriters;
			return new Buffer(m_ProjectedData, m_AbsoluteData, m_BoundsData, m_TerrainSystem.heightScaleOffset.y - 50f, m_TerrainSystem.heightScaleOffset.x + 100f);
		}

		public void AddBufferWriter(JobHandle handle)
		{
			m_BufferWriters = JobHandle.CombineDependencies(m_BufferWriters, handle);
		}

		

		[Preserve]
		protected override void OnUpdate()
		{
			m_BufferWriters.Complete();
			m_BufferWriters = default(JobHandle);
			m_ProjectedInstanceCount = 0;
			m_AbsoluteInstanceCount = 0;
			if ((!m_ProjectedData.IsCreated || m_ProjectedData.Length == 0) && (!m_AbsoluteData.IsCreated || m_AbsoluteData.Length == 0))
			{
				return;
			}

			if (m_SettingsQuery.IsEmptyIgnoreFilter)
			{
				if (m_ProjectedData.IsCreated)
				{
					m_ProjectedData.Clear();
				}

				if (m_AbsoluteData.IsCreated)
				{
					m_AbsoluteData.Clear();
				}

				return;
			}

			if (m_ProjectedData.IsCreated && m_ProjectedData.Length != 0)
			{
				m_ProjectedInstanceCount = m_ProjectedData.Length;
				GetCurveMaterial(ref m_ProjectedMaterial, projected: true);
				GetCurveBuffer(ref m_ProjectedBuffer, m_ProjectedInstanceCount);
				m_ProjectedBuffer.SetData(m_ProjectedData.AsArray(), 0, 0, m_ProjectedInstanceCount);
				m_ProjectedMaterial.SetBuffer(m_CurveBufferID, m_ProjectedBuffer);
				m_ProjectedData.Clear();
			}

			if (m_AbsoluteData.IsCreated && m_AbsoluteData.Length != 0)
			{
				m_AbsoluteInstanceCount = m_AbsoluteData.Length;
				GetCurveMaterial(ref m_AbsoluteMaterial, projected: false);
				GetCurveBuffer(ref m_AbsoluteBuffer, m_AbsoluteInstanceCount);
				m_AbsoluteBuffer.SetData(m_AbsoluteData.AsArray(), 0, 0, m_AbsoluteInstanceCount);
				m_AbsoluteMaterial.SetBuffer(m_CurveBufferID, m_AbsoluteBuffer);
				m_AbsoluteData.Clear();
			}
		}

		private void Render(ScriptableRenderContext context, List<Camera> cameras)
		{
			try
			{
				if (m_RenderingSystem.hideOverlay)
				{
					return;
				}

				int num = 0;
				if (m_ProjectedInstanceCount != 0)
				{
					num += 5;
				}

				if (m_AbsoluteInstanceCount != 0)
				{
					num += 5;
				}

				if (num == 0)
				{
					return;
				}

				if (m_ArgsBuffer != null && m_ArgsBuffer.count < num)
				{
					m_ArgsBuffer.Release();
					m_ArgsBuffer = null;
				}

				if (m_ArgsBuffer == null)
				{
					m_ArgsBuffer = new ComputeBuffer(num, 4, ComputeBufferType.DrawIndirect);
					m_ArgsBuffer.name = "Overlay args buffer";
				}

				if (m_ArgsArray == null)
				{
					m_ArgsArray = new List<uint>();
				}

				m_ArgsArray.Clear();
				Bounds3 curveBounds = m_BoundsData.value.m_CurveBounds;
				MathUtil.ToBounds(ref curveBounds, out Bounds bounds);
				int num2 = 0;
				int num3 = 0;
				if (m_ProjectedInstanceCount != 0)
				{
					GetMesh(ref m_BoxMesh, box: true);
					GetCurveMaterial(ref m_ProjectedMaterial, projected: true);
					num2 = m_ArgsArray.Count;
					m_ArgsArray.Add(m_BoxMesh.GetIndexCount(0));
					m_ArgsArray.Add((uint)m_ProjectedInstanceCount);
					m_ArgsArray.Add(m_BoxMesh.GetIndexStart(0));
					m_ArgsArray.Add(m_BoxMesh.GetBaseVertex(0));
					m_ArgsArray.Add(0u);
				}

				if (m_AbsoluteInstanceCount != 0)
				{
					GetMesh(ref m_QuadMesh, box: false);
					GetCurveMaterial(ref m_AbsoluteMaterial, projected: false);
					num3 = m_ArgsArray.Count;
					m_ArgsArray.Add(m_QuadMesh.GetIndexCount(0));
					m_ArgsArray.Add((uint)m_AbsoluteInstanceCount);
					m_ArgsArray.Add(m_QuadMesh.GetIndexStart(0));
					m_ArgsArray.Add(m_QuadMesh.GetBaseVertex(0));
					m_ArgsArray.Add(0u);
				}

				foreach (Camera camera in cameras)
				{
					if (camera.cameraType == CameraType.Game || camera.cameraType == CameraType.SceneView)
					{
						if (m_ProjectedInstanceCount != 0)
						{
							Graphics.DrawMeshInstancedIndirect(m_BoxMesh, 0, m_ProjectedMaterial, bounds, m_ArgsBuffer, num2 * 4, null, ShadowCastingMode.Off, receiveShadows: false, 0, camera);
						}

						if (m_AbsoluteInstanceCount != 0)
						{
							Graphics.DrawMeshInstancedIndirect(m_QuadMesh, 0, m_AbsoluteMaterial, bounds, m_ArgsBuffer, num3 * 4, null, ShadowCastingMode.Off, receiveShadows: false, 0, camera);
						}
					}
				}

				m_ArgsBuffer.SetData(m_ArgsArray, 0, 0, m_ArgsArray.Count);
			}
			finally
			{
			}
		}

		private void GetMesh(ref Mesh mesh, bool box)
		{
			if (mesh == null)
			{
				mesh = new Mesh();
				mesh.name = "Overlay";
				if (box)
				{
					mesh.vertices = new Vector3[8]
					{
					new Vector3(-1f, 0f, -1f),
					new Vector3(-1f, 0f, 1f),
					new Vector3(1f, 0f, 1f),
					new Vector3(1f, 0f, -1f),
					new Vector3(-1f, 1f, -1f),
					new Vector3(-1f, 1f, 1f),
					new Vector3(1f, 1f, 1f),
					new Vector3(1f, 1f, -1f)
					};
					mesh.triangles = new int[36]
					{
					0, 1, 5, 5, 4, 0, 3, 7, 6, 6,
					2, 3, 0, 3, 2, 2, 1, 0, 4, 5,
					6, 6, 7, 4, 0, 4, 7, 7, 3, 0,
					1, 2, 6, 6, 5, 1
					};
				}
				else
				{
					mesh.vertices = new Vector3[4]
					{
					new Vector3(-1f, 0f, -1f),
					new Vector3(-1f, 0f, 1f),
					new Vector3(1f, 0f, 1f),
					new Vector3(1f, 0f, -1f)
					};
					mesh.triangles = new int[12]
					{
					0, 3, 2, 2, 1, 0, 0, 1, 2, 2,
					3, 0
					};
				}
			}
		}

		private void GetCurveMaterial(ref Material material, bool projected)
		{
			if (material == null)
			{
				OverlayConfigurationPrefab singletonPrefab = m_PrefabSystem.GetSingletonPrefab<OverlayConfigurationPrefab>(m_SettingsQuery);
				material = new Material(singletonPrefab.m_CurveMaterial);
				material.name = "Overlay curves";
				if (projected)
				{
					material.EnableKeyword("PROJECTED_MODE");
				}
			}
		}

		private unsafe void GetCurveBuffer(ref ComputeBuffer buffer, int count)
		{
			if (buffer != null && buffer.count < count)
			{
				count = math.max(buffer.count * 2, count);
				buffer.Release();
				buffer = null;
			}

			if (buffer == null)
			{
				buffer = new ComputeBuffer(math.max(64, count), sizeof(CurveData));
				buffer.name = "Overlay curve buffer";
			}
		}

		[Preserve]
		public SimpleOverlayRendererSystem()
		{
		}
	}
}
