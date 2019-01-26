using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Packages.TP.Adoption
{
	public class ModifyParent : MonoBehaviour
	{
		[SerializeField] private Transform parent;
		[SerializeField] private bool worldPositionStays = true;

		[SerializeField, ReadOnly] private Transform biologicalParent;

		private SkinnedMeshRenderer skinnedMeshRenderer;
		
		private void Awake()
		{
			biologicalParent = Parent;
			Parent = parent;
			CallCPS();
		}

		private Transform Parent
		{
			get { return transform.parent; }
			set
			{
				if (value != null)
				{
					skinnedMeshRenderer = value.gameObject.GetComponent<SkinnedMeshRenderer>();
				}

				if (skinnedMeshRenderer)
				{
					transform.SetParent(skinnedMeshRenderer.rootBone, worldPositionStays);
				}
				else
				{
					transform.SetParent(value, worldPositionStays);
				}
			}
		}

		private void CallCPS()
		{
			if (Parent != biologicalParent)
			{
				GameObject ChildReference = new GameObject(transform.name + " (Adopted)");
				ChildReference.transform.parent = biologicalParent;
				ChildReference.transform.position = transform.position;
				ChildReference.transform.rotation = transform.rotation;
				CPS cps = ChildReference.AddComponent<CPS>();
				cps.ChildReference = this.gameObject;
			}
		}
	}
}
