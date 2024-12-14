//
//SpingManager.cs for unity-chan!
//
//Original Script is here:
//ricopin / SpingManager.cs
//Rocket Jump : http://rocketjump.skr.jp/unity3d/109/
//https://twitter.com/ricopin416
//
//Revised by N.Kobayashi 2014/06/24
//           Y.Ebata
//
using UnityEngine;
using System.Collections;

namespace UnityChan
{
	public class SpringManager : MonoBehaviour
	{
		//public SpringCollider springCollider;
		//Kobayashi
		// DynamicRatio is paramater for activated level of dynamic animation 
		public float dynamicRatio = 1.0f;

		//Ebata
		public float			stiffnessForce;
		public AnimationCurve	stiffnessCurve;
		public float			dragForce;
		public AnimationCurve	dragCurve;
		public SpringBone[] springBones;

		void Start ()
		{
			UpdateParameters ();
		}
	
		void Update ()
		{
#if UNITY_EDITOR
		//Kobayashi
		if(dynamicRatio >= 1.0f)
			dynamicRatio = 1.0f;
		else if(dynamicRatio <= 0.0f)
			dynamicRatio = 0.0f;
		//Ebata
		UpdateParameters();
#endif
		}
	
		private void LateUpdate ()
		{
			//Kobayashi
			if (dynamicRatio != 0.0f) {
				for (int i = 0; i < springBones.Length; i++) {
					if (dynamicRatio > springBones [i].threshold) {
						springBones [i].UpdateSpring ();
					}
				}
			}
		}
		
		//TEST: 自己添加的方法
		/*
		[ContextMenu("删除所有碰撞体")]
		public void DeleteColliders()
		{
			//获取子对象上的Transform对象
			Transform[] transforms = GetComponentsInChildren<Transform>();
			foreach (Transform o in transforms)
			{
				//若名称以"Q_"开头（裙子对象）或"D_"开头（后摆对象）
				if (o.name.StartsWith("Q_") || o.name.StartsWith("D_"))
				{
					//且有子对象
					if (o.childCount > 0)
					{
						//若没有碰撞体
						if (o.gameObject.GetComponent<SpringBone>().colliders.Length != 0)
						{
							o.gameObject.GetComponent<SpringBone>().colliders = null;
						}
					}
				}
			}
		}
		
		[ContextMenu("挂载碰撞体")]
		public void AddCollider()
		{
			//获取子对象上的Transform对象
			Transform[] transforms = GetComponentsInChildren<Transform>();
			SpringCollider[] springColliders = new SpringCollider[] { springCollider };
			foreach (Transform o in transforms)
			{
				//若名称以"Q_"开头（裙子对象）或"D_"开头（后摆对象）
				if (o.name.StartsWith("Q_") || o.name.StartsWith("D_"))
				{
					//且有子对象
					if (o.childCount > 0)
					{
						//若没有碰撞体
						if (o.gameObject.GetComponent<SpringBone>().colliders.Length == 0)
						{
							o.gameObject.GetComponent<SpringBone>().colliders = springColliders;
							//Debug.Log(o.name);
						}
					}
				}
			}
		}
		[ContextMenu("绑定骨骼")]
		public void GetEveryBones()
		{
			//获取子对象上的Transform对象
			Transform[] transforms = GetComponentsInChildren<Transform>();
			foreach (Transform o in transforms)
			{
				//若名称以"Q_"开头（裙子对象）或"D_"开头（后摆对象）
				if (o.name.StartsWith("Q_") || o.name.StartsWith("D_"))
				{
					//且有子对象
					if (o.childCount > 0)
					{
						o.gameObject.AddComponent<SpringBone>();
						o.gameObject.GetComponent<SpringBone>().child = o.GetChild(0);
						o.gameObject.GetComponent<SpringBone>().boneAxis = new Vector3 (0, 1, 0);
					}
				}
			}
			//SpringManager中获取所有添加了SpringBone组件的子对象
			springBones = GetComponentsInChildren<SpringBone>();
		}
		[ContextMenu("清除骨骼")]
		public void DeleteEveryBones()
		{
			//获取子对象上的Transform对象
			Transform[] transforms = GetComponentsInChildren<Transform>();
			foreach (Transform o in transforms)
			{
				//若名称以"Q_"开头（裙子对象）或"D_"开头（后摆对象）
				if (o.name.StartsWith("Q_") || o.name.StartsWith("D_"))
				{
					//且有子对象
					if (o.childCount > 0)
					{
						//删除其上挂载的SpringBone组件
						DestroyImmediate(o.gameObject.GetComponent<SpringBone>());
					}
				}
			}
		}
		*/
		
		private void UpdateParameters ()
		{
			UpdateParameter ("stiffnessForce", stiffnessForce, stiffnessCurve);
			UpdateParameter ("dragForce", dragForce, dragCurve);
		}
	
		private void UpdateParameter (string fieldName, float baseValue, AnimationCurve curve)
		{
			var start = curve.keys [0].time;
			var end = curve.keys [curve.length - 1].time;
			//var step	= (end - start) / (springBones.Length - 1);
		
			var prop = springBones [0].GetType ().GetField (fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
		
			for (int i = 0; i < springBones.Length; i++) {
				//Kobayashi
				if (!springBones [i].isUseEachBoneForceSettings) {
					var scale = curve.Evaluate (start + (end - start) * i / (springBones.Length - 1));
					prop.SetValue (springBones [i], baseValue * scale);
				}
			}
		}
	}
}