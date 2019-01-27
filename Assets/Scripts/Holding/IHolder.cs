using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHolder
{
	void JointBroken(float breakForce);
    Transform JointAnchor { get; }
    Transform RightHandAttach { get; }
    Rigidbody Rigidbody { get; }
}
