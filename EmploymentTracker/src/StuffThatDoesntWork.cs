using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmploymentTracker
{
	public struct StuffThatDoesntWork
	{
		private void broken()
		{
			/*

			NativeArray<Entity> timerEntities = this.timerQuery.ToEntityArray(Allocator.Temp);
			this.timerQuery = GetEntityQuery(ComponentType.ReadWrite<DeleteTimer>());
			foreach (Entity e in timerEntities)
			{
				DeleteTimer timer = EntityManager.GetComponentData<DeleteTimer>(e);
				if (this.frameCount >= timer.endFrame)
				{
					switch (timer.componentType)
					{
						case ComponentTypeSelector.HIGHLIGHT:
							EntityManager.RemoveComponent<Highlighted>(e);
							EntityManager.AddComponent<BatchesUpdated>(e);
							break;
					}

					EntityManager.RemoveComponent<DeleteTimer>(e);
				}
			}
			 * 
			allQuery = this.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<Entity>());
            RequireForUpdate(allQuery);
			 */
			//Mod.log.Info("The entity count ? " +  this.allQuery.CalculateEntityCount());
			//var newColor = new Color(9, 1);
			// (Entity entity in this.allQuery.ToEntityArray(Allocator.Temp))
			{
				//EntityManager.AddComponentData<Color>(entity, newColor);
				//EntityManager.RemoveComponent<MeshColor>(entity);
				/*var color = EntityManager.AddBuffer<ColorVariation>(entity);
				ColorSet colorSet = new ColorSet();
				colorSet.m_Channel0 = new UnityEngine.Color(1, 0, 0, 1);
				colorSet.m_Channel1 = new UnityEngine.Color(100, 0, 0, 1);
				colorSet.m_Channel2 = new UnityEngine.Color(0, 0, 1000, 1);


				ColorVariation colorVariation = new ColorVariation();
				colorVariation.m_ColorSet = colorSet;
				if (color.Length > 0)
				{
					for (int i = 0; i < color.Length; ++i)
					{
						color[i] = colorVariation;
					}
				}
				//else
				{
					color.Add(colorVariation);
				}*/

				//EntityManager.AddComponent<Highlighted>(entity);

				/*	   DynamicBuffer<MeshColor> meshColors = EntityManager.AddBuffer<MeshColor>(entity);
					MeshColor meshColor = new MeshColor();
					meshColor.m_ColorSet = new ColorSet(UnityEngine.Color.red);
					meshColor.m_ColorSet.m_Channel0 = new UnityEngine.Color(1f, 0, 0);
					meshColor.m_ColorSet.m_Channel1 = new UnityEngine.Color(1f, 0, 0);
					meshColor.m_ColorSet.m_Channel2 = new UnityEngine.Color(1f, 0, 0);

					if (meshColors.Length > 0)
					{
						for (int i = 0; i < meshColors.Length; ++i)
						{
							meshColors[i] = meshColor;
						}
					}
					else
					{
						meshColors.Add(meshColor);
					}*/

				/*EntityManager.AddComponent<BatchesUpdated>(entity);
				EntityManager.AddComponent<EffectsUpdated>(entity);
				EntityManager.AddComponent<Updated>(entity);*/
			}
			/*EntityManager.AddBuffer<ColorVariation>(selected);

			EntityManager.AddComponent<BatchesUpdated>(selected);
			EntityManager.AddComponent<EffectsUpdated>(selected);
			EntityManager.AddComponent<Updated>(selected);*/
		}
	}
}
