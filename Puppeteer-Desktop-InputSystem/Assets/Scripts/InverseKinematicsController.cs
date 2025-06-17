using UnityEngine;
using System;
using System.Collections;

// Based on: https://docs.unity3d.com/6000.1/Documentation/Manual/InverseKinematics.html
public class InverseKinematicsController : MonoBehaviour
{
    protected Animator animator;

    public bool ikActive = false;
    public Transform lookObj = null;
    public Transform leftHandObj = null;
    public Transform rightHandObj = null;
    public Transform leftFootObj = null;
    public Transform rightFootObj = null;

    void Start ()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void OnAnimatorIK()
    {
        // Debug.Log ("Animating IK " + animator);
        if(animator) {
       
            //if the IK is active, set the position and rotation directly to the goal.
            if(ikActive) {

                // Set the look target position, if one has been assigned
                if(lookObj != null) {
                    animator.SetLookAtWeight(1);
                    animator.SetLookAtPosition(lookObj.position);
                }

                if(rightHandObj != null) {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand,1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand,1);  
                    animator.SetIKPosition(AvatarIKGoal.RightHand,rightHandObj.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand,rightHandObj.rotation);
                }
                if(leftHandObj != null) {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand,1);  
                    animator.SetIKPosition(AvatarIKGoal.LeftHand,leftHandObj.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand,leftHandObj.rotation);
                }
                if(rightFootObj != null) {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot,1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot,1);  
                    animator.SetIKPosition(AvatarIKGoal.RightFoot,rightFootObj.position);
                    animator.SetIKRotation(AvatarIKGoal.RightFoot,rightFootObj.rotation);
                }
                if(leftFootObj != null) {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot,1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot,1);  
                    animator.SetIKPosition(AvatarIKGoal.LeftFoot,leftFootObj.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftFoot,leftFootObj.rotation);
                }
            }

            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else {          
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand,0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand,0);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand,0);
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot,0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot,0);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot,0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot,0);
                animator.SetLookAtWeight(0);
            }
        }
    }
}
