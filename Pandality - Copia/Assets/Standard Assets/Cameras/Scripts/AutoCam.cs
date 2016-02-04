using System;
using UnityEngine;
#if UNITY_EDITOR

#endif

namespace UnityStandardAssets.Cameras
{
    [ExecuteInEditMode]
    public class AutoCam : PivotBasedCameraRig
    {
        [SerializeField] private float m_MoveSpeed = 3; // How fast the rig will move to keep up with target's position
    	[SerializeField] private float VelocitaRotazione = 1f;	//velocità alla quale la telecamera si dovrà spostare


        private float m_LastFlatAngle; // The relative angle of the target and the rig from the previous frame.
        private float m_CurrentTurnAmount; // How much to turn the camera
        private float m_TurnSpeedVelocityChange; // The change in the turn speed velocity
    
		private short Offset = 1;	//offset usato per la rotazione
	

        protected override void FollowTarget(float deltaTime)
        {
            // if no target, or no time passed then we quit early, as there is nothing to do
            if (!(deltaTime > 0) || m_Target == null)
            {
                return;
            }

           
            // camera position moves towards target position:
            transform.position = Vector3.Lerp(transform.position, m_Target.position, deltaTime*m_MoveSpeed);


				if(!Input.GetKey("r") || !Input.GetKey("t"))	//controllo se l'utente sta premendo entrambi i tasti, in tal caso non faccio nulla
				{
					if(Offset >= 360)
					{Offset -=360;}
					else
					{
						if(Offset <= -360)
						{
							Offset+=360;
						}
					}

					if (Input.GetKey("r"))	//ruoto a sinistra
		            {
						//qui vado a dare a unity la posizione bersaglio che dovrà essere raggiunta alla fine della rotazione
						Quaternion To_Pos = Quaternion.Euler(0,transform.rotation.y + Offset,0);	
						
						//applico la rotazione
						transform.rotation = Quaternion.Slerp(transform.rotation, To_Pos, VelocitaRotazione / Time.deltaTime);

						//incremento l'offset
						Offset++;
							//	transform.Rotate(Vector3.up*Time.deltaTime*VelocitaRotazione);	--> versione facile
		            
					}

					if (Input.GetKey("t"))	//ruoto a destra
					{
						Quaternion To_Pos = Quaternion.Euler(0,transform.rotation.y + Offset,0);
								
								
						transform.rotation = Quaternion.Slerp(transform.rotation, To_Pos, VelocitaRotazione / Time.deltaTime);

						Offset--;			
						//transform.Rotate(Vector3.down*Time.deltaTime*VelocitaRotazione);	--> versione facile
					}
				}

        }
    }
}
