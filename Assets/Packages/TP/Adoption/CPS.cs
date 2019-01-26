using UnityEngine;

namespace Packages.TP.Adoption
{
    public class CPS : MonoBehaviour
    {
        [SerializeField, ReadOnly] private GameObject childReference;

        public GameObject ChildReference
        {
            get { return childReference; }
            set { childReference = value; }
        }
    }
}